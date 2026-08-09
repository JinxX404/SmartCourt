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
        ContractPaymentEventTypes.ProposalRejected
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
        var actionUrl = $"/proposals/{payload.ProposalId}";
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
            _ => throw new InvalidOperationException(
                $"Proposal notification event type '{message.EventType}' is unsupported.")
        };

        return Task.FromResult<IReadOnlyCollection<NotificationDraft>>([draft]);
    }
}
