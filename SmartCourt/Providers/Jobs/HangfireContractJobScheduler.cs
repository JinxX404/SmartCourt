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
        DateTime runAtUtc,
        CancellationToken cancellationToken)
    {
        EnsureId(milestoneId, nameof(milestoneId));
        EnsureId(escrowHoldId, nameof(escrowHoldId));
        if (submissionVersion <= 0)
        {
            throw new BusinessException(
                "يجب أن يكون إصدار تسليم المرحلة أكبر من صفر.");
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
        DateTime runAtUtc,
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
        DateTime runAtUtc,
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
        DateTime runAtUtc,
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
        DateTime runAtUtc,
        CancellationToken cancellationToken)
    {
        return await _backgroundJobs.ScheduleAsync<IContractJobService>(
            service => service.ReconcileMissingSchedulesAsync(
                CancellationToken.None),
            EnsureUtc(runAtUtc),
            cancellationToken);
    }

    public async Task<string> SchedulePendingWalletProjectionReconciliationAsync(
        DateTime runAtUtc,
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
        DateTime runAtUtc,
        CancellationToken cancellationToken)
    {
        if (batchSize <= 0)
        {
            throw new BusinessException(
                "يجب أن يكون حجم دفعة إرسال أحداث صندوق الصادر أكبر من صفر.");
        }

        return await _backgroundJobs.ScheduleAsync<IContractJobService>(
            service => service.DispatchOutboxAsync(
                batchSize,
                CancellationToken.None),
            EnsureUtc(runAtUtc),
            cancellationToken);
    }

    private static DateTimeOffset EnsureUtc(DateTime runAtUtc)
    {
        var utcDate = runAtUtc.Kind == DateTimeKind.Unspecified
            ? DateTime.SpecifyKind(runAtUtc, DateTimeKind.Utc)
            : runAtUtc;

        if (utcDate.Kind != DateTimeKind.Utc)
        {
            throw new BusinessException(
                "يجب جدولة مهام العقود باستخدام توقيت عالمي منسق.");
        }

        return new DateTimeOffset(utcDate);
    }

    private static void EnsureId(Guid id, string parameterName)
    {
        if (id == Guid.Empty)
        {
            throw new BusinessException(
                $"يجب ألا يكون المعرّف {parameterName} فارغًا.");
        }
    }
}
