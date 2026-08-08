using Microsoft.EntityFrameworkCore;
using SmartCourt.Common.Enums;
using SmartCourt.Persistence;

namespace SmartCourt.Features.Case.Integration;

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

        return await dbContext.Cases
            .AsNoTracking()
            .Where(c =>
                c.Id == legalCaseId
                && c.Status == CaseStatus.Matched)
            .Select(c => new CaseContractEligibilityFacts(
                c.Id,
                c.ClientId))
            .SingleOrDefaultAsync(cancellationToken);
    }
}
