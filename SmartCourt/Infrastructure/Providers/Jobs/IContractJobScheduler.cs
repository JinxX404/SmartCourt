namespace SmartCourt.Infrastructure.Providers.Jobs;

public interface IContractJobScheduler
{
    Task<string> ScheduleAutoAcceptAsync(
        Guid milestoneId,
        Guid escrowHoldId,
        int submissionVersion,
        DateTime runAtUtc,
        CancellationToken cancellationToken);

    Task<string> ScheduleReleaseExpiredHoldAsync(
        Guid escrowHoldId,
        DateTime runAtUtc,
        CancellationToken cancellationToken);

    Task<string> ScheduleProviderReconciliationAsync(
        Guid paymentTransactionId,
        DateTime runAtUtc,
        CancellationToken cancellationToken);

    Task<string> ScheduleProviderRetryAsync(
        Guid paymentTransactionId,
        DateTime runAtUtc,
        CancellationToken cancellationToken);

    Task<string> ScheduleSchedulingReconciliationAsync(
        DateTime runAtUtc,
        CancellationToken cancellationToken);
}
