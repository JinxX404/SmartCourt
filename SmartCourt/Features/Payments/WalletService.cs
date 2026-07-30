using System.Data;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using SmartCourt.Common.Exceptions;
using SmartCourt.Features.Payments.DTOs;
using SmartCourt.Features.Payments.Entities;
using SmartCourt.Features.Payments.Enums;
using SmartCourt.Infrastructure.Idempotency;
using SmartCourt.Infrastructure.Persistence.Enums;
using SmartCourt.Infrastructure.Providers.Jobs;
using SmartCourt.Infrastructure.Providers.Payments;
using SmartCourt.Interfaces;
using SmartCourt.Persistence;

namespace SmartCourt.Features.Payments;

public sealed class WalletService(
    ApplicationDbContext dbContext,
    ICurrentUserService currentUserService,
    IPaymentProvider paymentProvider,
    IIdempotencyService idempotencyService,
    TimeProvider timeProvider,
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

        return new WalletDto(
            lawyerUserId,
            "EGP",
            wallet?.PendingBalance ?? 0m,
            wallet?.AvailableBalance ?? 0m,
            totalReleased);
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

        var withdrawal = await ReserveBalanceAsync(
            wallet.Id,
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
            request.DestinationReference.Trim());

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
            .Where(item => item.Status == WithdrawalStatus.Processing)
            .OrderBy(item => item.RequestedAt)
            .Select(item => item.Id)
            .Take(100)
            .ToListAsync(cancellationToken);
        var reconciled = 0;
        foreach (var withdrawalId in withdrawalIds)
        {
            var withdrawal = await dbContext.WithdrawalRequests
                .AsNoTracking()
                .SingleOrDefaultAsync(
                    item =>
                        item.Id == withdrawalId
                        && item.Status
                            == WithdrawalStatus.Processing,
                    cancellationToken);
            if (withdrawal is null)
            {
                continue;
            }

            var providerRequest = new ProviderWithdrawalRequest(
                withdrawal.Amount,
                withdrawal.Currency,
                withdrawal.Id,
                withdrawal.IdempotencyKey,
                withdrawal.Id);
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
                logger.LogWarning(
                    exception,
                    "Withdrawal reconciliation failed for {WithdrawalId}.",
                    withdrawal.Id);
                continue;
            }

            if (!ProviderResultMatches(
                    providerResult,
                    providerRequest))
            {
                await KeepProcessingAsync(
                    withdrawal.Id,
                    "بيانات نتيجة مزود الدفع لا تطابق طلب السحب أثناء المراجعة.",
                    cancellationToken);
                continue;
            }

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

                reconciled++;
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

                reconciled++;
            }
        }

        return reconciled == 0
            ? JobExecutionResult.NoOp(
                "NoPendingWithdrawalsWereReconciled")
            : JobExecutionResult.Completed(
                "PendingWithdrawalsReconciled",
                reconciled);
    }

    private async Task<WithdrawalRequest> ReserveBalanceAsync(
        Guid walletId,
        Guid lawyerUserId,
        decimal amount,
        Guid reservationId,
        CancellationToken cancellationToken)
    {
        await using var transaction = dbContext.Database.IsRelational()
            ? await dbContext.Database.BeginTransactionAsync(
                IsolationLevel.ReadCommitted,
                cancellationToken)
            : null;
        var now = UtcNow;
        var balanceReserved = dbContext.Database.IsRelational()
            ? await dbContext.LawyerWallets
                .Where(item =>
                    item.Id == walletId
                    && item.LawyerUserId == lawyerUserId
                    && item.AvailableBalance >= amount)
                .ExecuteUpdateAsync(
                    setters => setters
                        .SetProperty(
                            item => item.AvailableBalance,
                            item => item.AvailableBalance - amount)
                        .SetProperty(
                            item => item.UpdatedAt,
                            now),
                    cancellationToken) == 1
            : await ReserveInMemoryAsync(
                walletId,
                lawyerUserId,
                amount,
                now,
                cancellationToken);
        if (!balanceReserved)
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

        var withdrawalId = Guid.NewGuid();
        var withdrawal = new WithdrawalRequest(
            withdrawalId,
            lawyerUserId,
            amount,
            $"withdrawal-{withdrawalId:N}",
            now);
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

        return withdrawal;
    }

    private async Task<bool> ReserveInMemoryAsync(
        Guid walletId,
        Guid lawyerUserId,
        decimal amount,
        DateTime now,
        CancellationToken cancellationToken)
    {
        var wallet = await dbContext.LawyerWallets.SingleAsync(
            item =>
                item.Id == walletId
                && item.LawyerUserId == lawyerUserId,
            cancellationToken);
        if (wallet.AvailableBalance < amount)
        {
            return false;
        }

        wallet.AvailableBalance -= amount;
        wallet.UpdatedAt = now;
        return true;
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
        withdrawal.ProcessedAt = now;
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
            withdrawal.Status = WithdrawalStatus.Failed;
            withdrawal.FailureReason = failureReason;
            withdrawal.ProcessedAt = now;
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

    private static PaymentActionResultDto MapAction(
        WithdrawalRequest withdrawal)
    {
        return new PaymentActionResultDto(
            withdrawal.Id,
            withdrawal.Status.ToString(),
            withdrawal.ProcessedAt ?? withdrawal.RequestedAt);
    }

    private DateTime UtcNow =>
        timeProvider.GetUtcNow().UtcDateTime;

    private sealed record WithdrawalFailureResponse(string Message);
}
