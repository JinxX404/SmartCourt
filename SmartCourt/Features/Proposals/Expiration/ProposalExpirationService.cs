using Microsoft.EntityFrameworkCore;
using SmartCourt.Features.Proposals.Entities;
using SmartCourt.Features.Proposals.Enums;
using SmartCourt.Features.Proposals.Shared;
using SmartCourt.Infrastructure.Providers.Events;
using SmartCourt.Persistence;

namespace SmartCourt.Features.Proposals.Expiration;

public sealed class ProposalExpirationService(
    ApplicationDbContext context,
    IOutboxWriter outboxWriter,
    TimeProvider timeProvider) : IProposalExpirationService
{
    private const int BatchSize = 200;

    public Task<int> ExpireDueAsync(CancellationToken cancellationToken)
    {
        return ExpireAsync(legalCaseId: null, BatchSize, cancellationToken);
    }

    public Task<int> ExpireDueForCaseAsync(
        Guid legalCaseId,
        CancellationToken cancellationToken)
    {
        if (legalCaseId == Guid.Empty)
        {
            return Task.FromResult(0);
        }

        return ExpireAsync(legalCaseId, BatchSize, cancellationToken);
    }

    private async Task<int> ExpireAsync(
        Guid? legalCaseId,
        int batchSize,
        CancellationToken cancellationToken)
    {
        var now = timeProvider.GetUtcNow();
        var query = context.Proposals
            .Where(proposal =>
                proposal.Status == ProposalStatus.Pending
                && proposal.ExpiresAt <= now);
        if (legalCaseId.HasValue)
        {
            query = query.Where(proposal =>
                proposal.LegalCaseId == legalCaseId.Value);
        }

        var due = await query
            .OrderBy(proposal => proposal.ExpiresAt)
            .ThenBy(proposal => proposal.Id)
            .Take(batchSize)
            .ToListAsync(cancellationToken);

        foreach (var proposal in due)
        {
            proposal.Expire(now);
            await ProposalOutbox.EnqueueAsync(
                outboxWriter,
                ContractPaymentEventTypes.ProposalExpired,
                proposal,
                actorUserId: null,
                reason: null,
                cancellationToken);
        }

        if (due.Count > 0)
        {
            await context.SaveChangesAsync(cancellationToken);
        }

        return due.Count;
    }
}
