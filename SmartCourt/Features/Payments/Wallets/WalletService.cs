using System.Data;
using System.Globalization;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;
using SmartCourt.Common.Exceptions;
using SmartCourt.Features.Payments.DTOs;
using SmartCourt.Features.Payments.Entities;
using SmartCourt.Features.Payments.Enums;
using SmartCourt.Infrastructure.Idempotency;
using SmartCourt.Infrastructure.Persistence.Enums;
using SmartCourt.Infrastructure.Providers.Events;
using SmartCourt.Infrastructure.Providers.Jobs;
using SmartCourt.Infrastructure.Providers.Payments;
using SmartCourt.Interfaces;
using SmartCourt.Persistence;
using SmartCourt.Providers.Payments;

namespace SmartCourt.Features.Payments;

public sealed class WalletService(
    ApplicationDbContext dbContext,
    ICurrentUserService currentUserService,
    IPaymentProvider paymentProvider,
    IPaymentReconciliationProvider reconciliationProvider,
    IIdempotencyService idempotencyService,
    IOutboxWriter outboxWriter,
    TimeProvider timeProvider,
    IOptions<PaymentProviderOptions> paymentProviderOptions,
    ILogger<WalletService> logger) : IWalletService
{
    private const string WithdrawalOperation = "CreateWithdrawal";
    private const string WalletResource = "LawyerWallet";
    private static readonly JsonSerializerOptions SerializerOptions =
        new(JsonSerializerDefaults.Web);

    public async Task<WalletDto> GetAsync(
        CancellationToken cancellationToken)
    {
        var lawyerUserId = GetActorUserId();
        var wallet = await dbContext.LawyerWallets
            .AsNoTracking()
            .SingleOrDefaultAsync(
                item => item.LawyerUserId == lawyerUserId,
                cancellationToken);
        var totalReleased = await dbContext.EscrowAccounts
            .Where(account => dbContext.Contracts.Any(contract =>
                contract.Id == account.ContractId
                && contract.LawyerUserId == lawyerUserId))
            .SumAsync(
                account => (decimal?)account.TotalReleased,
                cancellationToken)
            ?? 0m;
        totalReleased += await dbContext.ConsultationPaymentTransactions
            .Where(transaction => transaction.OperationType == PaymentOperationType.Release
                && transaction.Status == PaymentTransactionStatus.Completed
                && dbContext.ConsultationBookings.Any(booking =>
                    booking.Id == transaction.BookingId
                    && booking.LawyerId == lawyerUserId))
            .SumAsync(transaction => (decimal?)transaction.Amount, cancellationToken)
            ?? 0m;

        var availableBalance = wallet?.AvailableBalance ?? 0m;
        var pendingBalance = wallet?.PendingBalance ?? 0m;
        var withdrawableAmount = availableBalance;
        var pendingSettlementAmount = 0m;
        DateTimeOffset? expectedAvailableAt = null;

        var payoutAccountProvider =
            paymentProvider as ILawyerPayoutAccountProvider;
        if (payoutAccountProvider is not null)
        {
            var payoutAccount = await dbContext.LawyerPayoutAccounts
                .AsNoTracking()
                .SingleOrDefaultAsync(
                    item => item.LawyerUserId == lawyerUserId
                        && item.ProviderCode == payoutAccountProvider.Settings.ProviderCode,
                    cancellationToken);

            if (payoutAccount is null || payoutAccount.Status != LawyerPayoutAccountStatus.Enabled)
            {
                withdrawableAmount = 0m;
                pendingSettlementAmount = availableBalance;
            }
            else if (availableBalance > 0 && payoutAccount.AvailableProviderAmountMinor > 0)
            {
                try
                {
                    var providerBalance = await payoutAccountProvider.GetBalanceAsync(
                        payoutAccount.ProviderAccountId,
                        payoutAccount.DefaultCurrency,
                        0L,
                        cancellationToken);

                    if (providerBalance.AvailableAmountMinor >= payoutAccount.AvailableProviderAmountMinor)
                    {
                        withdrawableAmount = availableBalance;
                        pendingSettlementAmount = 0m;
                    }
                    else
                    {
                        var allocated = decimal.Floor(
                            availableBalance
                            * providerBalance.AvailableAmountMinor
                            / payoutAccount.AvailableProviderAmountMinor
                            * 100m) / 100m;
                        withdrawableAmount = Math.Clamp(allocated, 0m, availableBalance);
                        pendingSettlementAmount = Math.Max(0m, availableBalance - withdrawableAmount);
                        expectedAvailableAt = providerBalance.ExpectedAvailableAt;
                    }
                }
                catch (OperationCanceledException)
                    when (cancellationToken.IsCancellationRequested)
                {
                    throw;
                }
                catch (Exception exception)
                {
                    logger.LogWarning(
                        exception,
                        "Could not retrieve real-time balance from payout provider for lawyer {LawyerUserId}.",
                        lawyerUserId);
                }
            }
        }

        return new WalletDto(
            lawyerUserId,
            "EGP",
            pendingBalance,
            availableBalance,
            totalReleased,
            withdrawableAmount,
            pendingSettlementAmount,
            expectedAvailableAt);
    }

    public async Task<IReadOnlyList<WithdrawalDto>> GetWithdrawalsAsync(
        CancellationToken cancellationToken)
    {
        var lawyerUserId = GetActorUserId();
        return await dbContext.WithdrawalRequests
            .AsNoTracking()
            .Where(item => item.LawyerUserId == lawyerUserId)
            .OrderByDescending(item => item.RequestedAt)
            .Select(item => new WithdrawalDto(
                item.Id,
                item.Amount,
                item.Currency,
                item.Status,
                item.ProviderStatus,
                item.FailureReason,
                item.RequiresManualAction,
                item.RequestedAt,
                item.ProcessedAt))
            .ToListAsync(cancellationToken);
    }

    public async Task<PaymentActionResultDto> WithdrawAsync(
        CreateWithdrawalRequest request,
        string? idempotencyKey,
        CancellationToken cancellationToken)
    {
        var lawyerUserId = GetActorUserId();
        var normalizedKey = RequireIdempotencyKey(idempotencyKey);
        var wallet = await dbContext.LawyerWallets
            .AsNoTracking()
            .SingleOrDefaultAsync(
                item => item.LawyerUserId == lawyerUserId,
                cancellationToken)
            ?? throw new BusinessException(
                "لا توجد محفظة مالية متاحة لهذا المحامي.");
        LawyerPayoutAccount? payoutAccount = null;
        var payoutAccountProvider =
            paymentProvider as ILawyerPayoutAccountProvider;
        if (payoutAccountProvider is not null)
        {
            payoutAccount = await dbContext.LawyerPayoutAccounts
                .AsNoTracking()
                .SingleOrDefaultAsync(
                    item => item.LawyerUserId == lawyerUserId
                        && item.Status == LawyerPayoutAccountStatus.Enabled,
                    cancellationToken)
                ?? throw new BusinessException(
                    "يجب إكمال وتفعيل حساب السحب لدى مزود الدفع قبل طلب السحب.");
        }
        var scope = new IdempotencyScope(
            lawyerUserId,
            WithdrawalOperation,
            WalletResource,
            wallet.Id);
        IdempotencyReservation reservation;
        try
        {
            reservation = await idempotencyService.ReserveAsync(
                scope,
                normalizedKey,
                request,
                cancellationToken);
        }
        catch (BusinessException exception)
        {
            throw new BusinessException(
                "تعذر قبول مفتاح طلب السحب لأنه مستخدم لطلب مختلف أو ما زال قيد المعالجة.",
                exception);
        }

        if (reservation.IsReplay)
        {
            return await ReplayAsync(
                reservation,
                cancellationToken);
        }

        if (payoutAccountProvider is not null && payoutAccount is not null)
        {
            try
            {
                await EnsureProviderBalanceIsAvailableAsync(
                    payoutAccountProvider,
                    payoutAccount,
                    wallet,
                    request.Amount,
                    cancellationToken);
            }
            catch (Exception exception)
                when (exception is BusinessException or ConflictException)
            {
                await idempotencyService.FailAsync(
                    reservation.RecordId,
                    exception is ConflictException ? 409 : 502,
                    new WithdrawalFailureResponse(exception.Message),
                    null,
                    cancellationToken);
                throw;
            }
        }

        var withdrawal = await ReserveBalanceAsync(
            wallet.Id,
            payoutAccount?.Id,
            lawyerUserId,
            request.Amount,
            reservation.RecordId,
            cancellationToken);
        var providerRequest = new ProviderWithdrawalRequest(
            withdrawal.Amount,
            withdrawal.Currency,
            withdrawal.Id,
            withdrawal.IdempotencyKey,
            withdrawal.Id,
            request.DestinationReference.Trim(),
            withdrawal.ProviderAccountId ?? string.Empty,
            withdrawal.ProviderAmountMinor.HasValue
                && !string.IsNullOrWhiteSpace(withdrawal.ProviderCurrency)
                    ? new ProviderMoney(
                        withdrawal.ProviderAmountMinor.Value,
                        withdrawal.ProviderCurrency)
                    : null);

        ProviderResult providerResult;
        try
        {
            providerResult = await paymentProvider.WithdrawAsync(
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
            await KeepProcessingAsync(
                withdrawal.Id,
                "تعذر التأكد من نتيجة طلب السحب لدى مزود الدفع.",
                cancellationToken);
            throw new BusinessException(
                "نتيجة طلب السحب غير مؤكدة. ظل المبلغ محجوزًا لحين المراجعة ولا يجب إنشاء طلب بديل.",
                exception);
        }

        if (!ProviderResultMatches(providerResult, providerRequest))
        {
            await KeepProcessingAsync(
                withdrawal.Id,
                "بيانات نتيجة مزود الدفع لا تطابق طلب السحب.",
                cancellationToken);
            throw new BusinessException(
                "تعذر التحقق من نتيجة طلب السحب. ظل المبلغ محجوزًا لحين المراجعة.");
        }

        await ApplyProviderResultAsync(
            withdrawal.Id,
            providerResult,
            cancellationToken);

        if (providerResult.Outcome == ProviderOperationOutcome.Failed)
        {
            const string message =
                "رفض مزود الدفع طلب السحب وتمت إعادة المبلغ إلى الرصيد المتاح.";
            await FailAndReleaseReservationAsync(
                wallet.Id,
                withdrawal.Id,
                providerResult.FailureReason ?? message,
                cancellationToken);
            await idempotencyService.FailAsync(
                reservation.RecordId,
                409,
                new WithdrawalFailureResponse(message),
                withdrawal.Id,
                cancellationToken);
            throw new BusinessException(message);
        }

        if (providerResult.Outcome
            is ProviderOperationOutcome.Processing
                or ProviderOperationOutcome.RequiresCustomerAction)
        {
            var processingResponse = new PaymentActionResultDto(
                withdrawal.Id,
                WithdrawalStatus.Processing.ToString(),
                UtcNow);
            return processingResponse;
        }

        if (providerResult.Outcome != ProviderOperationOutcome.Succeeded)
        {
            await KeepProcessingAsync(
                withdrawal.Id,
                providerResult.FailureReason
                ?? "نتيجة طلب السحب غير مؤكدة وتحتاج إلى مراجعة.",
                cancellationToken);
            throw new BusinessException(
                "نتيجة طلب السحب غير مؤكدة. ظل المبلغ محجوزًا لحين المراجعة ولا يجب إنشاء طلب بديل.");
        }

        if (string.IsNullOrWhiteSpace(
                providerResult.ProviderTransactionId)
            || providerResult.ProviderTransactionId.Length > 200)
        {
            await KeepProcessingAsync(
                withdrawal.Id,
                "لم يرسل مزود الدفع معرّفًا صالحًا لعملية السحب.",
                cancellationToken);
            throw new BusinessException(
                "تعذر توثيق عملية السحب الناجحة. ظل المبلغ محجوزًا لحين المراجعة.");
        }

        var response = await CompleteAsync(
            withdrawal.Id,
            providerResult.ProviderTransactionId,
            cancellationToken);
        await idempotencyService.CompleteAsync(
            reservation.RecordId,
            200,
            response,
            withdrawal.Id,
            cancellationToken);
        logger.LogInformation(
            "Withdrawal {WithdrawalId} completed for lawyer {LawyerUserId}.",
            withdrawal.Id,
            lawyerUserId);
        return response;
    }

    public async Task<JobExecutionResult> ReconcilePendingWithdrawalsAsync(
        CancellationToken cancellationToken)
    {
        var withdrawalIds = await dbContext.WithdrawalRequests
            .AsNoTracking()
            .Where(item => item.Status == WithdrawalStatus.Processing
                && !item.RequiresManualAction)
            .OrderBy(item => item.RequestedAt)
            .Select(item => item.Id)
            .Take(100)
            .ToListAsync(cancellationToken);
        var handled = 0;
        foreach (var withdrawalId in withdrawalIds)
        {
            var withdrawal = await dbContext.WithdrawalRequests
                .AsNoTracking()
                .SingleOrDefaultAsync(
                    item =>
                        item.Id == withdrawalId
                        && item.Status
                            == WithdrawalStatus.Processing
                        && !item.RequiresManualAction,
                    cancellationToken);
            if (withdrawal is null)
            {
                continue;
            }

            var statusRequest = new ProviderWithdrawalStatusRequest(
                withdrawal.Amount,
                withdrawal.Currency,
                withdrawal.Id,
                withdrawal.IdempotencyKey,
                withdrawal.Id,
                withdrawal.ProviderTransactionId,
                withdrawal.ProviderAccountId);
            ProviderResult? providerResult;
            try
            {
                providerResult = await reconciliationProvider
                    .GetWithdrawalStatusAsync(
                    statusRequest,
                    cancellationToken);
            }
            catch (OperationCanceledException)
                when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception exception)
            {
                if (HasExceededProcessingSla(withdrawal))
                {
                    await RequireManualActionAsync(
                        withdrawal.Id,
                        "تعذر حسم نتيجة طلب السحب بعد تجاوز مهلة المطابقة.",
                        exception,
                        cancellationToken);
                    handled++;
                    continue;
                }

                logger.LogWarning(
                    exception,
                    "Withdrawal reconciliation failed for {WithdrawalId}.",
                    withdrawal.Id);
                continue;
            }

            if (providerResult is null
                || providerResult.Outcome == ProviderOperationOutcome.Unknown)
            {
                if (HasExceededProcessingSla(withdrawal))
                {
                    await RequireManualActionAsync(
                        withdrawal.Id,
                        "ظلت نتيجة طلب السحب غير مؤكدة بعد تجاوز مهلة المطابقة.",
                        exception: null,
                        cancellationToken);
                    handled++;
                }

                continue;
            }

            if (!ProviderResultMatches(
                    providerResult,
                    statusRequest))
            {
                await KeepProcessingAsync(
                    withdrawal.Id,
                    "بيانات نتيجة مزود الدفع لا تطابق طلب السحب أثناء المراجعة.",
                    cancellationToken);
                if (HasExceededProcessingSla(withdrawal))
                {
                    await RequireManualActionAsync(
                        withdrawal.Id,
                        "تعذر اعتماد نتيجة مطابقة طلب السحب بعد تجاوز المهلة.",
                        exception: null,
                        cancellationToken);
                    handled++;
                }

                continue;
            }

            await ApplyProviderResultAsync(
                withdrawal.Id,
                providerResult,
                cancellationToken);

            var reservation = await dbContext.IdempotencyRecords
                .AsNoTracking()
                .SingleOrDefaultAsync(
                    item =>
                        item.ResultReferenceId == withdrawal.Id
                        && item.Status == IdempotencyStatus.Processing,
                    cancellationToken);
            if (providerResult.Outcome
                == ProviderOperationOutcome.Succeeded
                && !string.IsNullOrWhiteSpace(
                    providerResult.ProviderTransactionId)
                && providerResult.ProviderTransactionId.Length <= 200)
            {
                var response = await CompleteAsync(
                    withdrawal.Id,
                    providerResult.ProviderTransactionId,
                    cancellationToken);
                if (reservation is not null)
                {
                    await idempotencyService.CompleteAsync(
                        reservation.Id,
                        200,
                        response,
                        withdrawal.Id,
                        cancellationToken);
                }

                handled++;
                continue;
            }

            if (providerResult.Outcome
                == ProviderOperationOutcome.Failed)
            {
                const string message =
                    "رفض مزود الدفع طلب السحب أثناء المراجعة وتمت إعادة المبلغ إلى الرصيد المتاح.";
                var wallet = await dbContext.LawyerWallets
                    .AsNoTracking()
                    .SingleAsync(
                        item =>
                            item.LawyerUserId
                            == withdrawal.LawyerUserId,
                        cancellationToken);
                await FailAndReleaseReservationAsync(
                    wallet.Id,
                    withdrawal.Id,
                    providerResult.FailureReason ?? message,
                    cancellationToken);
                if (reservation is not null)
                {
                    await idempotencyService.FailAsync(
                        reservation.Id,
                        409,
                        new WithdrawalFailureResponse(message),
                        withdrawal.Id,
                        cancellationToken);
                }

                handled++;
            }
        }

        return handled == 0
            ? JobExecutionResult.NoOp(
                "NoPendingWithdrawalsWereReconciled")
            : JobExecutionResult.Completed(
                "PendingWithdrawalsReconciled",
                handled);
    }

    private async Task<WithdrawalRequest> ReserveBalanceAsync(
        Guid walletId,
        Guid? payoutAccountId,
        Guid lawyerUserId,
        decimal amount,
        Guid reservationId,
        CancellationToken cancellationToken)
    {
        await using var transaction = dbContext.Database.IsRelational()
            ? await dbContext.Database.BeginTransactionAsync(
                IsolationLevel.Serializable,
                cancellationToken)
            : null;
        var now = UtcNow;
        var wallet = await dbContext.LawyerWallets.SingleAsync(
            item => item.Id == walletId
                && item.LawyerUserId == lawyerUserId,
            cancellationToken);
        var payoutAccount = payoutAccountId.HasValue
            ? await dbContext.LawyerPayoutAccounts.SingleAsync(
                item => item.Id == payoutAccountId.Value
                    && item.LawyerUserId == lawyerUserId,
                cancellationToken)
            : null;
        if (wallet.AvailableBalance < amount
            || (payoutAccountId.HasValue
                && payoutAccount?.Status
                    != LawyerPayoutAccountStatus.Enabled))
        {
            if (transaction is not null)
            {
                await transaction.CommitAsync(cancellationToken);
            }

            const string message =
                "الرصيد المتاح في المحفظة لا يكفي لتنفيذ طلب السحب.";
            await idempotencyService.FailAsync(
                reservationId,
                409,
                new WithdrawalFailureResponse(message),
                null,
                cancellationToken);
            throw new BusinessException(message);
        }

        var providerAmountMinor = payoutAccount is null
            ? (long?)null
            : AllocateProviderMinorAmount(
                payoutAccount.AvailableProviderAmountMinor,
                amount,
                wallet.AvailableBalance);
        if (payoutAccount is not null
            && (providerAmountMinor <= 0
                || payoutAccount.AvailableProviderAmountMinor
                    < providerAmountMinor))
        {
            const string message =
                "رصيد مزود الدفع المتاح لا يكفي لتنفيذ طلب السحب.";
            if (transaction is not null)
            {
                await transaction.CommitAsync(cancellationToken);
            }

            await idempotencyService.FailAsync(
                reservationId,
                409,
                new WithdrawalFailureResponse(message),
                null,
                cancellationToken);
            throw new BusinessException(message);
        }

        wallet.AvailableBalance -= amount;
        wallet.UpdatedAt = now;
        if (payoutAccount is not null && providerAmountMinor.HasValue)
        {
            payoutAccount.AvailableProviderAmountMinor -=
                providerAmountMinor.Value;
            payoutAccount.UpdatedAt = now;
        }

        var withdrawalId = Guid.NewGuid();
        var withdrawal = new WithdrawalRequest(
            withdrawalId,
            lawyerUserId,
            amount,
            $"withdrawal-{withdrawalId:N}",
            now)
        {
            LawyerPayoutAccountId = payoutAccount?.Id,
            ProviderAccountId = payoutAccount?.ProviderAccountId,
            ProviderAmountMinor = providerAmountMinor,
            ProviderCurrency = payoutAccount?.DefaultCurrency
        };
        dbContext.WithdrawalRequests.Add(withdrawal);
        var idempotencyRecord =
            await dbContext.IdempotencyRecords.SingleAsync(
                item => item.Id == reservationId,
                cancellationToken);
        idempotencyRecord.ResultReferenceId = withdrawal.Id;
        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
            if (transaction is not null)
            {
                await transaction.CommitAsync(cancellationToken);
            }
        }
        catch (DbUpdateConcurrencyException exception)
        {
            throw new BusinessException(
                "تغير رصيد المحفظة أثناء إنشاء طلب السحب. يرجى إعادة تحميل المحفظة والمحاولة مرة أخرى.",
                exception);
        }
        catch (Exception exception) when (IsSqlDeadlock(exception))
        {
            throw new BusinessException(
                "تغير رصيد المحفظة أثناء إنشاء طلب السحب. يرجى إعادة تحميل المحفظة والمحاولة مرة أخرى.",
                exception);
        }

        return withdrawal;
    }

    private async Task<PaymentActionResultDto> CompleteAsync(
        Guid withdrawalId,
        string providerTransactionId,
        CancellationToken cancellationToken)
    {
        var withdrawal = await dbContext.WithdrawalRequests.SingleAsync(
            item => item.Id == withdrawalId,
            cancellationToken);
        if (withdrawal.Status == WithdrawalStatus.Completed)
        {
            return MapAction(withdrawal);
        }

        if (withdrawal.Status != WithdrawalStatus.Processing)
        {
            throw new BusinessException(
                "لا يمكن إكمال طلب سحب لم يعد قيد المعالجة.");
        }

        var now = UtcNow;
        withdrawal.Status = WithdrawalStatus.Completed;
        withdrawal.ProviderTransactionId = providerTransactionId;
        withdrawal.FailureReason = null;
        withdrawal.RequiresManualAction = false;
        withdrawal.ManualActionRequiredAt = null;
        withdrawal.ProcessedAt = now;
        await EnqueueWithdrawalEventAsync(
            ContractPaymentEventTypes.WithdrawalCompleted,
            withdrawal,
            cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
        return MapAction(withdrawal);
    }

    private async Task FailAndReleaseReservationAsync(
        Guid walletId,
        Guid withdrawalId,
        string failureReason,
        CancellationToken cancellationToken)
    {
        await using var transaction = dbContext.Database.IsRelational()
            ? await dbContext.Database.BeginTransactionAsync(
                IsolationLevel.Serializable,
                cancellationToken)
            : null;
        var wallet = await dbContext.LawyerWallets.SingleAsync(
            item => item.Id == walletId,
            cancellationToken);
        var withdrawal = await dbContext.WithdrawalRequests.SingleAsync(
            item => item.Id == withdrawalId,
            cancellationToken);
        if (withdrawal.Status == WithdrawalStatus.Processing)
        {
            var now = UtcNow;
            wallet.AvailableBalance += withdrawal.Amount;
            wallet.UpdatedAt = now;
            if (withdrawal.LawyerPayoutAccountId.HasValue
                && withdrawal.ProviderAmountMinor.HasValue)
            {
                var payoutAccount = await dbContext.LawyerPayoutAccounts
                    .SingleAsync(
                        item => item.Id
                            == withdrawal.LawyerPayoutAccountId.Value,
                        cancellationToken);
                payoutAccount.AvailableProviderAmountMinor +=
                    withdrawal.ProviderAmountMinor.Value;
                payoutAccount.UpdatedAt = now;
            }
            withdrawal.Status = WithdrawalStatus.Failed;
            withdrawal.FailureReason = failureReason;
            withdrawal.RequiresManualAction = false;
            withdrawal.ManualActionRequiredAt = null;
            withdrawal.ProcessedAt = now;
            await EnqueueWithdrawalEventAsync(
                ContractPaymentEventTypes.WithdrawalFailed,
                withdrawal,
                cancellationToken);
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        if (transaction is not null)
        {
            await transaction.CommitAsync(cancellationToken);
        }
    }

    private async Task KeepProcessingAsync(
        Guid withdrawalId,
        string reason,
        CancellationToken cancellationToken)
    {
        var withdrawal = await dbContext.WithdrawalRequests.SingleAsync(
            item => item.Id == withdrawalId,
            cancellationToken);
        if (withdrawal.Status == WithdrawalStatus.Processing)
        {
            withdrawal.FailureReason = reason;
            await dbContext.SaveChangesAsync(cancellationToken);
        }
    }

    private static bool IsSqlDeadlock(Exception exception)
    {
        for (var current = exception; current is not null; current = current.InnerException!)
        {
            if (current is SqlException { Number: 1205 })
            {
                return true;
            }
        }

        return false;
    }

    private async Task ApplyProviderResultAsync(
        Guid withdrawalId,
        ProviderResult result,
        CancellationToken cancellationToken)
    {
        var withdrawal = await dbContext.WithdrawalRequests.SingleAsync(
            item => item.Id == withdrawalId,
            cancellationToken);
        withdrawal.ProviderTransactionId = result.ProviderTransactionId;
        withdrawal.ProviderStatus = result.ProviderStatus;
        withdrawal.ProviderAmountMinor =
            result.ProviderMoney?.AmountMinor
            ?? withdrawal.ProviderAmountMinor;
        withdrawal.ProviderCurrency =
            result.ProviderMoney?.Currency
            ?? withdrawal.ProviderCurrency;
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task RequireManualActionAsync(
        Guid withdrawalId,
        string reason,
        Exception? exception,
        CancellationToken cancellationToken)
    {
        var withdrawal = await dbContext.WithdrawalRequests.SingleAsync(
            item => item.Id == withdrawalId,
            cancellationToken);
        if (withdrawal.Status != WithdrawalStatus.Processing
            || withdrawal.RequiresManualAction)
        {
            return;
        }

        var now = UtcNow;
        withdrawal.RequiresManualAction = true;
        withdrawal.ManualActionRequiredAt = now;
        withdrawal.FailureReason = reason;
        await EnqueueWithdrawalEventAsync(
            ContractPaymentEventTypes.WithdrawalDelayed,
            withdrawal,
            cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
        logger.LogCritical(
            exception,
            "Withdrawal {WithdrawalId} exceeded the processing SLA and requires manual action. Requested: {RequestedAt}; escalated: {EscalatedAt}.",
            withdrawal.Id,
            withdrawal.RequestedAt,
            now);
    }

    private bool HasExceededProcessingSla(
        WithdrawalRequest withdrawal)
    {
        var cutoff = UtcNow.AddMinutes(
            -paymentProviderOptions.Value.ProcessingSlaMinutes);
        return withdrawal.RequestedAt <= cutoff;
    }

    private async Task EnqueueWithdrawalEventAsync(
        string eventType,
        WithdrawalRequest withdrawal,
        CancellationToken cancellationToken)
    {
        await outboxWriter.EnqueueAsync(
            new OutboxEvent(
                eventType,
                1,
                new WithdrawalOutcomeEventPayload(
                    withdrawal.Id,
                    withdrawal.LawyerUserId),
                nameof(WithdrawalRequest),
                withdrawal.Id,
                withdrawal.Id),
            cancellationToken);
    }

    private async Task<PaymentActionResultDto> ReplayAsync(
        IdempotencyReservation reservation,
        CancellationToken cancellationToken)
    {
        if (reservation.Status == IdempotencyStatus.Failed)
        {
            var failure = string.IsNullOrWhiteSpace(
                    reservation.ResponseBody)
                ? null
                : JsonSerializer.Deserialize<WithdrawalFailureResponse>(
                    reservation.ResponseBody,
                    SerializerOptions);
            throw new BusinessException(
                failure?.Message
                ?? "فشل طلب السحب السابق المرتبط بمفتاح الطلب.");
        }

        if (!string.IsNullOrWhiteSpace(reservation.ResponseBody))
        {
            var response =
                JsonSerializer.Deserialize<PaymentActionResultDto>(
                    reservation.ResponseBody,
                    SerializerOptions);
            if (response is not null)
            {
                return response;
            }
        }

        if (reservation.ResultReferenceId.HasValue)
        {
            var withdrawal = await dbContext.WithdrawalRequests
                .AsNoTracking()
                .SingleOrDefaultAsync(
                    item =>
                        item.Id
                        == reservation.ResultReferenceId.Value,
                    cancellationToken);
            if (withdrawal?.Status == WithdrawalStatus.Completed)
            {
                return MapAction(withdrawal);
            }
        }

        throw new BusinessException(
            "تعذر استعادة نتيجة طلب السحب السابق المرتبط بمفتاح الطلب.");
    }

    private Guid GetActorUserId()
    {
        if (!currentUserService.IsAuthenticated
            || !currentUserService.UserId.HasValue
            || currentUserService.UserId.Value == Guid.Empty)
        {
            throw new AuthenticationException(
                "يجب تسجيل الدخول للوصول إلى المحفظة المالية.");
        }

        return currentUserService.UserId.Value;
    }

    private static string RequireIdempotencyKey(
        string? idempotencyKey)
    {
        var key = idempotencyKey?.Trim();
        if (string.IsNullOrWhiteSpace(key))
        {
            throw new BusinessException(
                "ترويسة Idempotency-Key مطلوبة لتنفيذ طلب السحب بأمان.");
        }

        if (key.Length > IdempotencyHeader.MaximumLength)
        {
            throw new BusinessException(
                $"ترويسة Idempotency-Key يجب ألا تتجاوز {IdempotencyHeader.MaximumLength} حرف.");
        }

        return key;
    }

    private static bool ProviderResultMatches(
        ProviderResult result,
        ProviderWithdrawalRequest request)
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

    private static bool ProviderResultMatches(
        ProviderResult result,
        ProviderWithdrawalStatusRequest request)
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
                StringComparison.Ordinal);
    }

    private static PaymentActionResultDto MapAction(
        WithdrawalRequest withdrawal)
    {
        return new PaymentActionResultDto(
            withdrawal.Id,
            withdrawal.Status.ToString(),
            withdrawal.ProcessedAt ?? withdrawal.RequestedAt);
    }

    private static long AllocateProviderMinorAmount(
        long availableProviderAmountMinor,
        decimal requestedBusinessAmount,
        decimal availableBusinessAmount)
    {
        if (availableProviderAmountMinor <= 0
            || requestedBusinessAmount <= 0m
            || availableBusinessAmount <= 0m)
        {
            return 0;
        }

        var allocated = decimal.Floor(
            availableProviderAmountMinor
            * requestedBusinessAmount
            / availableBusinessAmount);
        return allocated > long.MaxValue
            ? throw new BusinessException(
                "تجاوز مبلغ السحب لدى مزود الدفع الحد العددي المسموح به.")
            : (long)allocated;
    }

    private async Task EnsureProviderBalanceIsAvailableAsync(
        ILawyerPayoutAccountProvider payoutAccountProvider,
        LawyerPayoutAccount payoutAccount,
        LawyerWallet wallet,
        decimal requestedAmount,
        CancellationToken cancellationToken)
    {
        if (wallet.AvailableBalance < requestedAmount)
        {
            throw new ConflictException(
                "الرصيد المتاح في المحفظة لا يكفي لتنفيذ طلب السحب.");
        }

        var requiredProviderAmount = AllocateProviderMinorAmount(
            payoutAccount.AvailableProviderAmountMinor,
            requestedAmount,
            wallet.AvailableBalance);
        if (requiredProviderAmount <= 0)
        {
            throw new ConflictException(
                "لا يوجد رصيد متاح للسحب لدى مزود الدفع حاليًا.");
        }

        ProviderPayoutBalanceResult providerBalance;
        try
        {
            providerBalance = await payoutAccountProvider.GetBalanceAsync(
                payoutAccount.ProviderAccountId,
                payoutAccount.DefaultCurrency,
                requiredProviderAmount,
                cancellationToken);
        }
        catch (OperationCanceledException)
            when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
        {
            logger.LogWarning(
                exception,
                "Could not retrieve payout balance for account {ProviderAccountId}.",
                payoutAccount.ProviderAccountId);
            throw new BusinessException(
                "تعذر التحقق من الرصيد المتاح لدى مزود الدفع. يرجى المحاولة لاحقًا.",
                exception);
        }

        if (providerBalance.AvailableAmountMinor >= requiredProviderAmount)
        {
            return;
        }

        if (providerBalance.PendingAmountMinor > 0)
        {
            var availability = providerBalance.ExpectedAvailableAt.HasValue
                ? $" الموعد المتوقع لإتاحته هو {providerBalance.ExpectedAvailableAt.Value.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture)}."
                : " يرجى المحاولة لاحقًا.";
            throw new ConflictException(
                "الرصيد موجود لدى مزود الدفع لكنه ما زال معلّقًا وغير متاح للسحب حاليًا."
                + availability);
        }

        throw new ConflictException(
            "الرصيد المتاح لدى مزود الدفع لا يكفي لتنفيذ السحب حاليًا. يرجى المحاولة لاحقًا.");
    }

    private DateTimeOffset UtcNow =>
        timeProvider.GetUtcNow();

    private sealed record WithdrawalFailureResponse(string Message);
}
