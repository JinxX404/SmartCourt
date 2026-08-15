using SmartCourt.Features.Payments.Entities;
using SmartCourt.Features.Payments.Enums;

namespace SmartCourt.Features.Payments.Domain;

internal static class PaymentReleaseRetryPolicy
{
    internal const int MaximumProviderAttempts = 3;
    private static readonly TimeSpan[] RetryDelays =
    [
        TimeSpan.FromMinutes(1),
        TimeSpan.FromMinutes(5)
    ];

    internal static bool CanRetry(
        PaymentTransaction transaction,
        DateTimeOffset now)
    {
        return transaction.OperationType == PaymentOperationType.Release
            && transaction.Status == PaymentTransactionStatus.Failed
            && !transaction.RequiresManualAction
            && (!transaction.NextRetryAt.HasValue
                || transaction.NextRetryAt.Value <= now);
    }

    internal static void StartProviderAttempt(
        PaymentTransaction transaction,
        DateTimeOffset now)
    {
        EnsureRelease(transaction);
        transaction.ProviderAttemptCount++;
        transaction.Status = PaymentTransactionStatus.Processing;
        transaction.FailureReason = null;
        transaction.NextRetryAt = null;
        transaction.RequiresManualAction = false;
        transaction.ManualActionRequiredAt = null;
        transaction.ProcessedAt = null;
        transaction.UpdatedAt = now;
    }

    internal static bool RecordConfirmedFailure(
        PaymentTransaction transaction,
        string failureReason,
        DateTimeOffset now)
    {
        EnsureRelease(transaction);
        transaction.ProviderAttemptCount = Math.Max(
            1,
            transaction.ProviderAttemptCount);
        transaction.Status = PaymentTransactionStatus.Failed;
        transaction.FailureReason = failureReason;
        transaction.ProcessedAt = now;
        transaction.UpdatedAt = now;
        if (transaction.ProviderAttemptCount
            >= MaximumProviderAttempts)
        {
            transaction.RequiresManualAction = true;
            transaction.ManualActionRequiredAt = now;
            transaction.NextRetryAt = null;
            return false;
        }

        transaction.RequiresManualAction = false;
        transaction.ManualActionRequiredAt = null;
        transaction.NextRetryAt = now.Add(
            RetryDelays[transaction.ProviderAttemptCount - 1]);
        return true;
    }

    internal static void RecordSuccess(
        PaymentTransaction transaction,
        DateTimeOffset now)
    {
        EnsureRelease(transaction);
        transaction.Status = PaymentTransactionStatus.Completed;
        transaction.FailureReason = null;
        transaction.NextRetryAt = null;
        transaction.RequiresManualAction = false;
        transaction.ManualActionRequiredAt = null;
        transaction.ProcessedAt = now;
        transaction.UpdatedAt = now;
    }

    private static void EnsureRelease(PaymentTransaction transaction)
    {
        if (transaction.OperationType != PaymentOperationType.Release)
        {
            throw new InvalidOperationException(
                "Release retry policy can only be applied to release transactions.");
        }
    }
}
