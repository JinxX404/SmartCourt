using Microsoft.EntityFrameworkCore;
using SmartCourt.Features.Milestones.Integration;
using SmartCourt.Persistence;

namespace SmartCourt.Features.Milestones.Events;

internal sealed class MilestoneNotificationContextReader(
    ApplicationDbContext dbContext) : IMilestoneNotificationContextReader
{
    public async Task<MilestoneNotificationContext> GetMilestoneAsync(
        Guid milestoneId,
        CancellationToken cancellationToken)
    {
        return await (
                from milestone in dbContext.Milestones.AsNoTracking()
                join contract in dbContext.Contracts.AsNoTracking()
                    on milestone.ContractId equals contract.Id
                where milestone.Id == milestoneId
                select new MilestoneNotificationContext(
                    milestone.Id,
                    contract.Id,
                    contract.ProposalId,
                    contract.LegalCaseId,
                    contract.ClientUserId,
                    contract.LawyerUserId))
            .SingleOrDefaultAsync(cancellationToken)
            ?? throw new InvalidOperationException(
                "تعذر إنشاء الإشعار لأن المرحلة المرتبطة بالحدث غير موجودة.");
    }

    public async Task<MilestoneChangeRequestNotificationContext>
        GetChangeRequestAsync(
            Guid changeRequestId,
            CancellationToken cancellationToken)
    {
        return await (
                from changeRequest in
                    dbContext.MilestoneChangeRequests.AsNoTracking()
                join milestone in dbContext.Milestones.AsNoTracking()
                    on changeRequest.MilestoneId equals milestone.Id
                join contract in dbContext.Contracts.AsNoTracking()
                    on milestone.ContractId equals contract.Id
                where changeRequest.Id == changeRequestId
                select new MilestoneChangeRequestNotificationContext(
                    changeRequest.Id,
                    milestone.Id,
                    contract.Id,
                    contract.ProposalId,
                    contract.LegalCaseId,
                    contract.ClientUserId,
                    contract.LawyerUserId,
                    changeRequest.RequestedByUserId))
            .SingleOrDefaultAsync(cancellationToken)
            ?? throw new InvalidOperationException(
                "تعذر إنشاء الإشعار لأن طلب تعديل المرحلة المرتبط بالحدث غير موجود.");
    }
}
