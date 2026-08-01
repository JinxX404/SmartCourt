using SmartCourt.Features.Milestones;
using SmartCourt.Features.Contracts;
using SmartCourt.Features.Disputes;
using SmartCourt.Infrastructure.Providers.Jobs;

namespace SmartCourt.Features.Payments;

public sealed class PaymentContractJobOperations(
    IPaymentReconciliationService paymentReconciliationService,
    IMilestoneAutoAcceptanceService milestoneAutoAcceptanceService,
    IEscrowReleaseService escrowReleaseService,
    IWalletReconciliationService walletReconciliationService,
    IContractTerminationRecoveryService terminationRecoveryService,
    IDisputeSettlementRecoveryService disputeSettlementRecoveryService)
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
        RecoverPendingContractTerminationsAsync(
            CancellationToken cancellationToken)
    {
        return await terminationRecoveryService
            .RecoverPendingTerminationsAsync(cancellationToken);
    }

    public async Task<JobExecutionResult> RecoverPendingDisputeSettlementsAsync(
        CancellationToken cancellationToken)
    {
        return await disputeSettlementRecoveryService
            .RecoverPendingSettlementsAsync(cancellationToken);
    }

    public async Task<JobExecutionResult>
        ReconcilePendingWalletProjectionsAsync(
        CancellationToken cancellationToken)
    {
        return await walletReconciliationService.ReconcilePendingWithdrawalsAsync(
            cancellationToken);
    }
}
