using System.Text.Json;
using SmartCourt.Features.Notifications.Enums;
using SmartCourt.Infrastructure.Persistence.Entities;
using SmartCourt.Infrastructure.Providers.Events;

namespace SmartCourt.Features.Notifications.Events;

internal sealed class ProposalNotificationEventMapper
    : INotificationEventMapper
{
    private static readonly JsonSerializerOptions SerializerOptions = new(
        JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true
    };

    public IReadOnlyCollection<string> EventTypes =>
    [
        ContractPaymentEventTypes.ProposalCreated,
        ContractPaymentEventTypes.ProposalAccepted,
        ContractPaymentEventTypes.ProposalRejected,
        ContractPaymentEventTypes.ProposalCancelled,
        ContractPaymentEventTypes.ProposalExpired,
        ContractPaymentEventTypes.ProposalTerminated,
        ContractPaymentEventTypes.ProposalSuperseded
    ];

    public Task<IReadOnlyCollection<NotificationDraft>> MapAsync(
        OutboxMessage message,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (message.EventVersion != 1)
        {
            throw new InvalidOperationException(
                $"Proposal notification event version {message.EventVersion} is unsupported.");
        }

        var payload = JsonSerializer.Deserialize<ProposalEventPayload>(
            message.Payload,
            SerializerOptions)
            ?? throw new InvalidOperationException(
                "Proposal notification payload is invalid.");
        if (payload.ProposalId == Guid.Empty
            || payload.ProposalId != message.AggregateId)
        {
            throw new InvalidOperationException(
                "Proposal notification aggregate and payload identifiers do not match.");
        }

        var data = new Dictionary<string, string>
        {
            ["proposalId"] = payload.ProposalId.ToString(),
            ["legalCaseId"] = payload.LegalCaseId.ToString()
        };
        var actionUrl = NotificationActionUrls.Proposal(payload.ProposalId);
        NotificationDraft draft = message.EventType switch
        {
            ContractPaymentEventTypes.ProposalCreated => new(
                payload.LawyerUserId,
                "proposal.created",
                NotificationSeverity.Information,
                "عرض جديد",
                "أرسل إليك موكل عرضًا جديدًا لمراجعته.",
                actionUrl,
                data),
            ContractPaymentEventTypes.ProposalAccepted => new(
                payload.ClientUserId,
                "proposal.accepted",
                NotificationSeverity.Success,
                "تم قبول العرض",
                "وافق المحامي على عرضك.",
                actionUrl,
                data),
            ContractPaymentEventTypes.ProposalRejected => new(
                payload.ClientUserId,
                "proposal.rejected",
                NotificationSeverity.Warning,
                "تم رفض العرض",
                "رفض المحامي عرضك. يمكنك مراجعة التفاصيل واختيار محامٍ آخر.",
                actionUrl,
                data),
            ContractPaymentEventTypes.ProposalCancelled => new(
                payload.LawyerUserId,
                "proposal.cancelled",
                NotificationSeverity.Information,
                "تم إلغاء العرض",
                "ألغى الموكل العرض المعلق.",
                actionUrl,
                data),
            ContractPaymentEventTypes.ProposalExpired => new(
                payload.ClientUserId,
                "proposal.expired",
                NotificationSeverity.Warning,
                "انتهت صلاحية العرض",
                "لم يرد المحامي على العرض خلال ثلاثة أيام.",
                actionUrl,
                data),
            ContractPaymentEventTypes.ProposalTerminated => new(
                GetOtherParticipant(payload),
                "proposal.terminated",
                NotificationSeverity.Warning,
                "انتهت المفاوضات",
                "أنهى الطرف الآخر مفاوضات العرض.",
                actionUrl,
                data),
            ContractPaymentEventTypes.ProposalSuperseded => new(
                payload.LawyerUserId,
                "proposal.superseded",
                NotificationSeverity.Information,
                "تم إغلاق العرض",
                "نعتذر، تم إسناد القضية إلى محامٍ آخر ولم تعد محادثة التفاوض متاحة حفاظًا على خصوصية الموكل.",
                actionUrl,
                data),
            _ => throw new InvalidOperationException(
                $"Proposal notification event type '{message.EventType}' is unsupported.")
        };

        return Task.FromResult<IReadOnlyCollection<NotificationDraft>>([draft]);
    }

    private static Guid GetOtherParticipant(ProposalEventPayload payload)
    {
        return payload.ActorUserId switch
        {
            var actor when actor == payload.ClientUserId => payload.LawyerUserId,
            var actor when actor == payload.LawyerUserId => payload.ClientUserId,
            _ => throw new InvalidOperationException(
                "A terminated proposal event requires a valid participant actor.")
        };
    }
}
