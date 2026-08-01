using SmartCourt.Features.Milestones;
using SmartCourt.Infrastructure.Providers.Jobs;

namespace SmartCourt.Features.Payments;

public sealed class PaymentContractJobOperations(
    IPaymentReconciliationService paymentReconciliationService,
    IMilestoneAutoAcceptanceService milestoneAutoAcceptanceService,
    IEscrowReleaseService escrowReleaseService,
    IWalletReconciliationService walletReconciliationService)
    : IContractJobOperations
{
    public async Task<JobExecutionResult> ReconcileProviderTransactionAsync(
        Guid paymentTransactionId,
        CancellationToken cancellationToken)
    {
        return await paymentReconciliationService
            .ReconcileProviderTransactionAsync(
                paymentTransactionId,
                cancellationToken);
    }

    public async Task<JobExecutionResult> AutoAcceptMilestoneAsync(
        Guid milestoneId,
        Guid escrowHoldId,
        int submissionVersion,
        CancellationToken cancellationToken)
    {
        return await milestoneAutoAcceptanceService.AutoAcceptAsync(
            milestoneId,
            escrowHoldId,
            submissionVersion,
            cancellationToken);
    }

    public async Task<JobExecutionResult> ReleaseExpiredHoldAsync(
        Guid escrowHoldId,
        CancellationToken cancellationToken)
    {
        return await escrowReleaseService.ReleaseExpiredHoldAsync(
            escrowHoldId,
            cancellationToken);
    }

    public async Task<JobExecutionResult> RetryProviderTransactionAsync(
        Guid paymentTransactionId,
        CancellationToken cancellationToken)
    {
        return await paymentReconciliationService
            .ReconcileProviderTransactionAsync(
                paymentTransactionId,
                cancellationToken);
    }

    public async Task<JobExecutionResult>
        ReconcilePendingProviderTransactionsAsync(
            CancellationToken cancellationToken)
    {
        return await paymentReconciliationService
            .ReconcilePendingProviderTransactionsAsync(cancellationToken);
    }

    public async Task<JobExecutionResult>
        ReconcilePendingWalletProjectionsAsync(
        CancellationToken cancellationToken)
    {
        return await walletReconciliationService.ReconcilePendingWithdrawalsAsync(
            cancellationToken);
    }
}
