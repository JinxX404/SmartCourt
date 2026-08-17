using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using SmartCourt.Common.Exceptions;
using SmartCourt.Features.Consultations.Bookings;
using SmartCourt.Features.Consultations.Domain.Entities;
using SmartCourt.Features.Consultations.Domain.Enums;
using SmartCourt.Features.Consultations.DTOs;
using SmartCourt.Features.Consultations.Events;
using SmartCourt.Features.Consultations.Shared;
using SmartCourt.Features.Payments.Entities;
using SmartCourt.Features.Payments.Enums;
using SmartCourt.Infrastructure.Providers.Payments;
using SmartCourt.Interfaces;
using SmartCourt.Interfaces.Providers;
using SmartCourt.Infrastructure.Providers.Events;
using SmartCourt.Persistence;
using SmartCourt.Providers.Payments;

namespace SmartCourt.Features.Consultations.Payments;

public sealed class ConsultationPaymentService(
    ApplicationDbContext dbContext,
    ICurrentUserService currentUserService,
    IPaymentProvider paymentProvider,
    IPaymentReconciliationProvider reconciliationProvider,
    IBackgroundJobProvider backgroundJobs,
    IOutboxWriter outboxWriter,
    IOptions<PaymentProviderOptions> paymentOptions,
    TimeProvider timeProvider)
    : IConsultationPaymentService
{
    public async Task<ConsultationPaymentDto> FundAsync(
        Guid bookingId,
        string confirmationTokenReference,
        string? idempotencyKey,
        CancellationToken cancellationToken)
    {
        var clientId = ConsultationAccess.RequireUserId(currentUserService);
        var key = RequireIdempotencyKey(idempotencyKey);
        var booking = await dbContext.ConsultationBookings.SingleOrDefaultAsync(
            item => item.Id == bookingId && item.ClientId == clientId,
            cancellationToken) ?? throw new NotFoundException("Consultation booking was not found.");
        var existing = await dbContext.ConsultationPaymentTransactions
            .SingleOrDefaultAsync(item => item.ProviderName == ProviderCode
                && item.IdempotencyKey == key, cancellationToken);
        if (existing is not null)
        {
            if (existing.BookingId != booking.Id || existing.OperationType != PaymentOperationType.Deposit)
                throw new ConflictException("The idempotency key belongs to a different payment operation.");
            return Map(existing, null);
        }

        var now = UtcNow;
        if (booking.Status != ConsultationBookingStatus.AwaitingPayment)
            throw new ConflictException("The consultation booking is not awaiting payment.");
        if (booking.PaymentExpiresAtUtc <= now)
            throw new ConflictException("The consultation payment reservation has expired.");

        var transaction = NewTransaction(booking.Id, PaymentOperationType.Deposit, key, booking.GrossAmount);
        dbContext.ConsultationPaymentTransactions.Add(transaction);
        await dbContext.SaveChangesAsync(cancellationToken);

        var customerReference = await dbContext.ClientPaymentCustomers.AsNoTracking()
            .Where(item => item.ClientUserId == clientId && item.ProviderCode == ProviderCode)
            .Select(item => item.ProviderCustomerId)
            .SingleOrDefaultAsync(cancellationToken) ?? string.Empty;
        var request = new ProviderDepositRequest(
            booking.GrossAmount, booking.Currency, booking.Id, key, transaction.Id,
            PaymentMethodReference: paymentOptions.Value.UseMockProvider
                ? confirmationTokenReference.Trim()
                : string.Empty,
            ConfirmationTokenReference: confirmationTokenReference.Trim(),
            CustomerReference: customerReference);
        ProviderResult result;
        try
        {
            result = await paymentProvider.DepositAsync(request, cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) { throw; }
        catch (Exception exception)
        {
            transaction.FailureReason = "The payment provider outcome is unknown and requires reconciliation.";
            transaction.UpdatedAt = UtcNow;
            await dbContext.SaveChangesAsync(CancellationToken.None);
            throw new BusinessException(transaction.FailureReason, exception);
        }

        EnsureMatchingResult(result, request);
        ApplyProviderResult(transaction, result);
        if (result.Outcome == ProviderOperationOutcome.Succeeded)
            await CompleteDepositAsync(booking, transaction, cancellationToken);
        else
            await dbContext.SaveChangesAsync(cancellationToken);

        if (result.Outcome == ProviderOperationOutcome.Failed)
            throw new ConflictException(result.FailureReason ?? "The payment provider rejected the consultation payment.");
        return Map(transaction, result.ClientAction);
    }

    public async Task RefundAsync(
        Guid bookingId,
        decimal amount,
        string reason,
        CancellationToken cancellationToken)
    {
        var booking = await dbContext.ConsultationBookings.SingleAsync(item => item.Id == bookingId, cancellationToken);
        var hold = await dbContext.ConsultationEscrowHolds.SingleOrDefaultAsync(
            item => item.BookingId == bookingId, cancellationToken)
            ?? throw new ConflictException("The consultation has no funded payment to refund.");
        if (hold.Status is EscrowHoldStatus.Released or EscrowHoldStatus.Refunded)
            throw new ConflictException("The consultation payment is already settled.");
        if (amount <= 0 || amount > hold.GrossAmount)
            throw new BusinessException("The consultation refund amount is invalid.");

        var deposit = await dbContext.ConsultationPaymentTransactions.AsNoTracking()
            .SingleAsync(item => item.Id == hold.DepositTransactionId, cancellationToken);
        var transaction = NewTransaction(
            bookingId, PaymentOperationType.Refund,
            $"consultation-refund:{bookingId:N}:{amount:F2}", amount);
        dbContext.ConsultationPaymentTransactions.Add(transaction);
        await dbContext.SaveChangesAsync(cancellationToken);
        var request = new ProviderRefundRequest(
            amount, booking.Currency, booking.Id, transaction.IdempotencyKey,
            transaction.Id, reason.Trim(), deposit.ProviderTransactionId ?? string.Empty);
        var result = await paymentProvider.RefundAsync(request, cancellationToken);
        EnsureMatchingResult(result, request);
        ApplyProviderResult(transaction, result);
        if (result.Outcome != ProviderOperationOutcome.Succeeded)
        {
            await dbContext.SaveChangesAsync(cancellationToken);
            throw new ConflictException(result.FailureReason ?? "The consultation refund was not completed.");
        }
        await ApplyFullRefundAsync(booking, hold, transaction, amount, cancellationToken);
    }

    public async Task StartCompletionHoldAsync(Guid bookingId, CancellationToken cancellationToken)
    {
        var hold = await dbContext.ConsultationEscrowHolds.SingleOrDefaultAsync(
            item => item.BookingId == bookingId, cancellationToken)
            ?? throw new ConflictException("The consultation payment has not been funded.");
        if (hold.Status != EscrowHoldStatus.Funded)
            throw new ConflictException("The consultation payment cannot enter the release hold.");
        var now = UtcNow;
        hold.HoldStartsAtUtc ??= now;
        hold.HoldExpiresAtUtc ??= now.AddDays(ConsultationPolicy.ReleaseHoldDays);
        hold.UpdatedAt = now;
        await dbContext.SaveChangesAsync(cancellationToken);
        await backgroundJobs.ScheduleAsync<IConsultationJobService>(
            service => service.ReleaseAsync(bookingId, CancellationToken.None),
            hold.HoldExpiresAtUtc.Value, cancellationToken);
    }

    public async Task ReleaseAsync(Guid bookingId, CancellationToken cancellationToken)
    {
        var booking = await dbContext.ConsultationBookings.SingleOrDefaultAsync(
            item => item.Id == bookingId, cancellationToken);
        var hold = await dbContext.ConsultationEscrowHolds.SingleOrDefaultAsync(
            item => item.BookingId == bookingId, cancellationToken);
        if (booking is null || hold is null || hold.Status != EscrowHoldStatus.Funded
            || !hold.HoldExpiresAtUtc.HasValue || hold.HoldExpiresAtUtc > UtcNow)
            return;
        await ReleaseAllocationAsync(booking, hold, hold.GrossAmount, hold.NetAmount, hold.PlatformFeeAmount, cancellationToken);
    }

    public async Task SettleDisputeAsync(
        Guid bookingId,
        decimal clientRefundAmount,
        string reason,
        CancellationToken cancellationToken)
    {
        var booking = await dbContext.ConsultationBookings.SingleAsync(item => item.Id == bookingId, cancellationToken);
        var hold = await dbContext.ConsultationEscrowHolds.SingleAsync(item => item.BookingId == bookingId, cancellationToken);
        if (hold.Status != EscrowHoldStatus.Frozen || clientRefundAmount < 0 || clientRefundAmount > hold.GrossAmount)
            throw new ConflictException("The consultation dispute cannot be settled with this allocation.");

        var lawyerGross = hold.GrossAmount - clientRefundAmount;
        var settlement = ConsultationPolicy.CalculateSettlement(lawyerGross);
        if (clientRefundAmount > 0)
            await RefundForSettlementAsync(booking, hold, clientRefundAmount, reason, cancellationToken);

        if (lawyerGross > 0)
            await ReleaseAllocationAsync(booking, hold, lawyerGross, settlement.Net, settlement.Fee, cancellationToken);
        else
        {
            var wallet = await dbContext.LawyerWallets.SingleAsync(
                item => item.LawyerUserId == booking.LawyerId, cancellationToken);
            if (wallet.PendingBalance < hold.NetAmount)
                throw new ConflictException("The lawyer pending wallet does not match the consultation hold.");
            wallet.PendingBalance -= hold.NetAmount;
            wallet.UpdatedAt = UtcNow;
            hold.Status = EscrowHoldStatus.Refunded;
            hold.SettledAtUtc = UtcNow;
            hold.UpdatedAt = UtcNow;
            await dbContext.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task ReconcileProviderObjectAsync(
        string providerObjectId,
        CancellationToken cancellationToken)
    {
        var transaction = await dbContext.ConsultationPaymentTransactions.SingleOrDefaultAsync(
            item => item.ProviderTransactionId == providerObjectId
                && item.Status == PaymentTransactionStatus.Processing,
            cancellationToken);
        if (transaction is null)
            return;

        var correlation = Guid.NewGuid();
        ProviderResult? result = transaction.OperationType switch
        {
            PaymentOperationType.Deposit => await reconciliationProvider.GetDepositStatusAsync(
                new(transaction.Amount, transaction.Currency, transaction.BookingId,
                    transaction.IdempotencyKey, correlation, transaction.ProviderTransactionId), cancellationToken),
            PaymentOperationType.Release => await reconciliationProvider.GetReleaseStatusAsync(
                new(transaction.Amount, transaction.Currency, transaction.BookingId,
                    transaction.IdempotencyKey, correlation, transaction.ProviderTransactionId), cancellationToken),
            PaymentOperationType.Refund => await reconciliationProvider.GetRefundStatusAsync(
                new(transaction.Amount, transaction.Currency, transaction.BookingId,
                    transaction.IdempotencyKey, correlation, transaction.ProviderTransactionId), cancellationToken),
            _ => null
        };
        if (result is null || result.Outcome is ProviderOperationOutcome.Unknown or ProviderOperationOutcome.Processing)
            return;
        ApplyProviderResult(transaction, result);
        if (transaction.OperationType == PaymentOperationType.Deposit
            && result.Outcome == ProviderOperationOutcome.Succeeded)
        {
            var booking = await dbContext.ConsultationBookings.SingleAsync(
                item => item.Id == transaction.BookingId, cancellationToken);
            await CompleteDepositAsync(booking, transaction, cancellationToken);
        }
        else
            await dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task CompleteDepositAsync(
        ConsultationBooking booking,
        ConsultationPaymentTransaction transaction,
        CancellationToken cancellationToken)
    {
        if (await dbContext.ConsultationEscrowHolds.AnyAsync(item => item.BookingId == booking.Id, cancellationToken))
            return;
        var now = UtcNow;
        transaction.Status = PaymentTransactionStatus.Completed;
        transaction.ProcessedAtUtc = now;
        var hold = new ConsultationEscrowHold
        {
            Id = Guid.NewGuid(), BookingId = booking.Id, DepositTransactionId = transaction.Id,
            GrossAmount = booking.GrossAmount, PlatformFeeAmount = booking.PlatformFeeAmount,
            NetAmount = booking.LawyerNetAmount, Currency = booking.Currency,
            Status = EscrowHoldStatus.Funded, FundedAtUtc = now, CreatedAt = now, UpdatedAt = now
        };
        dbContext.ConsultationEscrowHolds.Add(hold);
        var wallet = await GetOrCreateWalletAsync(booking.LawyerId, cancellationToken);
        wallet.PendingBalance += booking.LawyerNetAmount;
        wallet.UpdatedAt = now;
        booking.Status = ConsultationBookingStatus.Confirmed;
        booking.UpdatedAt = now;
        var slot = await dbContext.ConsultationAvailabilitySlots.SingleAsync(item => item.Id == booking.SlotId, cancellationToken);
        slot.Status = ConsultationSlotStatus.Booked;
        slot.ReservedUntilUtc = null;
        slot.UpdatedAt = now;
        AddLedger(booking.Id, transaction.Id, LedgerTransactionType.Deposit,
            booking.GrossAmount, booking.GrossAmount, "Consultation payment funded.");
        await ConsultationOutbox.EnqueueAsync(
            outboxWriter, ConsultationEventTypes.PaymentFunded,
            booking, booking.ClientId, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task ApplyFullRefundAsync(
        ConsultationBooking booking,
        ConsultationEscrowHold hold,
        ConsultationPaymentTransaction transaction,
        decimal amount,
        CancellationToken cancellationToken)
    {
        if (amount != hold.GrossAmount)
            throw new BusinessException("Partial refunds must be completed through dispute settlement.");
        var wallet = await dbContext.LawyerWallets.SingleAsync(
            item => item.LawyerUserId == booking.LawyerId, cancellationToken);
        if (wallet.PendingBalance < hold.NetAmount)
            throw new ConflictException("The lawyer pending wallet does not match the consultation hold.");
        wallet.PendingBalance -= hold.NetAmount;
        wallet.UpdatedAt = UtcNow;
        hold.Status = EscrowHoldStatus.Refunded;
        hold.SettledAtUtc = UtcNow;
        hold.UpdatedAt = UtcNow;
        AddLedger(booking.Id, transaction.Id, LedgerTransactionType.Refund,
            amount, 0m, "Consultation payment refunded to the client.");
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task RefundForSettlementAsync(
        ConsultationBooking booking,
        ConsultationEscrowHold hold,
        decimal amount,
        string reason,
        CancellationToken cancellationToken)
    {
        var deposit = await dbContext.ConsultationPaymentTransactions.AsNoTracking()
            .SingleAsync(item => item.Id == hold.DepositTransactionId, cancellationToken);
        var transaction = NewTransaction(booking.Id, PaymentOperationType.Refund,
            $"consultation-settlement-refund:{booking.Id:N}:{amount:F2}", amount);
        dbContext.ConsultationPaymentTransactions.Add(transaction);
        await dbContext.SaveChangesAsync(cancellationToken);
        var request = new ProviderRefundRequest(amount, booking.Currency, booking.Id,
            transaction.IdempotencyKey, transaction.Id, reason.Trim(), deposit.ProviderTransactionId ?? string.Empty);
        var result = await paymentProvider.RefundAsync(request, cancellationToken);
        EnsureMatchingResult(result, request);
        ApplyProviderResult(transaction, result);
        if (result.Outcome != ProviderOperationOutcome.Succeeded)
        {
            await dbContext.SaveChangesAsync(cancellationToken);
            throw new ConflictException(result.FailureReason ?? "The dispute refund was not completed.");
        }
        AddLedger(booking.Id, transaction.Id, LedgerTransactionType.Refund,
            amount, hold.GrossAmount - amount, "Consultation dispute refund.");
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task ReleaseAllocationAsync(
        ConsultationBooking booking,
        ConsultationEscrowHold hold,
        decimal lawyerGross,
        decimal lawyerNet,
        decimal platformFee,
        CancellationToken cancellationToken)
    {
        var payoutAccount = paymentProvider is ILawyerPayoutAccountProvider
            ? await dbContext.LawyerPayoutAccounts.SingleOrDefaultAsync(
                item => item.LawyerUserId == booking.LawyerId
                    && item.ProviderCode == ProviderCode
                    && item.Status == LawyerPayoutAccountStatus.Enabled
                    && item.TransfersEnabled, cancellationToken)
            : null;
        if (paymentProvider is ILawyerPayoutAccountProvider && payoutAccount is null)
            throw new ConflictException("حساب استلام أتعاب المحامي غير جاهز لتحرير دفعة الاستشارة.");
        var deposit = await dbContext.ConsultationPaymentTransactions.AsNoTracking()
            .SingleAsync(item => item.Id == hold.DepositTransactionId, cancellationToken);
        var transaction = NewTransaction(booking.Id, PaymentOperationType.Release,
            $"consultation-release:{booking.Id:N}:{lawyerGross:F2}", lawyerNet);
        dbContext.ConsultationPaymentTransactions.Add(transaction);
        await dbContext.SaveChangesAsync(cancellationToken);
        var request = new ProviderReleaseRequest(
            lawyerNet, booking.Currency, booking.Id, transaction.IdempotencyKey,
            transaction.Id, deposit.ProviderTransactionId ?? string.Empty,
            DestinationAccountId: payoutAccount?.ProviderAccountId ?? string.Empty,
            GrossBusinessAmount: lawyerGross);
        var result = await paymentProvider.ReleaseAsync(request, cancellationToken);
        EnsureMatchingResult(result, request);
        ApplyProviderResult(transaction, result);
        if (result.Outcome != ProviderOperationOutcome.Succeeded)
        {
            await dbContext.SaveChangesAsync(cancellationToken);
            throw new ConflictException(result.FailureReason ?? "تعذر تحرير دفعة الاستشارة للمحامي.");
        }

        var wallet = await dbContext.LawyerWallets.SingleAsync(
            item => item.LawyerUserId == booking.LawyerId, cancellationToken);
        if (wallet.PendingBalance < hold.NetAmount)
            throw new ConflictException("الرصيد المعلّق في محفظة المحامي لا يطابق حجز دفعة الاستشارة.");
        wallet.PendingBalance -= hold.NetAmount;
        wallet.AvailableBalance += lawyerNet;
        wallet.UpdatedAt = UtcNow;
        if (payoutAccount is not null && result.ProviderMoney is not null)
        {
            if (payoutAccount.AvailableProviderAmountMinor
                > long.MaxValue - result.ProviderMoney.AmountMinor)
                throw new BusinessException("تجاوز رصيد مزود الدفع الحد العددي المسموح به.");
            payoutAccount.AvailableProviderAmountMinor +=
                result.ProviderMoney.AmountMinor;
            payoutAccount.DefaultCurrency = result.ProviderMoney.Currency;
            payoutAccount.UpdatedAt = UtcNow;
        }
        hold.Status = EscrowHoldStatus.Released;
        hold.SettledAtUtc = UtcNow;
        hold.UpdatedAt = UtcNow;
        AddLedger(booking.Id, transaction.Id, LedgerTransactionType.Release,
            lawyerNet, platformFee, "Consultation lawyer allocation released.");
        if (platformFee > 0)
            AddLedger(booking.Id, transaction.Id, LedgerTransactionType.PlatformFee,
                platformFee, 0m, "Consultation platform fee recognized.");
        await ConsultationOutbox.EnqueueAsync(
            outboxWriter, ConsultationEventTypes.PaymentReleased,
            booking, null, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private ConsultationPaymentTransaction NewTransaction(
        Guid bookingId,
        PaymentOperationType operation,
        string idempotencyKey,
        decimal amount)
    {
        var now = UtcNow;
        return new()
        {
            Id = Guid.NewGuid(), BookingId = bookingId, OperationType = operation,
            Status = PaymentTransactionStatus.Processing, ProviderName = ProviderCode,
            IdempotencyKey = idempotencyKey, Amount = amount, Currency = "EGP",
            CreatedAt = now, UpdatedAt = now
        };
    }

    private async Task<LawyerWallet> GetOrCreateWalletAsync(Guid lawyerId, CancellationToken cancellationToken)
    {
        var wallet = await dbContext.LawyerWallets.SingleOrDefaultAsync(
            item => item.LawyerUserId == lawyerId, cancellationToken);
        if (wallet is not null)
            return wallet;
        wallet = new LawyerWallet(Guid.NewGuid(), lawyerId, UtcNow);
        dbContext.LawyerWallets.Add(wallet);
        return wallet;
    }

    private void ApplyProviderResult(ConsultationPaymentTransaction transaction, ProviderResult result)
    {
        transaction.ProviderTransactionId = result.ProviderTransactionId;
        transaction.RelatedProviderTransactionId = result.RelatedProviderTransactionId;
        transaction.ProviderStatus = result.ProviderStatus;
        transaction.FailureReason = result.FailureReason;
        transaction.UpdatedAt = UtcNow;
        transaction.Status = result.Outcome switch
        {
            ProviderOperationOutcome.Succeeded => PaymentTransactionStatus.Completed,
            ProviderOperationOutcome.Failed => PaymentTransactionStatus.Failed,
            _ => PaymentTransactionStatus.Processing
        };
        if (transaction.Status != PaymentTransactionStatus.Processing)
            transaction.ProcessedAtUtc = UtcNow;
    }

    private static void EnsureMatchingResult(ProviderResult result, IProviderOperationRequest request)
    {
        if (result.Amount != request.Amount || result.Currency != request.Currency
            || result.BusinessId != request.BusinessId
            || result.ProviderIdempotencyKey != request.ProviderIdempotencyKey
            || result.CorrelationId != request.CorrelationId)
            throw new BusinessException("The payment provider returned mismatched consultation payment data.");
    }

    private void AddLedger(
        Guid bookingId,
        Guid transactionId,
        LedgerTransactionType type,
        decimal amount,
        decimal runningBalance,
        string description) =>
        dbContext.ConsultationLedgerEntries.Add(new ConsultationLedgerEntry
        {
            Id = Guid.NewGuid(), BookingId = bookingId, PaymentTransactionId = transactionId,
            TransactionType = type, Amount = amount, RunningBalance = runningBalance,
            Description = description, CorrelationId = transactionId, CreatedAt = UtcNow
        });

    private static string RequireIdempotencyKey(string? value)
    {
        var key = value?.Trim();
        if (string.IsNullOrWhiteSpace(key) || key.Length > 200)
            throw new BusinessException("A non-empty Idempotency-Key header of at most 200 characters is required.");
        return key;
    }

    private static ConsultationPaymentDto Map(
        ConsultationPaymentTransaction item,
        ProviderClientAction? action) => new(
            item.Id, item.BookingId, item.OperationType, item.Status,
            item.Amount, item.Currency, action?.Type.ToString(),
            action?.ClientSecret, action?.RedirectUrl, item.FailureReason, item.CreatedAt);

    private string ProviderCode => paymentOptions.Value.ProviderCode;
    private DateTimeOffset UtcNow => timeProvider.GetUtcNow();
}
