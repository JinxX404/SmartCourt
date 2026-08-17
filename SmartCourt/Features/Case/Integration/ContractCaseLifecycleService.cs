using Microsoft.EntityFrameworkCore;
using SmartCourt.Common.Enums;
using SmartCourt.Features.Contracts.Integration;
using SmartCourt.Persistence;

namespace SmartCourt.Features.Case.Integration;

public sealed class ContractCaseLifecycleService(
    ApplicationDbContext context) : IContractCaseLifecycleService
{
    public async Task ApplyAsync(
        ContractCaseLifecycleUpdate update,
        CancellationToken cancellationToken)
    {
        if (update.LegalCaseId == Guid.Empty)
        {
            return;
        }

        var legalCase = await context.Cases.SingleOrDefaultAsync(
            item => item.Id == update.LegalCaseId,
            cancellationToken);

        if (legalCase is null)
        {
            return;
        }

        if (update.Transition == ContractCaseLifecycleTransition.ContractCompleted
            || update.Transition == ContractCaseLifecycleTransition.ContractTerminated)
        {
            if (legalCase.Status != CaseStatus.Closed)
            {
                legalCase.Status = CaseStatus.Closed;
                legalCase.UpdatedAt = update.OccurredAt.UtcDateTime;
                await context.SaveChangesAsync(cancellationToken);
            }
        }
    }
}
