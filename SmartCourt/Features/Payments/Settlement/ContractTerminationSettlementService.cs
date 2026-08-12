using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using SmartCourt.Common.Exceptions;
using SmartCourt.Features.Disputes.Enums;
using SmartCourt.Features.Milestones.Domain;
using SmartCourt.Features.Milestones.Entities;
using SmartCourt.Features.Milestones.Enums;
using SmartCourt.Features.Payments.Domain;
using SmartCourt.Features.Payments.Entities;
using SmartCourt.Features.Payments.Enums;
using SmartCourt.Features.Payments.Integration;
using SmartCourt.Infrastructure.Idempotency;
using SmartCourt.Infrastructure.Persistence;
using SmartCourt.Infrastructure.Persistence.Entities;
using SmartCourt.Infrastructure.Providers.Events;
using SmartCourt.Infrastructure.Providers.Payments;
using SmartCourt.Persistence;

namespace SmartCourt.Features.Payments;

public sealed class ContractTerminationSettlementService(
    ApplicationDbContext dbContext,
    IPaymentProvider paymentProvider,
    IOutboxWriter outboxWriter,
    TimeProvider timeProvider,
    ILogger<ContractTerminationSettlementService> logger)
    : IContractTerminationSettlementService
{
    private const string RefundOperation = "TerminationRefund";
    private const string RefundReferenceType = "ContractTermination";

    public async Task<ContractTerminationSettlement>
        SettleForTerminationAsync(
            Guid contractId,
            Guid actorUserId,
            string reason,
            Guid correlationId,
            CancellationToken cancellationToken)
    {
        if (contractId == Guid.Empty || actorUserId == Guid.Empty)
        {
            throw new BusinessException(
                "تعذر بدء تسوية إنهاء العقد لأن بيانات العقد أو المستخدم غير مكتملة.");
        }

        await using var transaction =
            await SerializableOperationTransaction.CreateAsync(
                dbContext,
                cancellationToken);
        var contract = await dbContext.Contracts.SingleOrDefaultAsync(
            item => item.Id == contractId,
            cancellationToken)
            ?? throw new BusinessException(
                "العقد المطلوب تسويته غير موجود.");
        var milestones = await dbContext.Milestones
            .Where(item => item.ContractId == contractId)
            .ToListAsync(cancellationToken);
        var holds = await dbContext.EscrowHolds
            .Where(item => item.ContractId == contractId)
            .ToListAsync(cancellationToken);

        var hasFundingInProgress =
            milestones.Any(item =>
                item.Status == MilestoneStatus.FundingProcessing)
            || await dbContext.PaymentTransactions.AnyAsync(
                item =>
                    item.ContractId == contractId
                    && item.OperationType == PaymentOperationType.Deposit
                    && item.Status == PaymentTransactionStatus.Processing,
                cancellationToken);
        if (hasFundingInProgress)
        {
            await transaction.CommitAndCloseAsync(cancellationToken);
            return Pending();
        }

        var unsettledHolds = holds
            .Where(item => item.Status is EscrowHoldStatus.Funded
                or EscrowHoldStatus.Frozen)
            .ToArray();
        if (unsettledHolds.Length == 0)
        {
            await transaction.CommitAndCloseAsync(cancellationToken);
            return Completed();
        }

        var milestoneById = milestones.ToDictionary(item => item.Id);
        if (unsettledHolds.Any(hold =>
                !milestoneById.TryGetValue(hold.MilestoneId, out var milestone)
                || !IsEligibleUnstartedHold(hold, milestone)))
        {
            await transaction.CommitAndCloseAsync(cancellationToken);
            return Pending();
        }

        var eligibleMilestoneIds = unsettledHolds
            .Select(item => item.MilestoneId)
            .ToArray();
        var hasSubmission = await dbContext.MilestoneSubmissions.AnyAsync(
            item => eligibleMilestoneIds.Contains(item.MilestoneId),
            cancellationToken);
        var hasActiveDispute = await dbContext.Disputes.AnyAsync(
            item =>
                item.ContractId == contractId
                && item.Status != DisputeStatus.Resolved
                && item.Status != DisputeStatus.Closed,
            cancellationToken);
        if (hasSubmission || hasActiveDispute)
        {
            await transaction.CommitAndCloseAsync(cancellationToken);
            return Pending();
        }

        decimal grossRefunded = 0m;
        foreach (var hold in unsettledHolds)
        {
            var milestone = milestoneById[hold.MilestoneId];
            var refunded = await RefundAsync(
                contract.LawyerUserId,
                milestone,
                hold,
                actorUserId,
                reason,
                correlationId,
                transaction,
                cancellationToken);
            if (!refunded)
            {
                await dbContext.SaveChangesAsync(cancellationToken);
                await transaction.CommitAndCloseAsync(cancellationToken);
                return Pending();
            }

            grossRefunded += hold.GrossAmount;
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAndCloseAsync(cancellationToken);
        logger.LogInformation(
            "Refunded {Amount} EGP from contract {ContractId} before termination.",
            grossRefunded,
            contractId);
        return new ContractTerminationSettlement(
            true,
            grossRefunded,
            grossRefunded,
            0m,
            0m);
    }

    private async Task<bool> RefundAsync(
        Guid lawyerUserId,
        Milestone milestone,
        EscrowHold hold,
        Guid actorUserId,
        string reason,
        Guid correlationId,
        SerializableOperationTransaction transaction,
        CancellationToken cancellationToken)
    {
        var account = await dbContext.EscrowAccounts.SingleOrDefaultAsync(
            item => item.Id == hold.EscrowAccountId,
            cancellationToken);
        var wallet = await dbContext.LawyerWallets.SingleOrDefaultAsync(
            item => item.LawyerUserId == lawyerUserId,
            cancellationToken);
        if (account is null
            || wallet is null
            || account.ContractId != hold.ContractId
            || !FinancialStateIsValid(hold, account, wallet))
        {
            throw new BusinessException(
                "تعذر رد تمويل المرحلة لأن أرصدة الضمان أو المحفظة غير متطابقة.");
        }

        var now = timeProvider.GetUtcNow().UtcDateTime;
        var requestHash = CreateRequestHash(hold);
        var settlement = await dbContext.IdempotencyRecords
            .SingleOrDefaultAsync(
                item =>
                    item.ResourceType
                        == IdempotencyScope.HoldSettlementResourceType
                    && item.ResourceId == hold.Id,
                cancellationToken);
        if (settlement is not null
            && (!string.Equals(
                    settlement.Operation,
                    RefundOperation,
                    StringComparison.Ordinal)
                || !string.Equals(
                    settlement.RequestHash,
                    requestHash,
                    StringComparison.Ordinal)))
        {
            return false;
        }

        var refundTransactions = await dbContext.PaymentTransactions
            .Where(
                item =>
                    item.EscrowHoldId == hold.Id
                    && item.OperationType == PaymentOperationType.Refund)
            .OrderBy(item => item.CreatedAt)
            .ToListAsync(cancellationToken);
        var refundTransaction = refundTransactions.FirstOrDefault(item =>
                item.Status == PaymentTransactionStatus.Completed)
            ?? refundTransactions.FirstOrDefault(item =>
                item.Status == PaymentTransactionStatus.Processing)
            ?? refundTransactions.LastOrDefault();
        if (settlement is null)
        {
            if (refundTransaction is not null)
            {
                return false;
            }

            var providerIdempotencyKey =
                $"termination-refund-{hold.Id:N}";
            refundTransaction = new PaymentTransaction(
                Guid.NewGuid(),
                hold.ContractId,
                hold.MilestoneId,
                PaymentOperationType.Refund,
                paymentProvider.GetType().Name,
                providerIdempotencyKey,
                hold.GrossAmount,
                now)
            {
                EscrowHoldId = hold.Id
            };
            settlement = new IdempotencyRecord(
                Guid.NewGuid(),
                actorUserId,
                providerIdempotencyKey,
                RefundOperation,
                IdempotencyScope.HoldSettlementResourceType,
                hold.Id,
                requestHash,
                now.AddDays(30),
                now);
            dbContext.PaymentTransactions.Add(refundTransaction);
            dbContext.IdempotencyRecords.Add(settlement);
        }
        else if (refundTransaction is null)
        {
            return false;
        }
        else if (refundTransaction.Status == PaymentTransactionStatus.Failed)
        {
            var attemptNumber = refundTransactions.Count + 1;
            refundTransaction = new PaymentTransaction(
                Guid.NewGuid(),
                hold.ContractId,
                hold.MilestoneId,
                PaymentOperationType.Refund,
                paymentProvider.GetType().Name,
                $"termination-refund-{hold.Id:N}-{attemptNumber}",
                hold.GrossAmount,
                now)
            {
                EscrowHoldId = hold.Id
            };
            dbContext.PaymentTransactions.Add(refundTransaction);
        }

        ProviderResult? providerResult = null;
        if (refundTransaction.Status != PaymentTransactionStatus.Completed)
        {
            string sourceProviderTransactionId = string.Empty;
            if (paymentProvider is ILawyerPayoutAccountProvider)
            {
                var depositTransaction = await dbContext.PaymentTransactions
                    .AsNoTracking()
                    .SingleOrDefaultAsync(
                        item => item.Id == hold.ProviderDepositTransactionId
                            && item.OperationType == PaymentOperationType.Deposit
                            && item.Status == PaymentTransactionStatus.Completed,
                        cancellationToken);
                if (depositTransaction is null
                    || string.IsNullOrWhiteSpace(
                        depositTransaction.ProviderTransactionId))
                {
                    refundTransaction.FailureReason =
                        "معرّف عملية الإيداع لدى مزود الدفع غير متاح لتنفيذ الرد.";
                    refundTransaction.UpdatedAt = now;
                    return false;
                }

                sourceProviderTransactionId =
                    depositTransaction.ProviderTransactionId;
            }

            var providerRequest = new ProviderRefundRequest(
                hold.GrossAmount,
                account.Currency,
                hold.Id,
                refundTransaction.IdempotencyKey,
                refundTransaction.Id,
                reason,
                sourceProviderTransactionId);
            await dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAndCloseAsync(cancellationToken);
            try
            {
                providerResult = await paymentProvider.RefundAsync(
                    providerRequest,
                    cancellationToken);
            }
            catch (OperationCanceledException)
                when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception exception)
            {
                await transaction.BeginAsync(cancellationToken);
                refundTransaction.Status =
                    PaymentTransactionStatus.Processing;
                refundTransaction.FailureReason =
                    "تعذر التأكد من نتيجة رد التمويل لدى مزود الدفع.";
                refundTransaction.UpdatedAt = now;
                logger.LogWarning(
                    exception,
                    "Unable to confirm termination refund for hold {EscrowHoldId}.",
                    hold.Id);
                return false;
            }

            await transaction.BeginAsync(cancellationToken);
            if (!ProviderResultMatches(providerResult, providerRequest))
            {
                refundTransaction.Status =
                    PaymentTransactionStatus.Processing;
                refundTransaction.FailureReason =
                    "بيانات نتيجة مزود الدفع لا تطابق طلب رد التمويل.";
                refundTransaction.UpdatedAt = now;
                return false;
            }

            if (providerResult.Outcome == ProviderOperationOutcome.Failed)
            {
                refundTransaction.Status = PaymentTransactionStatus.Failed;
                refundTransaction.FailureReason =
                    providerResult.FailureReason
                    ?? "رفض مزود الدفع عملية رد التمويل.";
                refundTransaction.UpdatedAt = now;
                return false;
            }

            if (providerResult.Outcome != ProviderOperationOutcome.Succeeded)
            {
                refundTransaction.Status =
                    PaymentTransactionStatus.Processing;
                refundTransaction.FailureReason =
                    providerResult.FailureReason
                    ?? "لم تكتمل عملية رد التمويل لدى مزود الدفع.";
                refundTransaction.UpdatedAt = now;
                return false;
            }

            if (string.IsNullOrWhiteSpace(
                    providerResult.ProviderTransactionId)
                || providerResult.ProviderTransactionId.Length > 200)
            {
                refundTransaction.Status =
                    PaymentTransactionStatus.Processing;
                refundTransaction.FailureReason =
                    "لم يرسل مزود الدفع معرّفًا صالحًا لعملية رد التمويل.";
                refundTransaction.UpdatedAt = now;
                return false;
            }

            refundTransaction.ProviderTransactionId =
                providerResult.ProviderTransactionId;
            refundTransaction.ProviderRelatedTransactionId =
                providerResult.RelatedProviderTransactionId;
            refundTransaction.ProviderStatus = providerResult.ProviderStatus;
            refundTransaction.ProviderObjectType =
                providerResult.ProviderObjectType;
            refundTransaction.ProviderAmountMinor =
                providerResult.ProviderMoney?.AmountMinor;
            refundTransaction.ProviderCurrency =
                providerResult.ProviderMoney?.Currency;
            refundTransaction.Status =
                PaymentTransactionStatus.Completed;
            refundTransaction.FailureReason = null;
            refundTransaction.ProcessedAt = now;
            refundTransaction.UpdatedAt = now;
        }

        var currentBalance = CurrentBalance(account);
        var remainingBalance = currentBalance - hold.GrossAmount;
        if (remainingBalance < 0m)
        {
            throw new BusinessException(
                "رصيد حساب الضمان لا يكفي لرد تمويل المرحلة.");
        }

        dbContext.EscrowLedgerEntries.Add(
            new EscrowLedgerEntry(
                Guid.NewGuid(),
                account.Id,
                hold.Id,
                LedgerTransactionType.Refund,
                hold.GrossAmount,
                remainingBalance,
                RefundReferenceType,
                milestone.Id,
                refundTransaction.Id,
                "رد كامل تمويل المرحلة غير المنفذة قبل إنهاء العقد.",
                actorUserId,
                correlationId,
                now));
        account.TotalRefunded += hold.GrossAmount;
        account.UpdatedAt = now;
        wallet.PendingBalance -= hold.NetAmount;
        wallet.UpdatedAt = now;

        EscrowHoldTransitionGuard.EnsureCanTransition(
            hold.Status,
            EscrowHoldStatus.Refunded);
        hold.Status = EscrowHoldStatus.Refunded;
        hold.SettledAt = now;
        hold.SettlementType = SettlementType.Refund;
        hold.ProviderRefundTransactionId = refundTransaction.Id;
        hold.UpdatedAt = now;
        MilestoneTransitionGuard.EnsureCanTransition(
            milestone.Status,
            MilestoneStatus.Refunded);
        var previousStatus = milestone.Status;
        milestone.Status = MilestoneStatus.Refunded;
        milestone.RefundedAt = now;
        milestone.UpdatedAt = now;
        dbContext.MilestoneStateHistories.Add(
            MilestoneStateHistoryFactory.Create(
                Guid.NewGuid(),
                milestone.Id,
                previousStatus,
                MilestoneStatus.Refunded,
                ContractPaymentEventTypes.FundsRefunded,
                actorUserId,
                "تم رد كامل تمويل المرحلة غير المنفذة قبل إنهاء العقد.",
                correlationId,
                now));
        await outboxWriter.EnqueueAsync(
            new OutboxEvent(
                ContractPaymentEventTypes.FundsRefunded,
                1,
                new FundsRefundedEventPayload(
                    milestone.Id,
                    hold.Id,
                    refundTransaction.Id,
                    hold.GrossAmount),
                "EscrowHold",
                hold.Id,
                correlationId),
            cancellationToken);
        settlement.Complete(
            200,
            JsonSerializer.Serialize(
                new
                {
                    refundTransaction.ProviderTransactionId,
                    Outcome = "Completed"
                }),
            refundTransaction.Id,
            now);
        return true;
    }

    private static bool IsEligibleUnstartedHold(
        EscrowHold hold,
        Milestone milestone)
    {
        return hold.Status == EscrowHoldStatus.Funded
            && milestone.Status == MilestoneStatus.FundedInProgress
            && !milestone.SubmittedAt.HasValue
            && !milestone.AcceptedAt.HasValue
            && !milestone.HoldStartsAt.HasValue
            && !milestone.HoldExpiresAt.HasValue;
    }

    private static bool FinancialStateIsValid(
        EscrowHold hold,
        EscrowAccount account,
        LawyerWallet wallet)
    {
        return string.Equals(
                account.Currency,
                "EGP",
                StringComparison.Ordinal)
            && string.Equals(
                wallet.Currency,
                "EGP",
                StringComparison.Ordinal)
            && hold.GrossAmount
                == hold.NetAmount + hold.PlatformFeeAmount
            && hold.GrossAmount > 0m
            && wallet.PendingBalance >= hold.NetAmount
            && CurrentBalance(account) >= hold.GrossAmount;
    }

    private static decimal CurrentBalance(EscrowAccount account)
    {
        return account.TotalDeposited
            - account.TotalReleased
            - account.TotalRefunded
            - account.TotalFees;
    }

    private static string CreateRequestHash(EscrowHold hold)
    {
        var canonical =
            $"{RefundOperation}:{hold.Id:N}:{hold.GrossAmount:F2}:EGP";
        return Convert.ToHexString(
            SHA256.HashData(Encoding.UTF8.GetBytes(canonical)));
    }

    private static bool ProviderResultMatches(
        ProviderResult result,
        ProviderRefundRequest request)
    {
        return result.Amount == request.Amount
            && string.Equals(
                result.Currency,
                request.Currency,
                StringComparison.Ordinal)
            && result.BusinessId == request.BusinessId
            && string.Equals(
                result.ProviderIdempotencyKey,
                request.ProviderIdempotencyKey,
                StringComparison.Ordinal)
            && result.CorrelationId == request.CorrelationId;
    }

    private static ContractTerminationSettlement Pending()
        => new(false, 0m, 0m, 0m, 0m);

    private static ContractTerminationSettlement Completed()
        => new(true, 0m, 0m, 0m, 0m);
}
