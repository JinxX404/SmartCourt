using SmartCourt.Infrastructure.Providers.Jobs;

namespace SmartCourt.Features.Payments;

public interface IContractJobOperations
{
    Task<JobExecutionResult> AutoAcceptMilestoneAsync(
        Guid milestoneId,
        Guid escrowHoldId,
        int submissionVersion,
        CancellationToken cancellationToken);

    Task<JobExecutionResult> ReleaseExpiredHoldAsync(
        Guid escrowHoldId,
        CancellationToken cancellationToken);

    Task<JobExecutionResult> ReconcileProviderTransactionAsync(
        Guid paymentTransactionId,
        CancellationToken cancellationToken);

    Task<JobExecutionResult> RetryProviderTransactionAsync(
        Guid paymentTransactionId,
        CancellationToken cancellationToken);

    Task<JobExecutionResult> ReconcilePendingProviderTransactionsAsync(
        CancellationToken cancellationToken)
        => Task.FromResult(JobExecutionResult.NoOp(
            "PendingProviderTransactionReconciliationNotConfigured"));

    Task<JobExecutionResult> ReconcilePendingWalletProjectionsAsync(
        CancellationToken cancellationToken);
}
