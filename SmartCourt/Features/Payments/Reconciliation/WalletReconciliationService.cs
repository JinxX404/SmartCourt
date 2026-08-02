using SmartCourt.Infrastructure.Providers.Jobs;

namespace SmartCourt.Features.Payments;

public sealed class WalletReconciliationService(
    IWalletService walletService) : IWalletReconciliationService
{
    public async Task<JobExecutionResult> ReconcilePendingWithdrawalsAsync(
        CancellationToken cancellationToken)
    {
        return await walletService.ReconcilePendingWithdrawalsAsync(
            cancellationToken);
    }
}
