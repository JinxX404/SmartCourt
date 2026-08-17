using System.Text.Json;
using SmartCourt.Features.Contracts.Integration;
using SmartCourt.Features.Notifications.Enums;
using SmartCourt.Infrastructure.Persistence.Entities;
using SmartCourt.Infrastructure.Providers.Events;

namespace SmartCourt.Features.Notifications.Events;

internal sealed class ContractNotificationEventMapper(
    IContractNotificationContextReader contextReader)
    : INotificationEventMapper
{
    private static readonly JsonSerializerOptions SerializerOptions = new(
        JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true
    };

    public IReadOnlyCollection<string> EventTypes =>
    [
        ContractPaymentEventTypes.ContractCreated,
        ContractPaymentEventTypes.ContractDraftUpdated,
        ContractPaymentEventTypes.ContractAccepted,
        ContractPaymentEventTypes.ContractActivated,
        ContractPaymentEventTypes.ContractCompleted,
        ContractPaymentEventTypes.ContractTerminationRequested,
        ContractPaymentEventTypes.ContractTerminated
    ];

    public async Task<IReadOnlyCollection<NotificationDraft>> MapAsync(
        OutboxMessage message,
        CancellationToken cancellationToken)
    {
        if (message.EventType == ContractPaymentEventTypes.ContractAccepted
            && message.EventVersion == 1)
        {
            // V1 did not capture the actor, so choosing a counterparty is unsafe.
            return [];
        }

        var eventContext = ValidateAndReadPayload(message);
        var context = await contextReader.GetAsync(
            eventContext.ContractId,
            cancellationToken);
        if (context.ContractId != message.AggregateId)
        {
            throw new InvalidOperationException(
                "Contract notification aggregate and context identifiers do not match.");
        }
        if (eventContext.LegalCaseId.HasValue
            && eventContext.LegalCaseId.Value != context.LegalCaseId)
        {
            throw new InvalidOperationException(
                "Contract notification legal-case identifiers do not match.");
        }
        if (message.EventType == ContractPaymentEventTypes.ContractTerminated
            && eventContext.ActorUserId != context.ClientUserId
            && eventContext.ActorUserId != context.LawyerUserId)
        {
            throw new InvalidOperationException(
                "Contract termination actor is not a contract participant.");
        }

        var data = new Dictionary<string, string>
        {
            ["contractId"] = context.ContractId.ToString(),
            ["proposalId"] = context.ProposalId.ToString(),
            ["legalCaseId"] = context.LegalCaseId.ToString()
        };

        return message.EventType switch
        {
            ContractPaymentEventTypes.ContractCreated =>
            [
                Draft(
                    context.ClientUserId,
                    "contract.created",
                    NotificationSeverity.Information,
                    "مسودة عقد جديدة",
                    "أنشأ المحامي مسودة عقد جديدة لتراجع شروطها.",
                    data)
            ],
            ContractPaymentEventTypes.ContractDraftUpdated =>
            [
                Draft(
                    context.ClientUserId,
                    "contract.draft-updated",
                    NotificationSeverity.Warning,
                    "تم تحديث مسودة العقد",
                    "تم تحديث شروط العقد، وتحتاج النسخة الحالية إلى مراجعتك وموافقتك.",
                    data)
            ],
            ContractPaymentEventTypes.ContractAccepted =>
                MapAcceptance(
                    eventContext.ActorUserId!.Value,
                    eventContext.RequiresCounterpartyAcceptance,
                    context,
                    data),
            ContractPaymentEventTypes.ContractActivated =>
                BothParticipants(
                    context,
                    "contract.activated",
                    NotificationSeverity.Success,
                    "تم تفعيل العقد",
                    "أصبح العقد نشطًا ويمكن بدء تنفيذ مراحله.",
                    data),
            ContractPaymentEventTypes.ContractCompleted =>
                BothParticipants(
                    context,
                    "contract.completed",
                    NotificationSeverity.Success,
                    "اكتمل العقد",
                    "اكتملت جميع مراحل العقد وتسوياته بنجاح.",
                    data),
            ContractPaymentEventTypes.ContractTerminationRequested =>
                MapTerminationRequested(
                    eventContext.ActorUserId!.Value,
                    context,
                    data),
            ContractPaymentEventTypes.ContractTerminated =>
                BothParticipants(
                    context,
                    "contract.terminated",
                    NotificationSeverity.Warning,
                    "تم إنهاء العقد",
                    "اكتملت إجراءات إنهاء العقد وتسويته.",
                    data),
            _ => throw new InvalidOperationException(
                $"Contract notification event type '{message.EventType}' is unsupported.")
        };
    }

    private static ContractEventContext ValidateAndReadPayload(
        OutboxMessage message)
    {
        return message.EventType switch
        {
            ContractPaymentEventTypes.ContractCreated
                or ContractPaymentEventTypes.ContractActivated
                or ContractPaymentEventTypes.ContractCompleted =>
                ReadAggregate(message),
            ContractPaymentEventTypes.ContractDraftUpdated =>
                ReadDraftUpdated(message),
            ContractPaymentEventTypes.ContractAccepted =>
                ReadAcceptance(message),
            ContractPaymentEventTypes.ContractTerminationRequested =>
                ReadTerminationRequested(message),
            ContractPaymentEventTypes.ContractTerminated =>
                ReadTerminated(message),
            _ => throw new InvalidOperationException(
                $"Contract notification event type '{message.EventType}' is unsupported.")
        };
    }

    private static ContractEventContext ReadAggregate(OutboxMessage message)
    {
        EnsureVersion(message, 1);
        var payload = Deserialize<ContractPaymentAggregateEventPayload>(message);
        EnsureAggregate(message, payload.EntityId);
        return new ContractEventContext(payload.EntityId, null, false, null);
    }

    private static ContractEventContext ReadDraftUpdated(OutboxMessage message)
    {
        EnsureVersion(message, 1);
        var payload = Deserialize<ContractDraftUpdatedEventPayload>(message);
        EnsureAggregate(message, payload.ContractId);
        return new ContractEventContext(payload.ContractId, null, false, null);
    }

    private static ContractEventContext ReadAcceptance(OutboxMessage message)
    {
        EnsureVersion(message, 2);
        var payload = Deserialize<ContractAcceptanceRecordedEventPayload>(message);
        EnsureAggregate(message, payload.ContractId);
        if (payload.AcceptedByUserId == Guid.Empty)
        {
            throw new InvalidOperationException(
                "Contract acceptance actor identifier is invalid.");
        }

        return new ContractEventContext(
            payload.ContractId,
            payload.AcceptedByUserId,
            payload.RequiresCounterpartyAcceptance,
            null);
    }

    private static ContractEventContext ReadTerminationRequested(
        OutboxMessage message)
    {
        EnsureVersion(message, 1);
        var payload = Deserialize<ContractTerminationRequestedEventPayload>(message);
        EnsureAggregate(message, payload.ContractId);
        if (payload.RequestedByUserId == Guid.Empty)
        {
            throw new InvalidOperationException(
                "Contract termination requester identifier is invalid.");
        }

        return new ContractEventContext(
            payload.ContractId,
            payload.RequestedByUserId,
            false,
            null);
    }

    private static ContractEventContext ReadTerminated(OutboxMessage message)
    {
        EnsureVersion(message, 1);
        var payload = Deserialize<ContractTerminatedEventPayload>(message);
        EnsureAggregate(message, payload.ContractId);
        if (payload.LegalCaseId == Guid.Empty
            || payload.TerminatedByUserId == Guid.Empty)
        {
            throw new InvalidOperationException(
                "Contract termination payload identifiers are invalid.");
        }
        return new ContractEventContext(
            payload.ContractId,
            payload.TerminatedByUserId,
            false,
            payload.LegalCaseId);
    }

    private static IReadOnlyCollection<NotificationDraft> MapAcceptance(
        Guid actorUserId,
        bool requiresCounterpartyAcceptance,
        ContractNotificationContext context,
        IReadOnlyDictionary<string, string> data)
    {
        if (!requiresCounterpartyAcceptance)
        {
            return [];
        }

        var recipientUserId = actorUserId == context.ClientUserId
            ? context.LawyerUserId
            : actorUserId == context.LawyerUserId
                ? context.ClientUserId
                : throw new InvalidOperationException(
                    "Contract acceptance actor is not a contract participant.");
        return
        [
            Draft(
                recipientUserId,
                "contract.acceptance-recorded",
                NotificationSeverity.Information,
                "موافقة جديدة على العقد",
                "وافق الطرف الآخر على نسخة العقد الحالية، والعقد بانتظار موافقتك.",
                data)
        ];
    }

    private static IReadOnlyCollection<NotificationDraft>
        MapTerminationRequested(
            Guid requesterUserId,
            ContractNotificationContext context,
            IReadOnlyDictionary<string, string> data)
    {
        var counterpartyUserId = requesterUserId == context.ClientUserId
            ? context.LawyerUserId
            : requesterUserId == context.LawyerUserId
                ? context.ClientUserId
                : throw new InvalidOperationException(
                    "Contract termination requester is not a contract participant.");
        var drafts = new List<NotificationDraft>
        {
            Draft(
                counterpartyUserId,
                "contract.termination-requested",
                NotificationSeverity.Warning,
                "تم طلب إنهاء العقد",
                "تم تسجيل طلب إنهاء العقد، وتجري معالجة التسوية اللازمة.",
                data)
        };
        if (!context.IsTerminated)
        {
            drafts.Add(
                Draft(
                    requesterUserId,
                    "contract.termination-requested",
                    NotificationSeverity.Warning,
                    "تم طلب إنهاء العقد",
                    "تم تسجيل طلب إنهاء العقد، وتجري معالجة التسوية اللازمة.",
                    data));
        }

        return drafts;
    }

    private static IReadOnlyCollection<NotificationDraft> BothParticipants(
        ContractNotificationContext context,
        string type,
        NotificationSeverity severity,
        string title,
        string body,
        IReadOnlyDictionary<string, string> data) =>
    [
        Draft(context.ClientUserId, type, severity, title, body, data),
        Draft(context.LawyerUserId, type, severity, title, body, data)
    ];

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
            NotificationActionUrls.Contract(
                Guid.Parse(data["contractId"])),
            data);

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
                "Contract notification payload is invalid.",
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
                $"Contract notification event version {message.EventVersion} is unsupported for '{message.EventType}'.");
        }
    }

    private static void EnsureAggregate(
        OutboxMessage message,
        Guid contractId)
    {
        if (contractId == Guid.Empty || contractId != message.AggregateId)
        {
            throw new InvalidOperationException(
                "Contract notification aggregate and payload identifiers do not match.");
        }
    }

    private sealed record ContractEventContext(
        Guid ContractId,
        Guid? ActorUserId,
        bool RequiresCounterpartyAcceptance,
        Guid? LegalCaseId);
}
