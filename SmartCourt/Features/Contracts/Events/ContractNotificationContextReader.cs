using Microsoft.EntityFrameworkCore;
using SmartCourt.Features.Contracts.Enums;
using SmartCourt.Features.Contracts.Integration;
using SmartCourt.Persistence;

namespace SmartCourt.Features.Contracts.Events;

internal sealed class ContractNotificationContextReader(
    ApplicationDbContext dbContext) : IContractNotificationContextReader
{
    public async Task<ContractNotificationContext> GetAsync(
        Guid contractId,
        CancellationToken cancellationToken)
    {
        return await dbContext.Contracts
            .AsNoTracking()
            .Where(contract => contract.Id == contractId)
            .Select(contract => new ContractNotificationContext(
                contract.Id,
                contract.ProposalId,
                contract.LegalCaseId,
                contract.ClientUserId,
                contract.LawyerUserId,
                contract.Status == ContractStatus.Terminated))
            .SingleOrDefaultAsync(cancellationToken)
            ?? throw new InvalidOperationException(
                "تعذر إنشاء الإشعار لأن العقد المرتبط بالحدث غير موجود.");
    }
}
