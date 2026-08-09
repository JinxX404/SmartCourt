using SmartCourt.Features.Proposals.Entities;
using SmartCourt.Infrastructure.Providers.Events;

namespace SmartCourt.Features.Proposals.Shared;

internal static class ProposalOutbox
{
    public static Task EnqueueAsync(
        IOutboxWriter outboxWriter,
        string eventType,
        Proposal proposal,
        Guid? actorUserId,
        string? reason,
        CancellationToken cancellationToken)
    {
        return outboxWriter.EnqueueAsync(
            new OutboxEvent(
                eventType,
                1,
                new ProposalEventPayload(
                    proposal.Id,
                    proposal.LegalCaseId,
                    proposal.ClientUserId,
                    proposal.LawyerUserId,
                    actorUserId,
                    reason),
                nameof(Proposal),
                proposal.Id,
                Guid.NewGuid()),
            cancellationToken);
    }
}
