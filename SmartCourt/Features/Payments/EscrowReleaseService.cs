using System.Data;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using SmartCourt.Common.Exceptions;
using SmartCourt.Features.Contracts;
using SmartCourt.Features.Disputes.Enums;
using SmartCourt.Features.Milestones.Domain;
using SmartCourt.Features.Milestones.Enums;
using SmartCourt.Features.Payments.Domain;
using SmartCourt.Features.Payments.Entities;
using SmartCourt.Features.Payments.Enums;
using SmartCourt.Infrastructure.Idempotency;
using SmartCourt.Infrastructure.Persistence.Entities;
using SmartCourt.Infrastructure.Persistence.Enums;
using SmartCourt.Infrastructure.Providers.Events;
using SmartCourt.Infrastructure.Providers.Jobs;
using SmartCourt.Infrastructure.Providers.Payments;
using SmartCourt.Persistence;

namespace SmartCourt.Features.Payments;

public sealed class EscrowReleaseService(
    ApplicationDbContext dbContext,
    IPaymentProvider paymentProvider,
    IOutboxWriter outboxWriter,
    IContractCompletionEvaluator completionEvaluator,
    TimeProvider timeProvider,
    ILogger<EscrowReleaseService> logger)
    : IEscrowReleaseService
{
    private const string ReleaseOperation = "ReleaseExpiredHold";
    private const string ReleaseReferenceType = "MilestoneRelease";

    public async Task<JobExecutionResult> ReleaseExpiredHoldAsync(
        Guid escrowHoldId,
        CancellationToken cancellationToken)
    {
        if (escrowHoldId == Guid.Empty)
        {
            return NoOp("InvalidEscrowHoldId", escrowHoldId);
        }

        await using var transaction = dbContext.Database.IsRelational()
            ? await dbContext.Database.BeginTransactionAsync(
                IsolationLevel.Serializable,
                cancellationToken)
            : null;
        var hold = await dbContext.EscrowHolds.SingleOrDefaultAsync(
            item => item.Id == escrowHoldId,
            cancellationToken);
        if (hold is null)
        {
            return NoOp("EscrowHoldNotFound", escrowHoldId);
        }

        if (hold.Status != EscrowHoldStatus.Funded)
        {
            return NoOp("EscrowHoldNoLongerFunded", escrowHoldId);
        }

        var milestone = await dbContext.Milestones
            .SingleOrDefaultAsync(
                item => item.Id == hold.MilestoneId,
                cancellationToken);
        if (milestone is null
            || milestone.ContractId != hold.ContractId
            || milestone.Status != MilestoneStatus.AcceptedHold)
        {
            return NoOp(
                "MilestoneNoLongerInAcceptedHold",
                escrowHoldId);
        }

        var now = timeProvider.GetUtcNow().UtcDateTime;
        if (!hold.HoldExpiresAt.HasValue
            || !milestone.HoldExpiresAt.HasValue
            || hold.HoldExpiresAt.Value
                != milestone.HoldExpiresAt.Value)
        {
            return NoOp("HoldExpiryIsInvalid", escrowHoldId);
        }

        if (hold.HoldExpiresAt.Value > now)
        {
            return NoOp("HoldReleaseDeadlineNotElapsed", escrowHoldId);
        }

        var hasActiveDispute = await dbContext.Disputes.AnyAsync(
            dispute =>
                dispute.MilestoneId == milestone.Id
                && dispute.Status != DisputeStatus.Resolved
                && dispute.Status != DisputeStatus.Closed,
            cancellationToken);
        if (hasActiveDispute)
        {
            return NoOp("ActiveDisputeExists", escrowHoldId);
        }

        var account = await dbContext.EscrowAccounts
            .SingleOrDefaultAsync(
                item => item.Id == hold.EscrowAccountId,
                cancellationToken);
        var contract = await dbContext.Contracts
            .SingleOrDefaultAsync(
                item => item.Id == hold.ContractId,
                cancellationToken);
        if (account is null
            || contract is null
            || account.ContractId != hold.ContractId)
        {
            return NoOp(
                "EscrowReleaseOwnershipIsInvalid",
                escrowHoldId);
        }

        var wallet = await dbContext.LawyerWallets
            .SingleOrDefaultAsync(
                item => item.LawyerUserId == contract.LawyerUserId,
                cancellationToken);
        if (wallet is null)
        {
            return NoOp("LawyerWalletNotFound", escrowHoldId);
        }

        if (!FinancialStateIsValid(hold, account, wallet))
        {
            return NoOp(
                "EscrowReleaseFinancialStateIsInvalid",
                escrowHoldId);
        }

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
                    ReleaseOperation,
                    StringComparison.Ordinal)
                || !string.Equals(
                    settlement.RequestHash,
                    requestHash,
                    StringComparison.Ordinal)))
        {
            return NoOp(
                "EscrowHoldHasDifferentSettlementReservation",
                escrowHoldId);
        }

        var releaseTransaction = await dbContext.PaymentTransactions
            .SingleOrDefaultAsync(
                item =>
                    item.EscrowHoldId == hold.Id
                    && item.OperationType
                        == PaymentOperationType.Release,
                cancellationToken);
        if (settlement is null)
        {
            if (releaseTransaction is not null)
            {
                return NoOp(
                    "ReleaseAttemptHasNoSettlementReservation",
                    escrowHoldId);
            }

            var providerIdempotencyKey =
                $"release-{hold.Id:N}";
            releaseTransaction = new PaymentTransaction(
                Guid.NewGuid(),
                hold.ContractId,
                hold.MilestoneId,
                PaymentOperationType.Release,
                paymentProvider.GetType().Name,
                providerIdempotencyKey,
                hold.GrossAmount,
                now)
            {
                EscrowHoldId = hold.Id
            };
            settlement = new IdempotencyRecord(
                Guid.NewGuid(),
                contract.LawyerUserId,
                providerIdempotencyKey,
                ReleaseOperation,
                IdempotencyScope.HoldSettlementResourceType,
                hold.Id,
                requestHash,
                now.AddDays(30),
                now);
            dbContext.PaymentTransactions.Add(releaseTransaction);
            dbContext.IdempotencyRecords.Add(settlement);
        }
        else if (releaseTransaction is null)
        {
            return NoOp(
                "SettlementReservationHasNoReleaseAttempt",
                escrowHoldId);
        }

        if (releaseTransaction.Status
            == PaymentTransactionStatus.Failed)
        {
            return NoOp(
                "ReleaseProviderAttemptFailed",
                escrowHoldId);
        }

        ProviderResult? providerResult = null;
        if (releaseTransaction.Status
            != PaymentTransactionStatus.Completed)
        {
            var providerRequest = new ProviderReleaseRequest(
                hold.GrossAmount,
                account.Currency,
                hold.Id,
                releaseTransaction.IdempotencyKey,
                releaseTransaction.Id);
            try
            {
                providerResult = await paymentProvider.ReleaseAsync(
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
                releaseTransaction.FailureReason =
                    "تعذر التأكد من نتيجة تحرير حجز الضمان لدى مزود الدفع.";
                releaseTransaction.UpdatedAt = now;
                await SaveAttemptAndCommitAsync(
                    transaction,
                    cancellationToken);
                throw new BusinessException(
                    "تعذر التأكد من نتيجة تحرير حجز الضمان. ستظل الأموال محجوزة لحين إعادة المحاولة.",
                    exception);
            }

            if (!ProviderResultMatches(
                    providerResult,
                    providerRequest))
            {
                releaseTransaction.FailureReason =
                    "بيانات نتيجة مزود الدفع لا تطابق طلب تحرير حجز الضمان.";
                releaseTransaction.UpdatedAt = now;
                await SaveAttemptAndCommitAsync(
                    transaction,
                    cancellationToken);
                throw new BusinessException(
                    "تعذر التحقق من نتيجة تحرير حجز الضمان. ستظل الأموال محجوزة لحين المراجعة.");
            }

            if (providerResult.Outcome
                != ProviderOperationOutcome.Succeeded)
            {
                releaseTransaction.Status =
                    providerResult.Outcome
                        == ProviderOperationOutcome.Failed
                    ? PaymentTransactionStatus.Failed
                    : PaymentTransactionStatus.Processing;
                releaseTransaction.FailureReason =
                    providerResult.FailureReason
                    ?? "لم ينجح مزود الدفع في تحرير حجز الضمان.";
                releaseTransaction.ProcessedAt =
                    providerResult.Outcome
                        == ProviderOperationOutcome.Failed
                    ? now
                    : null;
                releaseTransaction.UpdatedAt = now;
                await SaveAttemptAndCommitAsync(
                    transaction,
                    cancellationToken);
                return NoOp(
                    providerResult.Outcome
                        == ProviderOperationOutcome.Failed
                        ? "ReleaseProviderConfirmedFailure"
                        : "ReleaseProviderOutcomeUnknown",
                    escrowHoldId);
            }

            if (string.IsNullOrWhiteSpace(
                    providerResult.ProviderTransactionId)
                || providerResult.ProviderTransactionId.Length > 200)
            {
                releaseTransaction.FailureReason =
                    "لم يرسل مزود الدفع معرّفًا صالحًا لعملية التحرير الناجحة.";
                releaseTransaction.UpdatedAt = now;
                await SaveAttemptAndCommitAsync(
                    transaction,
                    cancellationToken);
                throw new BusinessException(
                    "تعذر توثيق نتيجة تحرير حجز الضمان. ستظل العملية معلقة لحين المراجعة.");
            }

            releaseTransaction.ProviderTransactionId =
                providerResult.ProviderTransactionId;
            releaseTransaction.Status =
                PaymentTransactionStatus.Completed;
            releaseTransaction.FailureReason = null;
            releaseTransaction.ProcessedAt = now;
            releaseTransaction.UpdatedAt = now;
        }

        var currentBalance = CurrentBalance(account);
        var afterRelease = currentBalance - hold.NetAmount;
        var afterFee = afterRelease - hold.PlatformFeeAmount;
        if (afterRelease < 0m || afterFee < 0m)
        {
            throw new BusinessException(
                "رصيد حساب الضمان لا يكفي لإتمام تحرير أموال المرحلة.");
        }

        var correlationId = Guid.NewGuid();
        dbContext.EscrowLedgerEntries.AddRange(
            new EscrowLedgerEntry(
                Guid.NewGuid(),
                account.Id,
                hold.Id,
                LedgerTransactionType.Release,
                hold.NetAmount,
                afterRelease,
                ReleaseReferenceType,
                milestone.Id,
                releaseTransaction.Id,
                "تحرير صافي مستحقات المحامي من حجز ضمان المرحلة.",
                null,
                correlationId,
                now),
            new EscrowLedgerEntry(
                Guid.NewGuid(),
                account.Id,
                hold.Id,
                LedgerTransactionType.PlatformFee,
                hold.PlatformFeeAmount,
                afterFee,
                ReleaseReferenceType,
                milestone.Id,
                releaseTransaction.Id,
                "تسجيل رسوم المنصة المستحقة عن المرحلة.",
                null,
                correlationId,
                now));
        account.TotalReleased += hold.NetAmount;
        account.TotalFees += hold.PlatformFeeAmount;
        account.UpdatedAt = now;
        wallet.PendingBalance -= hold.NetAmount;
        wallet.AvailableBalance += hold.NetAmount;
        wallet.UpdatedAt = now;

        EscrowHoldTransitionGuard.EnsureCanTransition(
            hold.Status,
            EscrowHoldStatus.Released);
        hold.Status = EscrowHoldStatus.Released;
        hold.SettledAt = now;
        hold.SettlementType = SettlementType.Release;
        hold.ProviderReleaseTransactionId = releaseTransaction.Id;
        hold.UpdatedAt = now;
        MilestoneTransitionGuard.EnsureCanTransition(
            milestone.Status,
            MilestoneStatus.Released);
        milestone.Status = MilestoneStatus.Released;
        milestone.ReleasedAt = now;
        milestone.UpdatedAt = now;
        dbContext.MilestoneStateHistories.Add(
            MilestoneStateHistoryFactory.Create(
                Guid.NewGuid(),
                milestone.Id,
                MilestoneStatus.AcceptedHold,
                MilestoneStatus.Released,
                ContractPaymentEventTypes.FundsReleased,
                actorUserId: null,
                "انتهت مدة الحجز وتم تحرير مستحقات المحامي ورسوم المنصة.",
                correlationId,
                now));
        await outboxWriter.EnqueueAsync(
            new OutboxEvent(
                ContractPaymentEventTypes.FundsReleased,
                1,
                new FundsReleasedEventPayload(
                    milestone.Id,
                    hold.Id,
                    releaseTransaction.Id,
                    hold.NetAmount,
                    hold.PlatformFeeAmount),
                "EscrowHold",
                hold.Id,
                correlationId),
            cancellationToken);
        var settlementResponseBody = providerResult is null
            ? JsonSerializer.Serialize(
                new
                {
                    releaseTransaction.ProviderTransactionId,
                    Outcome = "Completed"
                })
            : JsonSerializer.Serialize(providerResult);
        settlement.Complete(
            200,
            settlementResponseBody,
            releaseTransaction.Id,
            now);

        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
            if (transaction is not null)
            {
                await transaction.CommitAsync(cancellationToken);
            }
        }
        catch (DbUpdateConcurrencyException)
        {
            return NoOp(
                "EscrowReleaseChangedConcurrently",
                escrowHoldId);
        }
        catch (DbUpdateException)
        {
            return NoOp(
                "EscrowReleaseAlreadyReservedOrSettled",
                escrowHoldId);
        }

        logger.LogInformation(
            "Escrow hold {EscrowHoldId} released. Net: {NetAmount}; fee: {PlatformFeeAmount}.",
            hold.Id,
            hold.NetAmount,
            hold.PlatformFeeAmount);
        await completionEvaluator.EvaluateCompletionAsync(
            contract.Id,
            cancellationToken);
        return JobExecutionResult.Completed("EscrowHoldReleased");
    }

    private async Task SaveAttemptAndCommitAsync(
        Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction?
            transaction,
        CancellationToken cancellationToken)
    {
        await dbContext.SaveChangesAsync(cancellationToken);
        if (transaction is not null)
        {
            await transaction.CommitAsync(cancellationToken);
        }
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
            && hold.NetAmount > 0m
            && hold.PlatformFeeAmount > 0m
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
            $"{ReleaseOperation}:{hold.Id:N}:{hold.GrossAmount:F2}:EGP";
        return Convert.ToHexString(
            SHA256.HashData(Encoding.UTF8.GetBytes(canonical)));
    }

    private static bool ProviderResultMatches(
        ProviderResult result,
        ProviderReleaseRequest request)
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

    private JobExecutionResult NoOp(
        string reason,
        Guid escrowHoldId)
    {
        logger.LogInformation(
            "Escrow release no-op for hold {EscrowHoldId}. Reason: {Reason}.",
            escrowHoldId,
            reason);
        return JobExecutionResult.NoOp(reason);
    }
}
