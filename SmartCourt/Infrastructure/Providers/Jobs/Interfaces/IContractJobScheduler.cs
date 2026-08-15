namespace SmartCourt.Infrastructure.Providers.Jobs;

public interface IContractJobScheduler
{
    Task<string> ScheduleAutoAcceptAsync(
        Guid milestoneId,
        Guid escrowHoldId,
        int submissionVersion,
        DateTimeOffset runAtUtc,
        CancellationToken cancellationToken);

    Task<string> ScheduleReleaseExpiredHoldAsync(
        Guid escrowHoldId,
        DateTimeOffset runAtUtc,
        CancellationToken cancellationToken);

    Task<string> ScheduleProviderReconciliationAsync(
        Guid paymentTransactionId,
        DateTimeOffset runAtUtc,
        CancellationToken cancellationToken);

    Task<string> ScheduleProviderRetryAsync(
        Guid paymentTransactionId,
        DateTimeOffset runAtUtc,
        CancellationToken cancellationToken);

    Task<string> ScheduleSchedulingReconciliationAsync(
        DateTimeOffset runAtUtc,
        CancellationToken cancellationToken);

    Task<string> SchedulePendingWalletProjectionReconciliationAsync(
        DateTimeOffset runAtUtc,
        CancellationToken cancellationToken);

    Task<string> ScheduleOutboxDispatchAsync(
        int batchSize,
        DateTimeOffset runAtUtc,
        CancellationToken cancellationToken);
}
