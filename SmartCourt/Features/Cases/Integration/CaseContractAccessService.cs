using Microsoft.EntityFrameworkCore;
using SmartCourt.Common.Enums;
using SmartCourt.Persistence;

namespace SmartCourt.Features.Cases.Integration;

public sealed class CaseContractAccessService(
    ApplicationDbContext dbContext) : ICaseContractAccessService
{
    public async Task<CaseContractEligibilityFacts?>
        FindEligibleForContractAsync(
            Guid legalCaseId,
            CancellationToken cancellationToken)
    {
        if (legalCaseId == Guid.Empty)
        {
            return null;
        }

        return await dbContext.LegalCases
            .AsNoTracking()
            .Where(legalCase =>
                legalCase.Id == legalCaseId
                && legalCase.Status == CaseStatus.Matched)
            .Select(legalCase => new CaseContractEligibilityFacts(
                legalCase.Id,
                legalCase.ClientUserId))
            .SingleOrDefaultAsync(cancellationToken);
    }
}
