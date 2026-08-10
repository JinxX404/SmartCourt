using Microsoft.EntityFrameworkCore;
using SmartCourt.Features.Payments.Integration;
using SmartCourt.Persistence;

namespace SmartCourt.Features.Payments.Events;

internal sealed class PaymentNotificationContextReader(
    ApplicationDbContext dbContext) : IPaymentNotificationContextReader
{
    public async Task<WithdrawalNotificationContext> GetWithdrawalAsync(
        Guid withdrawalId,
        CancellationToken cancellationToken)
    {
        return await dbContext.WithdrawalRequests
            .AsNoTracking()
            .Where(withdrawal => withdrawal.Id == withdrawalId)
            .Select(withdrawal => new WithdrawalNotificationContext(
                withdrawal.Id,
                withdrawal.LawyerUserId,
                withdrawal.Status,
                withdrawal.RequiresManualAction))
            .SingleOrDefaultAsync(cancellationToken)
            ?? throw new InvalidOperationException(
                "تعذر إنشاء الإشعار لأن طلب السحب المرتبط بالحدث غير موجود.");
    }

    public async Task<WalletAdjustmentNotificationContext>
        GetWalletAdjustmentAsync(
            Guid walletAdjustmentId,
            CancellationToken cancellationToken)
    {
        return await (
                from adjustment in dbContext.WalletAdjustments.AsNoTracking()
                join wallet in dbContext.LawyerWallets.AsNoTracking()
                    on adjustment.LawyerWalletId equals wallet.Id
                where adjustment.Id == walletAdjustmentId
                select new WalletAdjustmentNotificationContext(
                    adjustment.Id,
                    wallet.LawyerUserId,
                    adjustment.ContractId))
            .SingleOrDefaultAsync(cancellationToken)
            ?? throw new InvalidOperationException(
                "تعذر إنشاء الإشعار لأن تصحيح المحفظة المرتبط بالحدث غير موجود.");
    }
}
