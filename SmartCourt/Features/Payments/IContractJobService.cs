using SmartCourt.Infrastructure.Providers.Jobs;

namespace SmartCourt.Features.Payments;

public interface IContractJobService
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

    Task<JobExecutionResult> ReconcileMissingSchedulesAsync(
        CancellationToken cancellationToken);

    Task<JobExecutionResult> ReconcilePendingProviderTransactionsAsync(
        CancellationToken cancellationToken);

    Task<JobExecutionResult> RecoverPendingContractTerminationsAsync(
        CancellationToken cancellationToken);

    Task<JobExecutionResult> RecoverPendingDisputeSettlementsAsync(
        CancellationToken cancellationToken);

    Task<JobExecutionResult> ReconcilePendingWalletProjectionsAsync(
        CancellationToken cancellationToken);

    Task<JobExecutionResult> DispatchOutboxAsync(
        int batchSize,
        CancellationToken cancellationToken);
}
