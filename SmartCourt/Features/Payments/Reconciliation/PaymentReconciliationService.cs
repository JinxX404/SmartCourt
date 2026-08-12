using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SmartCourt.Common.Exceptions;
using SmartCourt.Features.Milestones.Enums;
using SmartCourt.Features.Payments.Domain;
using SmartCourt.Features.Payments.Entities;
using SmartCourt.Features.Payments.Enums;
using SmartCourt.Infrastructure.Providers.Jobs;
using SmartCourt.Infrastructure.Providers.Payments;
using SmartCourt.Persistence;
using SmartCourt.Providers.Payments;

namespace SmartCourt.Features.Payments;

public sealed class PaymentReconciliationService(
    ApplicationDbContext dbContext,
    IPaymentEscrowService paymentEscrowService,
    IEscrowReleaseService escrowReleaseService,
    IPaymentReconciliationProvider reconciliationProvider,
    TimeProvider timeProvider,
    IOptions<PaymentProviderOptions> paymentProviderOptions,
    ILogger<PaymentReconciliationService> logger)
    : IPaymentReconciliationService
{
    private const int BatchSize = 100;

    public async Task<JobExecutionResult> ReconcileProviderTransactionAsync(
        Guid paymentTransactionId,
        CancellationToken cancellationToken)
    {
        if (paymentTransactionId == Guid.Empty)
        {
            throw new BusinessException(
                "معرّف معاملة الدفع مطلوب لإجراء المطابقة.");
        }

        var paymentTransaction = await dbContext.PaymentTransactions
            .SingleOrDefaultAsync(
                item => item.Id == paymentTransactionId,
                cancellationToken);
        if (paymentTransaction is null)
        {
            return JobExecutionResult.NoOp("PaymentTransactionNotFound");
        }

        var now = timeProvider.GetUtcNow().UtcDateTime;
        if (paymentTransaction.Status == PaymentTransactionStatus.Failed
            && paymentTransaction.OperationType
                == PaymentOperationType.Release
            && paymentTransaction.EscrowHoldId.HasValue)
        {
            if (paymentTransaction.RequiresManualAction)
            {
                return JobExecutionResult.NoOp(
                    "ReleaseRequiresManualAction");
            }

            if (!PaymentReleaseRetryPolicy.CanRetry(
                    paymentTransaction,
                    now))
            {
                return JobExecutionResult.NoOp("ReleaseRetryNotDue");
            }

            return await escrowReleaseService.ReleaseExpiredHoldAsync(
                paymentTransaction.EscrowHoldId.Value,
                cancellationToken);
        }

        if (paymentTransaction.Status != PaymentTransactionStatus.Processing)
        {
            return JobExecutionResult.NoOp("PaymentTransactionAlreadyFinal");
        }

        if (paymentTransaction.RequiresManualAction)
        {
            return JobExecutionResult.NoOp(
                "PaymentTransactionRequiresManualAction");
        }

        try
        {
            return paymentTransaction.OperationType switch
            {
                PaymentOperationType.Deposit => await ReconcileDepositAsync(
                    paymentTransaction,
                    cancellationToken),
                PaymentOperationType.Release => await ReconcileSettlementAsync(
                    paymentTransaction,
                    isRelease: true,
                    cancellationToken),
                PaymentOperationType.Refund => await ReconcileSettlementAsync(
                    paymentTransaction,
                    isRelease: false,
                    cancellationToken),
                _ => await EscalateIfStaleAsync(
                    paymentTransaction,
                    "PaymentOperationNotSupported",
                    "تعذر إجراء مطابقة آلية لنوع العملية المالية.",
                    cancellationToken)
            };
        }
        catch (OperationCanceledException)
            when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
            when (HasExceededProcessingSla(paymentTransaction))
        {
            dbContext.ChangeTracker.Clear();
            var pendingTransaction = await dbContext.PaymentTransactions
                .SingleOrDefaultAsync(
                    item => item.Id == paymentTransactionId,
                    cancellationToken);
            if (pendingTransaction is null
                || pendingTransaction.Status
                    != PaymentTransactionStatus.Processing
                || pendingTransaction.RequiresManualAction)
            {
                return JobExecutionResult.NoOp(
                    "PaymentTransactionAlreadyFinal");
            }

            return await RequireManualActionAsync(
                pendingTransaction,
                "تعذر حسم نتيجة العملية المالية بعد تجاوز مهلة المطابقة.",
                exception,
                cancellationToken);
        }
    }

    public async Task<JobExecutionResult>
        ReconcilePendingProviderTransactionsAsync(
            CancellationToken cancellationToken)
    {
        var now = timeProvider.GetUtcNow().UtcDateTime;
        var transactionIds = await dbContext.PaymentTransactions
            .AsNoTracking()
            .Where(item =>
                (item.Status == PaymentTransactionStatus.Processing
                    && !item.RequiresManualAction)
                || (item.OperationType == PaymentOperationType.Release
                    && item.Status == PaymentTransactionStatus.Failed
                    && !item.RequiresManualAction
                    && (!item.NextRetryAt.HasValue
                        || item.NextRetryAt <= now)))
            .OrderBy(item => item.CreatedAt)
            .ThenBy(item => item.Id)
            .Select(item => item.Id)
            .Take(BatchSize)
            .ToListAsync(cancellationToken);
        var reconciled = 0;
        foreach (var transactionId in transactionIds)
        {
            var result = await ReconcileProviderTransactionAsync(
                transactionId,
                cancellationToken);
            if (result.Outcome == JobExecutionOutcome.Completed)
            {
                reconciled++;
            }

            dbContext.ChangeTracker.Clear();
        }

        return reconciled == 0
            ? JobExecutionResult.NoOp("NoPendingProviderTransactionsWereReconciled")
            : JobExecutionResult.Completed(
                "PendingProviderTransactionsReconciled",
                reconciled);
    }

    private async Task<JobExecutionResult> ReconcileDepositAsync(
        PaymentTransaction paymentTransaction,
        CancellationToken cancellationToken)
    {
        if (!paymentTransaction.MilestoneId.HasValue)
        {
            return await EscalateIfStaleAsync(
                paymentTransaction,
                "DepositMilestoneIsMissing",
                "معاملة الإيداع غير مرتبطة بمرحلة صالحة للمطابقة.",
                cancellationToken);
        }

        var milestone = await dbContext.Milestones.SingleOrDefaultAsync(
            item => item.Id == paymentTransaction.MilestoneId.Value,
            cancellationToken);
        if (milestone is null
            || milestone.Status != MilestoneStatus.FundingProcessing)
        {
            return await EscalateIfStaleAsync(
                paymentTransaction,
                "MilestoneNoLongerAwaitingReconciliation",
                "حالة المرحلة لا تسمح بإكمال مطابقة معاملة الإيداع آليًا.",
                cancellationToken);
        }

        var correlationId = Guid.NewGuid();
        var result = await reconciliationProvider.GetDepositStatusAsync(
            new ProviderDepositStatusRequest(
                paymentTransaction.Amount,
                paymentTransaction.Currency,
                milestone.Id,
                paymentTransaction.IdempotencyKey,
                correlationId,
                paymentTransaction.ProviderTransactionId),
            cancellationToken);
        if (result is null || result.Outcome == ProviderOperationOutcome.Unknown)
        {
            return await EscalateIfStaleAsync(
                paymentTransaction,
                "ProviderOutcomeStillUnknown",
                "ظلت نتيجة معاملة الإيداع غير مؤكدة بعد تجاوز مهلة المطابقة.",
                cancellationToken);
        }

        EnsureResultMatches(result, paymentTransaction, milestone.Id);
        ApplyProviderResult(paymentTransaction, result);
        var contract = await dbContext.Contracts.AsNoTracking()
            .SingleOrDefaultAsync(
                item => item.Id == paymentTransaction.ContractId,
                cancellationToken)
            ?? throw new BusinessException(
                "العقد المرتبط بمعاملة الدفع غير موجود.");
        var reservationId =
            await paymentEscrowService.FindProcessingFundingReservationIdAsync(
                milestone.Id,
                cancellationToken);
        if (result.Outcome
            is ProviderOperationOutcome.Processing
                or ProviderOperationOutcome.RequiresCustomerAction)
        {
            ApplyProviderResult(paymentTransaction, result);
            await dbContext.SaveChangesAsync(cancellationToken);
            return JobExecutionResult.NoOp("ProviderOutcomeStillProcessing");
        }

        if (result.Outcome == ProviderOperationOutcome.Succeeded)
        {
            await paymentEscrowService.CompleteFundingAsync(
                milestone,
                contract.LawyerUserId,
                paymentTransaction,
                result,
                reservationId,
                null,
                correlationId,
                cancellationToken);
        }
        else
        {
            await paymentEscrowService.FinalizeFailedExternalResultAsync(
                milestone,
                paymentTransaction,
                result.ProviderTransactionId
                    ?? $"reconciled-failed-{paymentTransaction.Id:N}",
                reservationId,
                correlationId,
                cancellationToken);
        }

        return JobExecutionResult.Completed("ProviderTransactionReconciled");
    }

    private async Task<JobExecutionResult> ReconcileSettlementAsync(
        PaymentTransaction paymentTransaction,
        bool isRelease,
        CancellationToken cancellationToken)
    {
        if (!paymentTransaction.EscrowHoldId.HasValue)
        {
            throw new BusinessException(
                "معاملة التسوية غير مرتبطة بحجز ضمان صالح.");
        }

        var holdId = paymentTransaction.EscrowHoldId.Value;
        var correlationId = Guid.NewGuid();
        var result = isRelease
            ? await reconciliationProvider.GetReleaseStatusAsync(
                new ProviderReleaseStatusRequest(
                    paymentTransaction.Amount,
                    paymentTransaction.Currency,
                    holdId,
                    paymentTransaction.IdempotencyKey,
                    correlationId,
                    paymentTransaction.ProviderTransactionId),
                cancellationToken)
            : await reconciliationProvider.GetRefundStatusAsync(
                new ProviderRefundStatusRequest(
                    paymentTransaction.Amount,
                    paymentTransaction.Currency,
                    holdId,
                    paymentTransaction.IdempotencyKey,
                    correlationId,
                    paymentTransaction.ProviderTransactionId),
                cancellationToken);
        if (result is null || result.Outcome == ProviderOperationOutcome.Unknown)
        {
            return await EscalateIfStaleAsync(
                paymentTransaction,
                "ProviderOutcomeStillUnknown",
                "ظلت نتيجة معاملة التسوية غير مؤكدة بعد تجاوز مهلة المطابقة.",
                cancellationToken);
        }

        EnsureResultMatches(result, paymentTransaction, holdId);
        if (result.Outcome
            is ProviderOperationOutcome.Processing
                or ProviderOperationOutcome.RequiresCustomerAction)
        {
            ApplyProviderResult(paymentTransaction, result);
            await dbContext.SaveChangesAsync(cancellationToken);
            return JobExecutionResult.NoOp("ProviderOutcomeStillProcessing");
        }

        var now = timeProvider.GetUtcNow().UtcDateTime;
        if (result.Outcome == ProviderOperationOutcome.Failed)
        {
            bool? releaseRetryScheduled = null;
            if (isRelease)
            {
                releaseRetryScheduled = PaymentReleaseRetryPolicy
                    .RecordConfirmedFailure(
                        paymentTransaction,
                        result.FailureReason
                            ?? "أكد مزود الدفع فشل عملية تحرير حجز الضمان.",
                        now);
            }
            else
            {
                paymentTransaction.Status = PaymentTransactionStatus.Failed;
                paymentTransaction.FailureReason = result.FailureReason
                    ?? "أكد مزود الدفع فشل عملية التسوية.";
                paymentTransaction.RequiresManualAction = false;
                paymentTransaction.ManualActionRequiredAt = null;
                paymentTransaction.ProcessedAt = now;
                paymentTransaction.UpdatedAt = now;
            }

            await dbContext.SaveChangesAsync(cancellationToken);
            if (releaseRetryScheduled == false)
            {
                logger.LogError(
                    "Release transaction {PaymentTransactionId} requires manual action after {ProviderAttemptCount} provider attempts.",
                    paymentTransaction.Id,
                    paymentTransaction.ProviderAttemptCount);
            }

            return JobExecutionResult.Completed(
                releaseRetryScheduled.HasValue
                    ? releaseRetryScheduled.Value
                        ? "ReleaseRetryScheduled"
                        : "ReleaseRequiresManualAction"
                    : "ProviderSettlementFailureConfirmed");
        }

        if (string.IsNullOrWhiteSpace(result.ProviderTransactionId)
            || result.ProviderTransactionId.Length > 200)
        {
            throw new BusinessException(
                "أكد مزود الدفع نجاح التسوية دون إرسال معرّف صالح للمعاملة.");
        }

        paymentTransaction.ProviderTransactionId = result.ProviderTransactionId;
        ApplyProviderResult(paymentTransaction, result);
        if (isRelease)
        {
            PaymentReleaseRetryPolicy.RecordSuccess(
                paymentTransaction,
                now);
        }
        else
        {
            paymentTransaction.Status = PaymentTransactionStatus.Completed;
            paymentTransaction.FailureReason = null;
            paymentTransaction.RequiresManualAction = false;
            paymentTransaction.ManualActionRequiredAt = null;
            paymentTransaction.ProcessedAt = now;
            paymentTransaction.UpdatedAt = now;
        }
        await dbContext.SaveChangesAsync(cancellationToken);
        if (isRelease)
        {
            dbContext.ChangeTracker.Clear();
            var releaseResult = await escrowReleaseService.ReleaseExpiredHoldAsync(
                holdId,
                cancellationToken);
            if (releaseResult.Outcome == JobExecutionOutcome.Completed)
            {
                return releaseResult;
            }
        }

        return JobExecutionResult.Completed("ProviderSettlementReconciled");
    }

    private async Task<JobExecutionResult> EscalateIfStaleAsync(
        PaymentTransaction paymentTransaction,
        string pendingReason,
        string manualActionReason,
        CancellationToken cancellationToken)
    {
        if (!HasExceededProcessingSla(paymentTransaction))
        {
            return JobExecutionResult.NoOp(pendingReason);
        }

        return await RequireManualActionAsync(
            paymentTransaction,
            manualActionReason,
            exception: null,
            cancellationToken);
    }

    private async Task<JobExecutionResult> RequireManualActionAsync(
        PaymentTransaction paymentTransaction,
        string reason,
        Exception? exception,
        CancellationToken cancellationToken)
    {
        var now = timeProvider.GetUtcNow().UtcDateTime;
        paymentTransaction.RequiresManualAction = true;
        paymentTransaction.ManualActionRequiredAt = now;
        paymentTransaction.NextRetryAt = null;
        paymentTransaction.FailureReason = reason;
        paymentTransaction.UpdatedAt = now;
        await dbContext.SaveChangesAsync(cancellationToken);
        logger.LogCritical(
            exception,
            "Payment transaction {PaymentTransactionId} exceeded the processing SLA and requires manual action. Operation: {OperationType}; created: {CreatedAt}; escalated: {EscalatedAt}.",
            paymentTransaction.Id,
            paymentTransaction.OperationType,
            paymentTransaction.CreatedAt,
            now);
        return JobExecutionResult.Completed(
            "PaymentTransactionRequiresManualAction");
    }

    private bool HasExceededProcessingSla(
        PaymentTransaction paymentTransaction)
    {
        var cutoff = timeProvider.GetUtcNow().UtcDateTime.AddMinutes(
            -paymentProviderOptions.Value.ProcessingSlaMinutes);
        return paymentTransaction.CreatedAt <= cutoff;
    }

    private void EnsureResultMatches(
        ProviderResult result,
        PaymentTransaction transaction,
        Guid businessId)
    {
        if (result.Amount == transaction.Amount
            && string.Equals(
                result.Currency,
                transaction.Currency,
                StringComparison.Ordinal)
            && result.BusinessId == businessId
            && string.Equals(
                result.ProviderIdempotencyKey,
                transaction.IdempotencyKey,
                StringComparison.Ordinal))
        {
            return;
        }

        logger.LogWarning(
            "Rejected provider reconciliation result for transaction {PaymentTransactionId}: financial facts do not match.",
            transaction.Id);
        throw new BusinessException(
            "نتيجة مطابقة مزود الدفع لا تطابق بيانات معاملة الدفع الأصلية.");
    }

    private void ApplyProviderResult(
        PaymentTransaction transaction,
        ProviderResult result)
    {
        transaction.ProviderTransactionId = result.ProviderTransactionId;
        transaction.ProviderRelatedTransactionId =
            result.RelatedProviderTransactionId;
        transaction.ProviderStatus = result.ProviderStatus;
        transaction.ProviderObjectType = result.ProviderObjectType;
        transaction.ProviderAmountMinor = result.ProviderMoney?.AmountMinor;
        transaction.ProviderCurrency = result.ProviderMoney?.Currency;
        transaction.UpdatedAt = timeProvider.GetUtcNow().UtcDateTime;
    }
}
