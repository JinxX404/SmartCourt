using Microsoft.EntityFrameworkCore;
using SmartCourt.Features.Proposals.Enums;
using SmartCourt.Persistence;

namespace SmartCourt.Features.Proposals.Integration;

public sealed class ProposalContractAccessService(
    ApplicationDbContext dbContext) : IProposalContractAccessService
{
    public async Task<AcceptedProposalContractFacts?>
        FindAcceptedForContractAsync(
            Guid proposalId,
            CancellationToken cancellationToken)
    {
        if (proposalId == Guid.Empty)
        {
            return null;
        }

        return await dbContext.Proposals
            .AsNoTracking()
            .Where(proposal =>
                proposal.Id == proposalId
                && proposal.Status == ProposalStatus.Accepted)
            .Select(proposal => new AcceptedProposalContractFacts(
                proposal.Id,
                proposal.LegalCaseId,
                proposal.ClientUserId,
                proposal.LawyerUserId))
            .SingleOrDefaultAsync(cancellationToken);
    }
}
