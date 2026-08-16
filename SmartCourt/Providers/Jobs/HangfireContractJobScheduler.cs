using SmartCourt.Common.Exceptions;
using SmartCourt.Features.Payments;
using SmartCourt.Infrastructure.Providers.Jobs;
using SmartCourt.Interfaces.Providers;

namespace SmartCourt.Providers.Jobs;

public sealed class HangfireContractJobScheduler : IContractJobScheduler
{
    private readonly IBackgroundJobProvider _backgroundJobs;

    public HangfireContractJobScheduler(
        IBackgroundJobProvider backgroundJobs)
    {
        _backgroundJobs = backgroundJobs;
    }

    public async Task<string> ScheduleAutoAcceptAsync(
        Guid milestoneId,
        Guid escrowHoldId,
        int submissionVersion,
        DateTimeOffset runAtUtc,
        CancellationToken cancellationToken)
    {
        EnsureId(milestoneId, nameof(milestoneId));
        EnsureId(escrowHoldId, nameof(escrowHoldId));
        if (submissionVersion <= 0)
        {
            throw new BusinessException(
                "??? ?? ???? ????? ????? ??????? ???? ?? ???.");
        }

        return await _backgroundJobs.ScheduleAsync<IContractJobService>(
            service => service.AutoAcceptMilestoneAsync(
                milestoneId,
                escrowHoldId,
                submissionVersion,
                CancellationToken.None),
            EnsureUtc(runAtUtc),
            cancellationToken);
    }

    public async Task<string> ScheduleReleaseExpiredHoldAsync(
        Guid escrowHoldId,
        DateTimeOffset runAtUtc,
        CancellationToken cancellationToken)
    {
        EnsureId(escrowHoldId, nameof(escrowHoldId));
        return await _backgroundJobs.ScheduleAsync<IContractJobService>(
            service => service.ReleaseExpiredHoldAsync(
                escrowHoldId,
                CancellationToken.None),
            EnsureUtc(runAtUtc),
            cancellationToken);
    }

    public async Task<string> ScheduleProviderReconciliationAsync(
        Guid paymentTransactionId,
        DateTimeOffset runAtUtc,
        CancellationToken cancellationToken)
    {
        EnsureId(paymentTransactionId, nameof(paymentTransactionId));
        return await _backgroundJobs.ScheduleAsync<IContractJobService>(
            service => service.ReconcileProviderTransactionAsync(
                paymentTransactionId,
                CancellationToken.None),
            EnsureUtc(runAtUtc),
            cancellationToken);
    }

    public async Task<string> ScheduleProviderRetryAsync(
        Guid paymentTransactionId,
        DateTimeOffset runAtUtc,
        CancellationToken cancellationToken)
    {
        EnsureId(paymentTransactionId, nameof(paymentTransactionId));
        return await _backgroundJobs.ScheduleAsync<IContractJobService>(
            service => service.RetryProviderTransactionAsync(
                paymentTransactionId,
                CancellationToken.None),
            EnsureUtc(runAtUtc),
            cancellationToken);
    }

    public async Task<string> ScheduleSchedulingReconciliationAsync(
        DateTimeOffset runAtUtc,
        CancellationToken cancellationToken)
    {
        return await _backgroundJobs.ScheduleAsync<IContractJobService>(
            service => service.ReconcileMissingSchedulesAsync(
                CancellationToken.None),
            EnsureUtc(runAtUtc),
            cancellationToken);
    }

    public async Task<string> SchedulePendingWalletProjectionReconciliationAsync(
        DateTimeOffset runAtUtc,
        CancellationToken cancellationToken)
    {
        return await _backgroundJobs.ScheduleAsync<IContractJobService>(
            service => service.ReconcilePendingWalletProjectionsAsync(
                CancellationToken.None),
            EnsureUtc(runAtUtc),
            cancellationToken);
    }

    public async Task<string> ScheduleOutboxDispatchAsync(
        int batchSize,
        DateTimeOffset runAtUtc,
        CancellationToken cancellationToken)
    {
        if (batchSize <= 0)
        {
            throw new BusinessException(
                "??? ?? ???? ??? ???? ????? ????? ????? ?????? ???? ?? ???.");
        }

        return await _backgroundJobs.ScheduleAsync<IContractJobService>(
            service => service.DispatchOutboxAsync(
                batchSize,
                CancellationToken.None),
            EnsureUtc(runAtUtc),
            cancellationToken);
    }

    private static DateTimeOffset EnsureUtc(DateTimeOffset runAtUtc)
    {
        if (runAtUtc.Offset != TimeSpan.Zero)
        {
            throw new BusinessException(
                "??? ????? ???? ?????? ???????? ????? ????? ????.");
        }
        return runAtUtc;
    }

    private static void EnsureId(Guid id, string parameterName)
    {
        if (id == Guid.Empty)
        {
            throw new BusinessException(
                $"??? ??? ???? ??????? {parameterName} ??????.");
        }
    }
}