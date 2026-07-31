using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SmartCourt.Common.Exceptions;
using SmartCourt.Features.Milestones.Enums;
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
    IPaymentReconciliationProvider reconciliationProvider,
    ILogger<PaymentReconciliationService> logger) : IPaymentReconciliationService
{
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
            return JobExecutionResult.NoOp(
                "PaymentTransactionNotFound");
        }

        if (paymentTransaction.Status
            != PaymentTransactionStatus.Processing)
        {
            return JobExecutionResult.NoOp(
                "PaymentTransactionAlreadyFinal");
        }

        if (paymentTransaction.OperationType
                != PaymentOperationType.Deposit
            || !paymentTransaction.MilestoneId.HasValue)
        {
            return JobExecutionResult.NoOp(
                "PaymentOperationNotSupported");
        }

        var milestone = await dbContext.Milestones
            .SingleOrDefaultAsync(
                item =>
                    item.Id
                        == paymentTransaction.MilestoneId.Value,
                cancellationToken);
        if (milestone is null
            || milestone.Status
                != MilestoneStatus.FundingProcessing)
        {
            return JobExecutionResult.NoOp(
                "MilestoneNoLongerAwaitingReconciliation");
        }

        var correlationId = Guid.NewGuid();
        var result =
            await reconciliationProvider.GetDepositStatusAsync(
                new ProviderDepositStatusRequest(
                    paymentTransaction.Amount,
                    paymentTransaction.Currency,
                    milestone.Id,
                    paymentTransaction.IdempotencyKey,
                    correlationId),
                cancellationToken);
        if (result is null
            || result.Outcome == ProviderOperationOutcome.Unknown)
        {
            return JobExecutionResult.NoOp(
                "ProviderOutcomeStillUnknown");
        }

        if (!ReconciliationResultMatches(
                result,
                paymentTransaction,
                milestone.Id))
        {
            logger.LogWarning(
                "Rejected provider reconciliation result for transaction {PaymentTransactionId}: financial facts do not match.",
                paymentTransaction.Id);
            throw new BusinessException(
                "نتيجة مطابقة مزود الدفع لا تطابق بيانات معاملة التمويل الأصلية.");
        }

        var contract = await dbContext.Contracts
            .AsNoTracking()
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

        return JobExecutionResult.Completed(
            "ProviderTransactionReconciled");
    }

    private static bool ReconciliationResultMatches(
        ProviderResult result,
        PaymentTransaction transaction,
        Guid milestoneId)
    {
        return result.Amount == transaction.Amount
            && string.Equals(
                result.Currency,
                transaction.Currency,
                StringComparison.Ordinal)
            && result.BusinessId == milestoneId
            && string.Equals(
                result.ProviderIdempotencyKey,
                transaction.IdempotencyKey,
                StringComparison.Ordinal);
    }
}
