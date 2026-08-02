using SmartCourt.Infrastructure.Providers.Jobs;

namespace SmartCourt.Features.Payments;

public interface IWalletReconciliationService
{
    Task<JobExecutionResult> ReconcilePendingWithdrawalsAsync(
        CancellationToken cancellationToken);
}
