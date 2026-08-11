using System.Text.Json;
using SmartCourt.Features.Milestones.Integration;
using SmartCourt.Features.Notifications.Enums;
using SmartCourt.Infrastructure.Persistence.Entities;
using SmartCourt.Infrastructure.Providers.Events;

namespace SmartCourt.Features.Notifications.Events;

internal sealed class MilestoneNotificationEventMapper(
    IMilestoneNotificationContextReader contextReader)
    : INotificationEventMapper
{
    private static readonly JsonSerializerOptions SerializerOptions = new(
        JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true
    };

    public IReadOnlyCollection<string> EventTypes =>
    [
        ContractPaymentEventTypes.MilestoneCreated,
        ContractPaymentEventTypes.MilestoneDraftUpdated,
        ContractPaymentEventTypes.MilestoneAcceptanceRecorded,
        ContractPaymentEventTypes.MilestoneApproved,
        ContractPaymentEventTypes.MilestoneReadyForFunding,
        ContractPaymentEventTypes.MilestoneSubmitted,
        ContractPaymentEventTypes.MilestoneChangesRequested,
        ContractPaymentEventTypes.MilestoneAccepted,
        ContractPaymentEventTypes.MilestoneAutoAccepted,
        ContractPaymentEventTypes.MilestoneChangeRequestCreated,
        ContractPaymentEventTypes.MilestoneChangeRequestApproved,
        ContractPaymentEventTypes.MilestoneChangeRequestRejected,
        ContractPaymentEventTypes.MilestoneChangeRequestCancelled
    ];

    public async Task<IReadOnlyCollection<NotificationDraft>> MapAsync(
        OutboxMessage message,
        CancellationToken cancellationToken)
    {
        EnsureVersion(message, 1);
        if (IsChangeRequestEvent(message.EventType))
        {
            return await MapChangeRequestAsync(message, cancellationToken);
        }

        var payload = ReadMilestonePayload(message);
        var context = await contextReader.GetMilestoneAsync(
            payload.MilestoneId,
            cancellationToken);
        EnsureAggregate(message, context.MilestoneId);
        var data = Data(context);

        return message.EventType switch
        {
            ContractPaymentEventTypes.MilestoneCreated =>
                ToCounterparty(
                    RequireParticipant(payload.ActorUserId, context),
                    context,
                    "milestone.created",
                    NotificationSeverity.Information,
                    "مرحلة تعاقدية جديدة",
                    "أضاف الطرف الآخر مرحلة جديدة إلى العقد لتراجع شروطها.",
                    data),
            ContractPaymentEventTypes.MilestoneDraftUpdated =>
                ToCounterparty(
                    RequireParticipant(payload.ActorUserId, context),
                    context,
                    "milestone.draft-updated",
                    NotificationSeverity.Warning,
                    "تم تحديث المرحلة",
                    "تم تحديث شروط المرحلة، وتحتاج النسخة الحالية إلى مراجعتك وموافقتك.",
                    data),
            ContractPaymentEventTypes.MilestoneAcceptanceRecorded =>
                ToCounterparty(
                    RequireParticipant(payload.ActorUserId, context),
                    context,
                    "milestone.acceptance-recorded",
                    NotificationSeverity.Information,
                    "موافقة جديدة على المرحلة",
                    "وافق الطرف الآخر على شروط المرحلة الحالية، والمرحلة بانتظار موافقتك.",
                    data),
            ContractPaymentEventTypes.MilestoneApproved =>
                BothParticipants(
                    context,
                    "milestone.approved",
                    NotificationSeverity.Success,
                    "تم اعتماد المرحلة",
                    "وافق طرفا العقد على شروط المرحلة وأصبحت جاهزة للانتقال إلى التمويل.",
                    data),
            ContractPaymentEventTypes.MilestoneReadyForFunding =>
            [
                Draft(
                    context.ClientUserId,
                    "milestone.ready-for-funding",
                    NotificationSeverity.Information,
                    "المرحلة جاهزة للتمويل",
                    "أصبحت المرحلة جاهزة للتمويل حتى يتمكن المحامي من بدء العمل.",
                    data)
            ],
            ContractPaymentEventTypes.MilestoneSubmitted =>
            [
                Draft(
                    context.ClientUserId,
                    "milestone.submitted",
                    NotificationSeverity.Information,
                    "تم تسليم أعمال المرحلة",
                    "سلّم المحامي أعمال المرحلة، ويمكنك الآن مراجعتها وقبولها أو طلب تعديلات.",
                    data)
            ],
            ContractPaymentEventTypes.MilestoneChangesRequested =>
            [
                Draft(
                    context.LawyerUserId,
                    "milestone.changes-requested",
                    NotificationSeverity.Warning,
                    "طُلبت تعديلات على المرحلة",
                    "طلب العميل تعديلات على أعمال المرحلة، ويمكنك مراجعة الطلب وإعادة التسليم.",
                    data)
            ],
            ContractPaymentEventTypes.MilestoneAccepted =>
            [
                Draft(
                    context.LawyerUserId,
                    "milestone.accepted",
                    NotificationSeverity.Success,
                    "تم قبول أعمال المرحلة",
                    "قبل العميل أعمال المرحلة، وبدأت مدة حجز المبلغ قبل إتاحته للصرف.",
                    data)
            ],
            ContractPaymentEventTypes.MilestoneAutoAccepted =>
            [
                Draft(
                    context.ClientUserId,
                    "milestone.auto-accepted",
                    NotificationSeverity.Warning,
                    "تم قبول المرحلة تلقائيًا",
                    "انتهت مدة المراجعة وقُبلت أعمال المرحلة تلقائيًا، وبدأت مدة الاعتراض.",
                    data),
                Draft(
                    context.LawyerUserId,
                    "milestone.auto-accepted",
                    NotificationSeverity.Success,
                    "تم قبول المرحلة تلقائيًا",
                    "قُبلت أعمال المرحلة تلقائيًا بعد انتهاء مدة المراجعة، وبدأت مدة حجز المبلغ.",
                    data)
            ],
            _ => throw Unsupported(message.EventType)
        };
    }

    private async Task<IReadOnlyCollection<NotificationDraft>>
        MapChangeRequestAsync(
            OutboxMessage message,
            CancellationToken cancellationToken)
    {
        var payload = Deserialize<MilestoneChangeRequestEventPayload>(message);
        EnsureIdentifier(payload.MilestoneId, "milestone");
        EnsureIdentifier(payload.ChangeRequestId, "change request");
        EnsureAggregate(message, payload.ChangeRequestId);
        EnsureChangeRequestStatus(message.EventType, payload.Status);

        var context = await contextReader.GetChangeRequestAsync(
            payload.ChangeRequestId,
            cancellationToken);
        if (context.MilestoneId != payload.MilestoneId)
        {
            throw new InvalidOperationException(
                "Milestone change-request payload and context identifiers do not match.");
        }

        var requester = RequireParticipant(
            context.RequestedByUserId,
            context.ClientUserId,
            context.LawyerUserId,
            "Milestone change-request requester is not a contract participant.");
        var data = Data(context);
        return message.EventType switch
        {
            ContractPaymentEventTypes.MilestoneChangeRequestCreated =>
                ToCounterparty(
                    requester,
                    context.ClientUserId,
                    context.LawyerUserId,
                    "milestone.change-request-created",
                    NotificationSeverity.Information,
                    "طلب تعديل جديد للمرحلة",
                    "أنشأ الطرف الآخر طلبًا لتعديل شروط المرحلة، ويحتاج الطلب إلى قرارك.",
                    data),
            ContractPaymentEventTypes.MilestoneChangeRequestApproved =>
            [
                Draft(
                    requester,
                    "milestone.change-request-approved",
                    NotificationSeverity.Success,
                    "تمت الموافقة على طلب التعديل",
                    "وافق الطرف الآخر على طلب تعديل المرحلة، وطُبّقت الشروط المعتمدة.",
                    data)
            ],
            ContractPaymentEventTypes.MilestoneChangeRequestRejected =>
            [
                Draft(
                    requester,
                    "milestone.change-request-rejected",
                    NotificationSeverity.Warning,
                    "تم رفض طلب تعديل المرحلة",
                    "رفض الطرف الآخر طلب تعديل المرحلة. يمكنك مراجعة الطلب لمعرفة التفاصيل.",
                    data)
            ],
            ContractPaymentEventTypes.MilestoneChangeRequestCancelled =>
                ToCounterparty(
                    requester,
                    context.ClientUserId,
                    context.LawyerUserId,
                    "milestone.change-request-cancelled",
                    NotificationSeverity.Information,
                    "تم إلغاء طلب تعديل المرحلة",
                    "ألغى الطرف الآخر طلب تعديل المرحلة، ولم يعد القرار مطلوبًا منك.",
                    data),
            _ => throw Unsupported(message.EventType)
        };
    }

    private static MilestoneEventPayload ReadMilestonePayload(
        OutboxMessage message)
    {
        return message.EventType switch
        {
            ContractPaymentEventTypes.MilestoneCreated
                or ContractPaymentEventTypes.MilestoneDraftUpdated
                or ContractPaymentEventTypes.MilestoneAcceptanceRecorded =>
                ReadParticipant(message),
            ContractPaymentEventTypes.MilestoneApproved
                or ContractPaymentEventTypes.MilestoneReadyForFunding
                or ContractPaymentEventTypes.MilestoneChangesRequested =>
                ReadAggregate(message),
            ContractPaymentEventTypes.MilestoneSubmitted =>
                ReadSubmission(message),
            ContractPaymentEventTypes.MilestoneAccepted =>
                ReadAcceptance(message),
            ContractPaymentEventTypes.MilestoneAutoAccepted =>
                ReadAutoAcceptance(message),
            _ => throw Unsupported(message.EventType)
        };
    }

    private static MilestoneEventPayload ReadParticipant(OutboxMessage message)
    {
        var payload = Deserialize<MilestoneParticipantEventPayload>(message);
        EnsureIdentifier(payload.MilestoneId, "milestone");
        EnsureIdentifier(payload.ActorUserId, "actor");
        EnsureAggregate(message, payload.MilestoneId);
        return new MilestoneEventPayload(
            payload.MilestoneId,
            payload.ActorUserId);
    }

    private static MilestoneEventPayload ReadAggregate(OutboxMessage message)
    {
        var payload = Deserialize<ContractPaymentAggregateEventPayload>(message);
        EnsureIdentifier(payload.EntityId, "milestone");
        EnsureAggregate(message, payload.EntityId);
        return new MilestoneEventPayload(payload.EntityId, null);
    }

    private static MilestoneEventPayload ReadSubmission(OutboxMessage message)
    {
        var payload = Deserialize<MilestoneSubmissionEventPayload>(message);
        EnsureIdentifier(payload.MilestoneId, "milestone");
        EnsureIdentifier(payload.EscrowHoldId, "escrow hold");
        if (payload.SubmissionVersion <= 0)
        {
            throw new InvalidOperationException(
                "Milestone submission version is invalid.");
        }
        EnsureAggregate(message, payload.MilestoneId);
        return new MilestoneEventPayload(payload.MilestoneId, null);
    }

    private static MilestoneEventPayload ReadAcceptance(OutboxMessage message)
    {
        var payload = Deserialize<MilestoneAcceptanceEventPayload>(message);
        EnsureIdentifier(payload.MilestoneId, "milestone");
        EnsureIdentifier(payload.EscrowHoldId, "escrow hold");
        EnsureAggregate(message, payload.MilestoneId);
        return new MilestoneEventPayload(payload.MilestoneId, null);
    }

    private static MilestoneEventPayload ReadAutoAcceptance(
        OutboxMessage message)
    {
        var payload = Deserialize<MilestoneAutoAcceptedEventPayload>(message);
        EnsureIdentifier(payload.MilestoneId, "milestone");
        EnsureIdentifier(payload.EscrowHoldId, "escrow hold");
        if (payload.SubmissionVersion <= 0)
        {
            throw new InvalidOperationException(
                "Milestone automatic-acceptance submission version is invalid.");
        }
        EnsureAggregate(message, payload.MilestoneId);
        return new MilestoneEventPayload(payload.MilestoneId, null);
    }

    private static IReadOnlyCollection<NotificationDraft> ToCounterparty(
        Guid actorUserId,
        MilestoneNotificationContext context,
        string type,
        NotificationSeverity severity,
        string title,
        string body,
        IReadOnlyDictionary<string, string> data) =>
        ToCounterparty(
            actorUserId,
            context.ClientUserId,
            context.LawyerUserId,
            type,
            severity,
            title,
            body,
            data);

    private static IReadOnlyCollection<NotificationDraft> ToCounterparty(
        Guid actorUserId,
        Guid clientUserId,
        Guid lawyerUserId,
        string type,
        NotificationSeverity severity,
        string title,
        string body,
        IReadOnlyDictionary<string, string> data)
    {
        var recipientUserId = actorUserId == clientUserId
            ? lawyerUserId
            : actorUserId == lawyerUserId
                ? clientUserId
                : throw new InvalidOperationException(
                    "Milestone actor is not a contract participant.");
        return [Draft(recipientUserId, type, severity, title, body, data)];
    }

    private static IReadOnlyCollection<NotificationDraft> BothParticipants(
        MilestoneNotificationContext context,
        string type,
        NotificationSeverity severity,
        string title,
        string body,
        IReadOnlyDictionary<string, string> data) =>
    [
        Draft(context.ClientUserId, type, severity, title, body, data),
        Draft(context.LawyerUserId, type, severity, title, body, data)
    ];

    private static Guid RequireParticipant(
        Guid? actorUserId,
        MilestoneNotificationContext context)
    {
        if (!actorUserId.HasValue)
        {
            throw new InvalidOperationException(
                "Milestone participant event actor is missing.");
        }
        return RequireParticipant(
            actorUserId.Value,
            context.ClientUserId,
            context.LawyerUserId,
            "Milestone event actor is not a contract participant.");
    }

    private static Guid RequireParticipant(
        Guid actorUserId,
        Guid clientUserId,
        Guid lawyerUserId,
        string error)
    {
        if (actorUserId != clientUserId && actorUserId != lawyerUserId)
        {
            throw new InvalidOperationException(error);
        }
        return actorUserId;
    }

    private static IReadOnlyDictionary<string, string> Data(
        MilestoneNotificationContext context) =>
        new Dictionary<string, string>
        {
            ["milestoneId"] = context.MilestoneId.ToString(),
            ["contractId"] = context.ContractId.ToString(),
            ["proposalId"] = context.ProposalId.ToString(),
            ["legalCaseId"] = context.LegalCaseId.ToString()
        };

    private static IReadOnlyDictionary<string, string> Data(
        MilestoneChangeRequestNotificationContext context) =>
        new Dictionary<string, string>
        {
            ["milestoneId"] = context.MilestoneId.ToString(),
            ["contractId"] = context.ContractId.ToString(),
            ["proposalId"] = context.ProposalId.ToString(),
            ["legalCaseId"] = context.LegalCaseId.ToString(),
            ["changeRequestId"] = context.ChangeRequestId.ToString()
        };

    private static NotificationDraft Draft(
        Guid recipientUserId,
        string type,
        NotificationSeverity severity,
        string title,
        string body,
        IReadOnlyDictionary<string, string> data) => new(
            recipientUserId,
            type,
            severity,
            title,
            body,
            null,
            data);

    private static bool IsChangeRequestEvent(string eventType) =>
        eventType is ContractPaymentEventTypes.MilestoneChangeRequestCreated
            or ContractPaymentEventTypes.MilestoneChangeRequestApproved
            or ContractPaymentEventTypes.MilestoneChangeRequestRejected
            or ContractPaymentEventTypes.MilestoneChangeRequestCancelled;

    private static void EnsureChangeRequestStatus(
        string eventType,
        string status)
    {
        var expected = eventType switch
        {
            ContractPaymentEventTypes.MilestoneChangeRequestCreated => "Pending",
            ContractPaymentEventTypes.MilestoneChangeRequestApproved => "Approved",
            ContractPaymentEventTypes.MilestoneChangeRequestRejected => "Rejected",
            ContractPaymentEventTypes.MilestoneChangeRequestCancelled => "Cancelled",
            _ => throw Unsupported(eventType)
        };
        if (!string.Equals(status, expected, StringComparison.Ordinal))
        {
            throw new InvalidOperationException(
                "Milestone change-request event status does not match its event type.");
        }
    }

    private static T Deserialize<T>(OutboxMessage message)
    {
        try
        {
            return JsonSerializer.Deserialize<T>(
                    message.Payload,
                    SerializerOptions)
                ?? throw new JsonException();
        }
        catch (JsonException exception)
        {
            throw new InvalidOperationException(
                "Milestone notification payload is invalid.",
                exception);
        }
    }

    private static void EnsureVersion(
        OutboxMessage message,
        int expectedVersion)
    {
        if (message.EventVersion != expectedVersion)
        {
            throw new InvalidOperationException(
                $"Milestone notification event version {message.EventVersion} is unsupported for '{message.EventType}'.");
        }
    }

    private static void EnsureAggregate(
        OutboxMessage message,
        Guid aggregateId)
    {
        if (aggregateId == Guid.Empty || aggregateId != message.AggregateId)
        {
            throw new InvalidOperationException(
                "Milestone notification aggregate and payload identifiers do not match.");
        }
    }

    private static void EnsureIdentifier(Guid identifier, string name)
    {
        if (identifier == Guid.Empty)
        {
            throw new InvalidOperationException(
                $"Milestone notification {name} identifier is invalid.");
        }
    }

    private static InvalidOperationException Unsupported(string eventType) =>
        new($"Milestone notification event type '{eventType}' is unsupported.");

    private sealed record MilestoneEventPayload(
        Guid MilestoneId,
        Guid? ActorUserId);
}
