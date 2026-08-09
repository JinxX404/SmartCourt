using SmartCourt.Features.Notifications.Enums;
using SmartCourt.Infrastructure.Providers.Events;

namespace SmartCourt.Features.Notifications.Events;

internal static class ProposalNotificationMapper
{
    public static ProposalNotificationDefinition Map(
        string eventType,
        ProposalEventPayload payload)
    {
        var data = new Dictionary<string, string>
        {
            ["proposalId"] = payload.ProposalId.ToString(),
            ["legalCaseId"] = payload.LegalCaseId.ToString()
        };
        var actionUrl = $"/proposals/{payload.ProposalId}";

        return eventType switch
        {
            ContractPaymentEventTypes.ProposalCreated => new(
                payload.LawyerUserId,
                "proposal.created",
                NotificationSeverity.Information,
                "New proposal",
                "A client sent you a new proposal.",
                actionUrl,
                data),
            ContractPaymentEventTypes.ProposalAccepted => new(
                payload.ClientUserId,
                "proposal.accepted",
                NotificationSeverity.Success,
                "Proposal accepted",
                "A lawyer accepted your proposal.",
                actionUrl,
                data),
            ContractPaymentEventTypes.ProposalRejected => new(
                payload.ClientUserId,
                "proposal.rejected",
                NotificationSeverity.Warning,
                "Proposal rejected",
                "A lawyer rejected your proposal.",
                actionUrl,
                data),
            _ => throw new InvalidOperationException(
                $"Proposal notification event type '{eventType}' is unsupported.")
        };
    }
}
