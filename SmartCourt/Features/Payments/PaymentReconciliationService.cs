using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SmartCourt.Common.Exceptions;
using SmartCourt.Features.Milestones.Enums;
using SmartCourt.Features.Payments.Entities;
using SmartCourt.Features.Payments.Enums;
using SmartCourt.Infrastructure.Providers.Jobs;
using SmartCourt.Infrastructure.Providers.Payments;
using SmartCourt.Persistence;

namespace SmartCourt.Features.Payments;

public sealed class PaymentReconciliationService(
    ApplicationDbContext dbContext,
    IPaymentEscrowService paymentEscrowService,
    IEscrowReleaseService escrowReleaseService,
    IPaymentReconciliationProvider reconciliationProvider,
    TimeProvider timeProvider,
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

        if (paymentTransaction.Status != PaymentTransactionStatus.Processing)
        {
            return JobExecutionResult.NoOp("PaymentTransactionAlreadyFinal");
        }

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
            _ => JobExecutionResult.NoOp("PaymentOperationNotSupported")
        };
    }

    public async Task<JobExecutionResult>
        ReconcilePendingProviderTransactionsAsync(
            CancellationToken cancellationToken)
    {
        var transactionIds = await dbContext.PaymentTransactions
            .AsNoTracking()
            .Where(item => item.Status == PaymentTransactionStatus.Processing)
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
            return JobExecutionResult.NoOp("DepositMilestoneIsMissing");
        }

        var milestone = await dbContext.Milestones.SingleOrDefaultAsync(
            item => item.Id == paymentTransaction.MilestoneId.Value,
            cancellationToken);
        if (milestone is null
            || milestone.Status != MilestoneStatus.FundingProcessing)
        {
            return JobExecutionResult.NoOp(
                "MilestoneNoLongerAwaitingReconciliation");
        }

        var correlationId = Guid.NewGuid();
        var result = await reconciliationProvider.GetDepositStatusAsync(
            new ProviderDepositStatusRequest(
                paymentTransaction.Amount,
                paymentTransaction.Currency,
                milestone.Id,
                paymentTransaction.IdempotencyKey,
                correlationId),
            cancellationToken);
        if (result is null || result.Outcome == ProviderOperationOutcome.Unknown)
        {
            return JobExecutionResult.NoOp("ProviderOutcomeStillUnknown");
        }

        EnsureResultMatches(result, paymentTransaction, milestone.Id);
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
                    correlationId),
                cancellationToken)
            : await reconciliationProvider.GetRefundStatusAsync(
                new ProviderRefundStatusRequest(
                    paymentTransaction.Amount,
                    paymentTransaction.Currency,
                    holdId,
                    paymentTransaction.IdempotencyKey,
                    correlationId),
                cancellationToken);
        if (result is null || result.Outcome == ProviderOperationOutcome.Unknown)
        {
            return JobExecutionResult.NoOp("ProviderOutcomeStillUnknown");
        }

        EnsureResultMatches(result, paymentTransaction, holdId);
        var now = timeProvider.GetUtcNow().UtcDateTime;
        if (result.Outcome == ProviderOperationOutcome.Failed)
        {
            paymentTransaction.Status = PaymentTransactionStatus.Failed;
            paymentTransaction.FailureReason = result.FailureReason
                ?? "أكد مزود الدفع فشل عملية التسوية.";
            paymentTransaction.ProcessedAt = now;
            paymentTransaction.UpdatedAt = now;
            await dbContext.SaveChangesAsync(cancellationToken);
            return JobExecutionResult.Completed(
                "ProviderSettlementFailureConfirmed");
        }

        if (string.IsNullOrWhiteSpace(result.ProviderTransactionId)
            || result.ProviderTransactionId.Length > 200)
        {
            throw new BusinessException(
                "أكد مزود الدفع نجاح التسوية دون إرسال معرّف صالح للمعاملة.");
        }

        paymentTransaction.ProviderTransactionId = result.ProviderTransactionId;
        paymentTransaction.Status = PaymentTransactionStatus.Completed;
        paymentTransaction.FailureReason = null;
        paymentTransaction.ProcessedAt = now;
        paymentTransaction.UpdatedAt = now;
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
}
