using System.Data;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using SmartCourt.Common.Exceptions;
using SmartCourt.Features.Payments.DTOs;
using SmartCourt.Features.Payments.Entities;
using SmartCourt.Features.Payments.Enums;
using SmartCourt.Features.Users.Integration;
using SmartCourt.Infrastructure.Idempotency;
using SmartCourt.Infrastructure.Persistence.Enums;
using SmartCourt.Infrastructure.Providers.Events;
using SmartCourt.Interfaces;
using SmartCourt.Persistence;

namespace SmartCourt.Features.Payments;

public sealed class AdminWalletAdjustmentService(
    ApplicationDbContext dbContext,
    ICurrentUserService currentUserService,
    IContractUserEligibilityService userEligibilityService,
    IIdempotencyService idempotencyService,
    IOutboxWriter outboxWriter,
    TimeProvider timeProvider) : IAdminWalletAdjustmentService
{
    private const string Operation = "AdminWalletAdjustment";
    private const string ResourceType = "LawyerWallet";
    private static readonly JsonSerializerOptions SerializerOptions =
        new(JsonSerializerDefaults.Web);

    public async Task<AdminWalletAdjustmentDto> AdjustAsync(
        Guid lawyerUserId,
        AdminWalletAdjustmentRequest request,
        string? idempotencyKey,
        CancellationToken cancellationToken)
    {
        if (lawyerUserId == Guid.Empty)
        {
            throw new BusinessException("معرّف المحامي المطلوب تصحيح محفظته غير صالح.");
        }

        ValidateRequest(request);

        var actorUserId = GetActorUserId();
        await EnsureSuperAdministratorAsync(
            actorUserId,
            cancellationToken);
        var key = RequireIdempotencyKey(idempotencyKey);
        var facts = await FindFactsAsync(
            lawyerUserId,
            request.ContractId,
            cancellationToken);
        EnsureValidProspectiveBalances(
            facts.PendingBalance,
            facts.AvailableBalance,
            request.PendingBalanceDelta,
            request.AvailableBalanceDelta);

        IdempotencyReservation reservation;
        try
        {
            reservation = await idempotencyService.ReserveAsync(
                new IdempotencyScope(
                    actorUserId,
                    Operation,
                    ResourceType,
                    facts.WalletId),
                key,
                request,
                cancellationToken);
        }
        catch (BusinessException exception)
        {
            throw new BusinessException(
                "تعذر قبول مفتاح التصحيح المالي لأنه مستخدم لطلب مختلف أو ما زال قيد المعالجة.",
                exception);
        }

        if (reservation.IsReplay)
        {
            return Replay(reservation);
        }

        try
        {
            return await ApplyAsync(
                facts,
                lawyerUserId,
                actorUserId,
                request,
                reservation.RecordId,
                cancellationToken);
        }
        catch (OperationCanceledException)
            when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
        {
            dbContext.ChangeTracker.Clear();
            await idempotencyService.FailAsync(
                reservation.RecordId,
                409,
                new AdjustmentFailureResponse(
                    "تعذر تطبيق التصحيح المالي ولم تتغير أرصدة المحفظة."),
                null,
                cancellationToken);
            if (exception is BusinessException)
            {
                throw;
            }

            throw new ConflictException(
                "تعذر تطبيق التصحيح المالي بسبب تعديل متزامن على المحفظة.");
        }
    }

    private async Task<AdminWalletAdjustmentDto> ApplyAsync(
        AdjustmentFacts facts,
        Guid lawyerUserId,
        Guid actorUserId,
        AdminWalletAdjustmentRequest request,
        Guid reservationId,
        CancellationToken cancellationToken)
    {
        await using var transaction = dbContext.Database.IsRelational()
            ? await dbContext.Database.BeginTransactionAsync(
                IsolationLevel.Serializable,
                cancellationToken)
            : null;
        var wallet = await dbContext.LawyerWallets.SingleAsync(
            item => item.Id == facts.WalletId,
            cancellationToken);
        var account = await dbContext.EscrowAccounts.SingleAsync(
            item => item.Id == facts.EscrowAccountId,
            cancellationToken);
        var contractMatches = await dbContext.Contracts.AnyAsync(
            contract =>
                contract.Id == request.ContractId
                && contract.LawyerUserId == lawyerUserId,
            cancellationToken);
        if (!contractMatches
            || account.ContractId != request.ContractId
            || wallet.LawyerUserId != lawyerUserId
            || !string.Equals(wallet.Currency, "EGP", StringComparison.Ordinal)
            || !string.Equals(account.Currency, "EGP", StringComparison.Ordinal))
        {
            throw new BusinessException(
                "بيانات المحفظة أو العقد أو حساب الضمان لا تتطابق مع طلب التصحيح المالي.");
        }

        EnsureValidProspectiveBalances(
            wallet.PendingBalance,
            wallet.AvailableBalance,
            request.PendingBalanceDelta,
            request.AvailableBalanceDelta);
        var pendingBefore = wallet.PendingBalance;
        var availableBefore = wallet.AvailableBalance;
        var pendingAfter = pendingBefore + request.PendingBalanceDelta;
        var availableAfter = availableBefore + request.AvailableBalanceDelta;
        var escrowBalance = CurrentBalance(account);
        if (escrowBalance < 0m)
        {
            throw new BusinessException(
                "رصيد حساب الضمان المرتبط بالعقد غير متطابق ولا يسمح بإجراء تصحيح إداري.");
        }

        var now = timeProvider.GetUtcNow().UtcDateTime;
        var correlationId = Guid.NewGuid();
        var adjustmentId = Guid.NewGuid();
        var ledgerEntryId = Guid.NewGuid();
        var reason = request.Reason.Trim();
        var ledgerAmount = Math.Abs(request.PendingBalanceDelta)
            + Math.Abs(request.AvailableBalanceDelta);
        var ledgerEntry = new EscrowLedgerEntry(
            ledgerEntryId,
            account.Id,
            escrowHoldId: null,
            LedgerTransactionType.Adjustment,
            ledgerAmount,
            escrowBalance,
            "WalletAdjustment",
            adjustmentId,
            paymentTransactionId: null,
            $"تصحيح إداري لتعويض إسقاط المحفظة: {reason}",
            actorUserId,
            correlationId,
            now);
        var adjustment = new WalletAdjustment(
            adjustmentId,
            wallet.Id,
            request.ContractId,
            account.Id,
            ledgerEntry.Id,
            request.PendingBalanceDelta,
            request.AvailableBalanceDelta,
            pendingBefore,
            pendingAfter,
            availableBefore,
            availableAfter,
            reason,
            actorUserId,
            correlationId,
            now);
        wallet.PendingBalance = pendingAfter;
        wallet.AvailableBalance = availableAfter;
        wallet.UpdatedAt = now;
        dbContext.EscrowLedgerEntries.Add(ledgerEntry);
        dbContext.WalletAdjustments.Add(adjustment);
        await outboxWriter.EnqueueAsync(
            new OutboxEvent(
                ContractPaymentEventTypes.WalletAdjusted,
                1,
                new WalletAdjustedEventPayload(
                    adjustment.Id,
                    lawyerUserId,
                    adjustment.ContractId),
                nameof(WalletAdjustment),
                adjustment.Id,
                correlationId),
            cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        var response = new AdminWalletAdjustmentDto(
            adjustment.Id,
            lawyerUserId,
            adjustment.ContractId,
            adjustment.LedgerEntryId,
            adjustment.PendingBalanceDelta,
            adjustment.AvailableBalanceDelta,
            adjustment.PendingBalanceAfter,
            adjustment.AvailableBalanceAfter,
            adjustment.CreatedByUserId,
            adjustment.CreatedAt);
        await idempotencyService.CompleteAsync(
            reservationId,
            200,
            response,
            adjustment.Id,
            cancellationToken);
        await CommitAsync(transaction, cancellationToken);
        return response;
    }

    private async Task<AdjustmentFacts> FindFactsAsync(
        Guid lawyerUserId,
        Guid contractId,
        CancellationToken cancellationToken)
    {
        return await (
            from contract in dbContext.Contracts.AsNoTracking()
            join account in dbContext.EscrowAccounts.AsNoTracking()
                on contract.Id equals account.ContractId
            join wallet in dbContext.LawyerWallets.AsNoTracking()
                on contract.LawyerUserId equals wallet.LawyerUserId
            where contract.Id == contractId
                && contract.LawyerUserId == lawyerUserId
            select new AdjustmentFacts(
                wallet.Id,
                account.Id,
                wallet.PendingBalance,
                wallet.AvailableBalance))
            .SingleOrDefaultAsync(cancellationToken)
            ?? throw new NotFoundException(
                "لم يتم العثور على محفظة وحساب ضمان مطابقين للمحامي والعقد المحددين.");
    }

    private async Task EnsureSuperAdministratorAsync(
        Guid actorUserId,
        CancellationToken cancellationToken)
    {
        var eligibility = await userEligibilityService.FindEligibilityAsync(
            actorUserId,
            cancellationToken);
        if (eligibility is null
            || !eligibility.IsActive
            || !eligibility.CanActAsSuperAdministrator)
        {
            throw new ForbiddenAccessException(
                "إجراء التصحيحات الاستثنائية على المحافظ متاح للمشرف العام المخول فقط.");
        }
    }

    private static void EnsureValidProspectiveBalances(
        decimal pendingBalance,
        decimal availableBalance,
        decimal pendingDelta,
        decimal availableDelta)
    {
        if (pendingDelta == 0m && availableDelta == 0m)
        {
            throw new BusinessException(
                "يجب إدخال قيمة تصحيح غير صفرية لرصيد واحد على الأقل.");
        }

        if (pendingBalance + pendingDelta < 0m
            || availableBalance + availableDelta < 0m)
        {
            throw new BusinessException(
                "لا يمكن تطبيق التصحيح لأنه سيجعل أحد أرصدة المحفظة سالبًا.");
        }
    }

    private static void ValidateRequest(AdminWalletAdjustmentRequest request)
    {
        if (request.ContractId == Guid.Empty)
        {
            throw new BusinessException(
                "معرّف العقد المرتبط بالتصحيح المالي مطلوب.");
        }

        if (string.IsNullOrWhiteSpace(request.Reason)
            || request.Reason.Trim().Length is < 20 or > 1_500)
        {
            throw new BusinessException(
                "سبب التصحيح المالي مطلوب ويجب أن يتراوح بين 20 و1500 حرف.");
        }

        if (Math.Abs(request.PendingBalanceDelta) > 1_000_000m
            || Math.Abs(request.AvailableBalanceDelta) > 1_000_000m
            || decimal.Round(request.PendingBalanceDelta, 2)
                != request.PendingBalanceDelta
            || decimal.Round(request.AvailableBalanceDelta, 2)
                != request.AvailableBalanceDelta)
        {
            throw new BusinessException(
                "قيم التصحيح المالي غير صالحة أو تتجاوز منزلتين عشريتين أو الحد الإداري المسموح.");
        }
    }

    private static AdminWalletAdjustmentDto Replay(
        IdempotencyReservation reservation)
    {
        if (reservation.Status == IdempotencyStatus.Completed
            && !string.IsNullOrWhiteSpace(reservation.ResponseBody))
        {
            var response = JsonSerializer
                .Deserialize<AdminWalletAdjustmentDto>(
                    reservation.ResponseBody,
                    SerializerOptions);
            if (response is not null)
            {
                return response;
            }
        }

        if (reservation.Status == IdempotencyStatus.Failed)
        {
            throw new BusinessException(
                "فشل طلب التصحيح المالي السابق المرتبط بمفتاح الطلب ولم تتغير الأرصدة.");
        }

        throw new BusinessException(
            "طلب التصحيح المالي المرتبط بمفتاح الطلب ما زال قيد المعالجة.");
    }

    private Guid GetActorUserId()
    {
        if (!currentUserService.IsAuthenticated
            || !currentUserService.UserId.HasValue
            || currentUserService.UserId.Value == Guid.Empty)
        {
            throw new AuthenticationException(
                "يجب تسجيل الدخول لإجراء تصحيح مالي على المحفظة.");
        }

        return currentUserService.UserId.Value;
    }

    private static string RequireIdempotencyKey(string? idempotencyKey)
    {
        var key = idempotencyKey?.Trim();
        if (string.IsNullOrWhiteSpace(key) || key.Length > 200)
        {
            throw new BusinessException(
                "مفتاح الطلب مطلوب ويجب ألا يتجاوز 200 حرف.");
        }

        return key;
    }

    private static decimal CurrentBalance(EscrowAccount account)
        => account.TotalDeposited
            - account.TotalReleased
            - account.TotalRefunded
            - account.TotalFees;

    private static async Task CommitAsync(
        Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction? transaction,
        CancellationToken cancellationToken)
    {
        if (transaction is not null)
        {
            await transaction.CommitAsync(cancellationToken);
        }
    }

    private sealed record AdjustmentFacts(
        Guid WalletId,
        Guid EscrowAccountId,
        decimal PendingBalance,
        decimal AvailableBalance);

    private sealed record AdjustmentFailureResponse(string Message);
}
