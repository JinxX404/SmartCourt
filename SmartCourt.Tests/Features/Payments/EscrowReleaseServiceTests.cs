using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using SmartCourt.Features.Contracts.Entities;
using SmartCourt.Features.Contracts.Enums;
using SmartCourt.Features.Milestones.Entities;
using SmartCourt.Features.Milestones.Enums;
using SmartCourt.Features.Payments;
using SmartCourt.Features.Payments.Entities;
using SmartCourt.Features.Payments.Enums;
using SmartCourt.Infrastructure.Persistence.Enums;
using SmartCourt.Infrastructure.Providers.Events;
using SmartCourt.Infrastructure.Providers.Jobs;
using SmartCourt.Infrastructure.Providers.Payments;
using SmartCourt.Persistence;
using Xunit;

namespace SmartCourt.Tests.Features.Payments;

public sealed class EscrowReleaseServiceTests
{
    private readonly DateTime _utcNow =
        new(2026, 9, 2, 10, 0, 0, DateTimeKind.Utc);

    [Fact]
    public async Task ExactFourteenDayBoundary_ReleasesNetAndFeeOnce()
    {
        await using var context = CreateContext();
        var state = await AddAcceptedHoldAsync(
            context,
            holdExpiresAt: _utcNow);
        var provider =
            new TestPaymentProvider(ProviderOperationOutcome.Succeeded);
        var service = CreateService(context, provider);

        var first = await service.ReleaseExpiredHoldAsync(
            state.Hold.Id,
            CancellationToken.None);
        var second = await service.ReleaseExpiredHoldAsync(
            state.Hold.Id,
            CancellationToken.None);

        Assert.Equal(JobExecutionOutcome.Completed, first.Outcome);
        Assert.Equal("EscrowHoldReleased", first.Reason);
        Assert.Equal(JobExecutionOutcome.NoOp, second.Outcome);
        Assert.Equal("EscrowHoldNoLongerFunded", second.Reason);
        Assert.Equal(1, provider.ReleaseCalls);
        Assert.Equal(EscrowHoldStatus.Released, state.Hold.Status);
        Assert.Equal(SettlementType.Release, state.Hold.SettlementType);
        Assert.Equal(_utcNow, state.Hold.SettledAt);
        Assert.NotNull(state.Hold.ProviderReleaseTransactionId);
        Assert.Equal(
            MilestoneStatus.Released,
            state.Milestone.Status);
        Assert.Equal(_utcNow, state.Milestone.ReleasedAt);
        Assert.Equal(950m, state.Account.TotalReleased);
        Assert.Equal(50m, state.Account.TotalFees);
        Assert.Equal(0m, CurrentBalance(state.Account));
        Assert.Equal(0m, state.Wallet.PendingBalance);
        Assert.Equal(950m, state.Wallet.AvailableBalance);

        var releaseTransaction =
            await context.PaymentTransactions.SingleAsync(
                item =>
                    item.OperationType
                        == PaymentOperationType.Release);
        Assert.Equal(
            PaymentTransactionStatus.Completed,
            releaseTransaction.Status);
        Assert.Equal(
            state.Hold.Id,
            releaseTransaction.EscrowHoldId);
        var settlement =
            await context.IdempotencyRecords.SingleAsync();
        Assert.Equal(IdempotencyStatus.Completed, settlement.Status);
        Assert.Equal(
            releaseTransaction.Id,
            settlement.ResultReferenceId);

        var settlementEntries =
            await context.EscrowLedgerEntries
                .Where(entry =>
                    entry.TransactionType
                        == LedgerTransactionType.Release
                    || entry.TransactionType
                        == LedgerTransactionType.PlatformFee)
                .OrderBy(entry => entry.CreatedAt)
                .ThenBy(entry => entry.TransactionType)
                .ToListAsync();
        Assert.Equal(2, settlementEntries.Count);
        var release = Assert.Single(
            settlementEntries,
            entry =>
                entry.TransactionType
                    == LedgerTransactionType.Release);
        var fee = Assert.Single(
            settlementEntries,
            entry =>
                entry.TransactionType
                    == LedgerTransactionType.PlatformFee);
        Assert.Equal(950m, release.Amount);
        Assert.Equal(50m, release.RunningBalance);
        Assert.Equal(50m, fee.Amount);
        Assert.Equal(0m, fee.RunningBalance);
        Assert.Equal(
            state.Hold.GrossAmount,
            release.Amount + fee.Amount);

        var history =
            await context.MilestoneStateHistories.SingleAsync();
        Assert.Equal(
            MilestoneStatus.AcceptedHold,
            history.PreviousStatus);
        Assert.Equal(MilestoneStatus.Released, history.NewStatus);
        var message = await context.OutboxMessages.SingleAsync(
            item =>
                item.EventType
                    == ContractPaymentEventTypes.FundsReleased);
        var payload =
            JsonSerializer.Deserialize<FundsReleasedEventPayload>(
                message.Payload,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
        Assert.NotNull(payload);
        Assert.Equal(state.Milestone.Id, payload.MilestoneId);
        Assert.Equal(state.Hold.Id, payload.EscrowHoldId);
        Assert.Equal(950m, payload.LawyerNetAmount);
        Assert.Equal(50m, payload.PlatformFeeAmount);
    }

    [Fact]
    public async Task BeforeFourteenDayBoundary_DoesNotCreateAttempt()
    {
        await using var context = CreateContext();
        var state = await AddAcceptedHoldAsync(
            context,
            holdExpiresAt: _utcNow.AddTicks(1));
        var provider =
            new TestPaymentProvider(ProviderOperationOutcome.Succeeded);

        var result = await CreateService(
                context,
                provider)
            .ReleaseExpiredHoldAsync(
                state.Hold.Id,
                CancellationToken.None);

        Assert.Equal(JobExecutionOutcome.NoOp, result.Outcome);
        Assert.Equal(
            "HoldReleaseDeadlineNotElapsed",
            result.Reason);
        Assert.Equal(0, provider.ReleaseCalls);
        Assert.Equal(EscrowHoldStatus.Funded, state.Hold.Status);
        Assert.Equal(
            MilestoneStatus.AcceptedHold,
            state.Milestone.Status);
        Assert.Equal(950m, state.Wallet.PendingBalance);
        Assert.Empty(await context.PaymentTransactions.ToListAsync());
        Assert.Empty(await context.IdempotencyRecords.ToListAsync());
        Assert.Empty(await context.EscrowLedgerEntries.ToListAsync());
    }

    [Theory]
    [InlineData(EscrowHoldStatus.Frozen, MilestoneStatus.Disputed)]
    [InlineData(EscrowHoldStatus.Refunded, MilestoneStatus.Refunded)]
    [InlineData(EscrowHoldStatus.Released, MilestoneStatus.Released)]
    public async Task SettledFrozenOrDisputedHold_CannotUseNormalRelease(
        EscrowHoldStatus holdStatus,
        MilestoneStatus milestoneStatus)
    {
        await using var context = CreateContext();
        var state = await AddAcceptedHoldAsync(
            context,
            holdExpiresAt: _utcNow);
        state.Hold.Status = holdStatus;
        state.Milestone.Status = milestoneStatus;
        await context.SaveChangesAsync();
        var provider =
            new TestPaymentProvider(ProviderOperationOutcome.Succeeded);

        var result = await CreateService(
                context,
                provider)
            .ReleaseExpiredHoldAsync(
                state.Hold.Id,
                CancellationToken.None);

        Assert.Equal(JobExecutionOutcome.NoOp, result.Outcome);
        Assert.Equal("EscrowHoldNoLongerFunded", result.Reason);
        Assert.Equal(0, provider.ReleaseCalls);
        Assert.Empty(await context.PaymentTransactions.ToListAsync());
        Assert.Empty(await context.EscrowLedgerEntries.ToListAsync());
    }

    [Theory]
    [InlineData(
        ProviderOperationOutcome.Failed,
        PaymentTransactionStatus.Failed,
        "ReleaseProviderConfirmedFailure")]
    [InlineData(
        ProviderOperationOutcome.Unknown,
        PaymentTransactionStatus.Processing,
        "ReleaseProviderOutcomeUnknown")]
    public async Task ProviderFailure_RetainsRetryableAttemptWithoutSettlement(
        ProviderOperationOutcome outcome,
        PaymentTransactionStatus expectedStatus,
        string expectedReason)
    {
        await using var context = CreateContext();
        var state = await AddAcceptedHoldAsync(
            context,
            holdExpiresAt: _utcNow);
        var provider = new TestPaymentProvider(outcome);

        var result = await CreateService(
                context,
                provider)
            .ReleaseExpiredHoldAsync(
                state.Hold.Id,
                CancellationToken.None);

        Assert.Equal(JobExecutionOutcome.NoOp, result.Outcome);
        Assert.Equal(expectedReason, result.Reason);
        Assert.Equal(EscrowHoldStatus.Funded, state.Hold.Status);
        Assert.Equal(
            MilestoneStatus.AcceptedHold,
            state.Milestone.Status);
        Assert.Equal(950m, state.Wallet.PendingBalance);
        Assert.Equal(0m, state.Wallet.AvailableBalance);
        Assert.Equal(0m, state.Account.TotalReleased);
        Assert.Equal(0m, state.Account.TotalFees);
        Assert.Empty(await context.EscrowLedgerEntries.ToListAsync());
        Assert.Empty(await context.MilestoneStateHistories.ToListAsync());
        Assert.Empty(await context.OutboxMessages.ToListAsync());
        var attempt =
            await context.PaymentTransactions.SingleAsync();
        Assert.Equal(expectedStatus, attempt.Status);
        Assert.Equal(
            PaymentOperationType.Release,
            attempt.OperationType);
        Assert.NotNull(attempt.FailureReason);
        var reservation =
            await context.IdempotencyRecords.SingleAsync();
        Assert.Equal(
            IdempotencyStatus.Processing,
            reservation.Status);
    }

    private EscrowReleaseService CreateService(
        ApplicationDbContext context,
        IPaymentProvider paymentProvider)
    {
        var timeProvider = new FixedTimeProvider(_utcNow);
        return new EscrowReleaseService(
            context,
            paymentProvider,
            new OutboxWriter(context, timeProvider),
            timeProvider,
            NullLogger<EscrowReleaseService>.Instance);
    }

    private async Task<AcceptedHoldState> AddAcceptedHoldAsync(
        ApplicationDbContext context,
        DateTime holdExpiresAt)
    {
        var contractId = Guid.NewGuid();
        var lawyerUserId = Guid.NewGuid();
        var createdAt = _utcNow.AddDays(-20);
        var contract = new Contract(
            contractId,
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            lawyerUserId,
            "عقد تمثيل قانوني",
            "شروط العقد",
            createdAt)
        {
            Status = ContractStatus.Active,
            ActivatedAt = createdAt
        };
        var milestone = new Milestone(
            Guid.NewGuid(),
            contractId,
            "المرحلة الأولى",
            null,
            1,
            1_000m,
            14,
            null,
            createdAt)
        {
            Status = MilestoneStatus.AcceptedHold,
            FundedAt = createdAt,
            SubmittedAt = createdAt.AddDays(1),
            AcceptedAt = holdExpiresAt.AddDays(-14),
            AcceptanceSource = MilestoneAcceptanceSource.Manual,
            HoldStartsAt = holdExpiresAt.AddDays(-14),
            HoldExpiresAt = holdExpiresAt,
            SubmissionVersion = 1,
            RowVersion = [1, 2, 3, 4]
        };
        var account = new EscrowAccount(
            Guid.NewGuid(),
            contractId,
            createdAt)
        {
            TotalDeposited = 1_000m,
            RowVersion = [2, 3, 4, 5]
        };
        var hold = new EscrowHold(
            Guid.NewGuid(),
            account.Id,
            contractId,
            milestone.Id,
            1_000m,
            50m,
            950m,
            Guid.NewGuid(),
            createdAt,
            createdAt)
        {
            HoldStartsAt = holdExpiresAt.AddDays(-14),
            HoldExpiresAt = holdExpiresAt,
            RowVersion = [3, 4, 5, 6]
        };
        var wallet = new LawyerWallet(
            Guid.NewGuid(),
            lawyerUserId,
            createdAt)
        {
            PendingBalance = 950m,
            RowVersion = [4, 5, 6, 7]
        };
        context.Contracts.Add(contract);
        context.Milestones.Add(milestone);
        context.EscrowAccounts.Add(account);
        context.EscrowHolds.Add(hold);
        context.LawyerWallets.Add(wallet);
        await context.SaveChangesAsync();
        return new AcceptedHoldState(
            milestone,
            hold,
            account,
            wallet);
    }

    private ApplicationDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(
                $"escrow-release-{Guid.NewGuid():N}")
            .Options;
        return new ApplicationDbContext(
            options,
            new FixedTimeProvider(_utcNow));
    }

    private static decimal CurrentBalance(EscrowAccount account)
    {
        return account.TotalDeposited
            - account.TotalReleased
            - account.TotalRefunded
            - account.TotalFees;
    }

    private sealed record AcceptedHoldState(
        Milestone Milestone,
        EscrowHold Hold,
        EscrowAccount Account,
        LawyerWallet Wallet);

    private sealed class TestPaymentProvider(
        ProviderOperationOutcome outcome) : IPaymentProvider
    {
        public int ReleaseCalls { get; private set; }

        public Task<ProviderResult> ReleaseAsync(
            ProviderReleaseRequest request,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ReleaseCalls++;
            return Task.FromResult(
                new ProviderResult(
                    request.Amount,
                    request.Currency,
                    request.BusinessId,
                    request.ProviderIdempotencyKey,
                    request.CorrelationId,
                    outcome,
                    outcome == ProviderOperationOutcome.Succeeded
                        ? $"release-{Guid.NewGuid():N}"
                        : null,
                    outcome == ProviderOperationOutcome.Succeeded
                        ? null
                        : "تعذر تنفيذ عملية التحرير."));
        }

        public Task<ProviderResult> DepositAsync(
            ProviderDepositRequest request,
            CancellationToken cancellationToken)
            => throw new NotSupportedException();

        public Task<ProviderResult> RetryDepositAsync(
            ProviderDepositRetryRequest request,
            CancellationToken cancellationToken)
            => throw new NotSupportedException();

        public Task<ProviderResult> RefundAsync(
            ProviderRefundRequest request,
            CancellationToken cancellationToken)
            => throw new NotSupportedException();

        public Task<ProviderResult> WithdrawAsync(
            ProviderWithdrawalRequest request,
            CancellationToken cancellationToken)
            => throw new NotSupportedException();
    }

    private sealed class FixedTimeProvider(DateTime utcNow)
        : TimeProvider
    {
        public override DateTimeOffset GetUtcNow()
            => new(utcNow);
    }
}
