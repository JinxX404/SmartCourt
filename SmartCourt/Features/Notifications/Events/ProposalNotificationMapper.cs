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
            ContractPaymentEventTypes.ProposalCancelled => new(
                payload.LawyerUserId,
                "proposal.cancelled",
                NotificationSeverity.Information,
                "Proposal cancelled",
                "A client cancelled a pending proposal.",
                actionUrl,
                data),
            ContractPaymentEventTypes.ProposalExpired => new(
                payload.ClientUserId,
                "proposal.expired",
                NotificationSeverity.Warning,
                "Proposal expired",
                "A lawyer did not respond to your proposal within three days.",
                actionUrl,
                data),
            ContractPaymentEventTypes.ProposalTerminated => new(
                GetOtherParticipant(payload),
                "proposal.terminated",
                NotificationSeverity.Warning,
                "Negotiation ended",
                "The proposal negotiation was ended by the other participant.",
                actionUrl,
                data),
            ContractPaymentEventTypes.ProposalSuperseded => new(
                payload.LawyerUserId,
                "proposal.superseded",
                NotificationSeverity.Information,
                "Proposal closed",
                "The client activated another contract for this case.",
                actionUrl,
                data),
            _ => throw new InvalidOperationException(
                $"Proposal notification event type '{eventType}' is unsupported.")
        };
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
