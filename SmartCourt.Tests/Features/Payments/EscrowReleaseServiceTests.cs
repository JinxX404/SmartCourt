using System.Text.Json;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using SmartCourt.Features.Contracts.Entities;
using SmartCourt.Features.Contracts.Enums;
using SmartCourt.Features.Contracts;
using SmartCourt.Features.Contracts.DTOs;
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
    private readonly RecordingCompletionEvaluator _completionEvaluator = new();
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
        Assert.Equal(state.Hold.ContractId, _completionEvaluator.ContractId);

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

    [Fact]
    public async Task ExpenseRelease_BypassesSubmissionAcceptanceAndHoldDeadline()
    {
        await using var context = CreateContext();
        var state = await AddAcceptedHoldAsync(
            context,
            holdExpiresAt: null,
            type: MilestoneType.Expense);
        var provider =
            new TestPaymentProvider(ProviderOperationOutcome.Succeeded);

        var result = await CreateService(context, provider)
            .ReleaseExpiredHoldAsync(
                state.Hold.Id,
                CancellationToken.None);

        Assert.Equal(JobExecutionOutcome.Completed, result.Outcome);
        Assert.Equal(MilestoneStatus.Released, state.Milestone.Status);
        Assert.Null(state.Milestone.SubmittedAt);
        Assert.Null(state.Milestone.AcceptedAt);
        Assert.Null(state.Milestone.HoldStartsAt);
        Assert.Null(state.Milestone.HoldExpiresAt);
        Assert.Null(state.Hold.HoldStartsAt);
        Assert.Null(state.Hold.HoldExpiresAt);
        Assert.Equal(950m, state.Wallet.AvailableBalance);
        var history = await context.MilestoneStateHistories.SingleAsync();
        Assert.Equal(MilestoneStatus.ReleasePending, history.PreviousStatus);
        Assert.Equal(MilestoneStatus.Released, history.NewStatus);
    }

    [Fact]
    public async Task ForceReleaseMilestone_CompletesBeforeExpiry()
    {
        await using var context = CreateContext();
        var state = await AddAcceptedHoldAsync(
            context,
            holdExpiresAt: _utcNow.AddDays(14));
        var provider =
            new TestPaymentProvider(ProviderOperationOutcome.Succeeded);

        var result = await CreateService(
                context,
                provider)
            .ForceReleaseMilestoneAsync(
                state.Milestone.Id,
                CancellationToken.None);

        Assert.Equal(JobExecutionOutcome.Completed, result.Outcome);
        Assert.Equal("EscrowHoldReleased", result.Reason);
        Assert.Equal(1, provider.ReleaseCalls);
        Assert.Equal(EscrowHoldStatus.Released, state.Hold.Status);
        Assert.Equal(
            MilestoneStatus.Released,
            state.Milestone.Status);
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
        var settlement =
            await context.IdempotencyRecords.SingleAsync();
        Assert.Equal(IdempotencyStatus.Completed, settlement.Status);
        Assert.Equal(
            releaseTransaction.Id,
            settlement.ResultReferenceId);
        Assert.Equal(
            state.Hold.ContractId,
            _completionEvaluator.ContractId);
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
        Assert.NotNull(message);
        Assert.Equal(
            state.Milestone.Id,
            JsonSerializer.Deserialize<FundsReleasedEventPayload>(
                message.Payload,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                })!.MilestoneId);
    }

    [Fact]
    public async Task ForceRelease_RequiresAcceptedHold()
    {
        await using var context = CreateContext();
        var state = await AddAcceptedHoldAsync(
            context,
            holdExpiresAt: _utcNow.AddDays(14));
        state.Milestone.Status = MilestoneStatus.FundedInProgress;
        await context.SaveChangesAsync();
        var provider =
            new TestPaymentProvider(ProviderOperationOutcome.Succeeded);

        var result = await CreateService(
                context,
                provider)
            .ForceReleaseMilestoneAsync(
                state.Milestone.Id,
                CancellationToken.None);

        Assert.Equal(JobExecutionOutcome.NoOp, result.Outcome);
        Assert.Equal("MilestoneNoLongerInAcceptedHold", result.Reason);
        Assert.Equal(0, provider.ReleaseCalls);
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
        "ReleaseRetryScheduled")]
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
        Assert.Equal(1, attempt.ProviderAttemptCount);
        Assert.False(attempt.RequiresManualAction);
        Assert.Equal(
            outcome == ProviderOperationOutcome.Failed
                ? _utcNow.AddMinutes(1)
                : null,
            attempt.NextRetryAt);
        var reservation =
            await context.IdempotencyRecords.SingleAsync();
        Assert.Equal(
            IdempotencyStatus.Processing,
            reservation.Status);
    }

    [Fact]
    public async Task ReleaseProviderCall_HasNoActiveDatabaseTransaction()
    {
        await using var connection =
            new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var context = CreateSqliteContext(connection);
        var createScript = context.Database.GenerateCreateScript()
            .Replace(
                "\"RowVersion\" BLOB NOT NULL",
                "\"RowVersion\" BLOB NOT NULL DEFAULT (randomblob(8))",
                StringComparison.Ordinal);
        await context.Database.ExecuteSqlRawAsync(createScript);
        await context.Database.ExecuteSqlRawAsync(
            "PRAGMA foreign_keys = OFF;");
        var state = await AddAcceptedHoldAsync(
            context,
            holdExpiresAt: _utcNow);
        var provider = new TestPaymentProvider(
            ProviderOperationOutcome.Succeeded,
            context);

        var result = await CreateService(context, provider)
            .ReleaseExpiredHoldAsync(
                state.Hold.Id,
                CancellationToken.None);

        Assert.Equal(JobExecutionOutcome.Completed, result.Outcome);
        Assert.True(provider.CallObservedWithoutTransaction);
        Assert.True(provider.AttemptObservedBeforeCall);
    }

    [Fact]
    public async Task ConfirmedFailures_BackOffThenRequireManualAction()
    {
        var timeProvider = new MutableTimeProvider(_utcNow);
        await using var context = CreateContext(timeProvider);
        var state = await AddAcceptedHoldAsync(
            context,
            holdExpiresAt: _utcNow);
        var provider = new TestPaymentProvider(
            ProviderOperationOutcome.Failed);
        var service = CreateService(
            context,
            provider,
            timeProvider);

        var first = await service.ReleaseExpiredHoldAsync(
            state.Hold.Id,
            CancellationToken.None);
        var early = await service.ReleaseExpiredHoldAsync(
            state.Hold.Id,
            CancellationToken.None);
        timeProvider.Advance(TimeSpan.FromMinutes(1));
        var second = await service.ReleaseExpiredHoldAsync(
            state.Hold.Id,
            CancellationToken.None);
        timeProvider.Advance(TimeSpan.FromMinutes(5));
        var third = await service.ReleaseExpiredHoldAsync(
            state.Hold.Id,
            CancellationToken.None);

        Assert.Equal("ReleaseRetryScheduled", first.Reason);
        Assert.Equal("ReleaseRetryNotDue", early.Reason);
        Assert.Equal("ReleaseRetryScheduled", second.Reason);
        Assert.Equal("ReleaseRequiresManualAction", third.Reason);
        Assert.Equal(3, provider.ReleaseCalls);
        Assert.Single(provider.ReleaseIdempotencyKeys.Distinct());
        var attempt = await context.PaymentTransactions.SingleAsync();
        Assert.Equal(3, attempt.ProviderAttemptCount);
        Assert.True(attempt.RequiresManualAction);
        Assert.Null(attempt.NextRetryAt);
        Assert.Equal(PaymentTransactionStatus.Failed, attempt.Status);
        Assert.Equal(EscrowHoldStatus.Funded, state.Hold.Status);
        Assert.Equal(MilestoneStatus.AcceptedHold, state.Milestone.Status);
    }

    private EscrowReleaseService CreateService(
        ApplicationDbContext context,
        IPaymentProvider paymentProvider,
        TimeProvider? timeProvider = null)
    {
        timeProvider ??= new FixedTimeProvider(_utcNow);
        return new EscrowReleaseService(
            context,
            paymentProvider,
            new OutboxWriter(context, timeProvider),
            _completionEvaluator,
            timeProvider,
            NullLogger<EscrowReleaseService>.Instance);
    }

    private sealed class RecordingCompletionEvaluator
        : IContractCompletionEvaluator
    {
        public Guid? ContractId { get; private set; }

        public Task<ContractActionResultDto> EvaluateCompletionAsync(
            Guid contractId,
            CancellationToken cancellationToken)
        {
            ContractId = contractId;
            return Task.FromResult(new ContractActionResultDto(
                contractId,
                ContractStatus.Completed.ToString(),
                DateTimeOffset.UtcNow));
        }
    }

    private async Task<AcceptedHoldState> AddAcceptedHoldAsync(
        ApplicationDbContext context,
        DateTime? holdExpiresAt,
        MilestoneType type = MilestoneType.Standard)
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
            type == MilestoneType.Standard ? 14 : null,
            null,
            null,
            createdAt,
            type)
        {
            Status = type == MilestoneType.Standard
                ? MilestoneStatus.AcceptedHold
                : MilestoneStatus.ReleasePending,
            FundedAt = createdAt,
            SubmittedAt = type == MilestoneType.Standard
                ? createdAt.AddDays(1)
                : null,
            AcceptedAt = type == MilestoneType.Standard
                ? holdExpiresAt!.Value.AddDays(-14)
                : null,
            AcceptanceSource = type == MilestoneType.Standard
                ? MilestoneAcceptanceSource.Manual
                : null,
            HoldStartsAt = type == MilestoneType.Standard
                ? holdExpiresAt!.Value.AddDays(-14)
                : null,
            HoldExpiresAt = holdExpiresAt,
            SubmissionVersion = type == MilestoneType.Standard ? 1 : 0,
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
            HoldStartsAt = type == MilestoneType.Standard
                ? holdExpiresAt!.Value.AddDays(-14)
                : null,
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

    private ApplicationDbContext CreateContext(
        TimeProvider? timeProvider = null)
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(
                $"escrow-release-{Guid.NewGuid():N}")
            .Options;
        return new ApplicationDbContext(
            options,
            timeProvider ?? new FixedTimeProvider(_utcNow));
    }

    private ApplicationDbContext CreateSqliteContext(
        SqliteConnection connection)
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlite(connection)
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

    private sealed class TestPaymentProvider : IPaymentProvider
    {
        private readonly ProviderOperationOutcome _outcome;
        private readonly ApplicationDbContext? _context;

        public TestPaymentProvider(
            ProviderOperationOutcome outcome,
            ApplicationDbContext? context = null)
        {
            _outcome = outcome;
            _context = context;
        }

        public int ReleaseCalls { get; private set; }
        public List<string> ReleaseIdempotencyKeys { get; } = [];
        public bool CallObservedWithoutTransaction { get; private set; }
        public bool AttemptObservedBeforeCall { get; private set; }

        public async Task<ProviderResult> ReleaseAsync(
            ProviderReleaseRequest request,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ReleaseCalls++;
            ReleaseIdempotencyKeys.Add(request.ProviderIdempotencyKey);
            if (_context is not null)
            {
                CallObservedWithoutTransaction =
                    _context.Database.CurrentTransaction is null;
                AttemptObservedBeforeCall =
                    await _context.PaymentTransactions
                        .AsNoTracking()
                        .AnyAsync(
                            item => item.Id == request.CorrelationId,
                            cancellationToken);
            }

            return new ProviderResult(
                request.Amount,
                request.Currency,
                request.BusinessId,
                request.ProviderIdempotencyKey,
                request.CorrelationId,
                _outcome,
                _outcome == ProviderOperationOutcome.Succeeded
                    ? $"release-{Guid.NewGuid():N}"
                    : null,
                _outcome == ProviderOperationOutcome.Succeeded
                    ? null
                    : "تعذر تنفيذ عملية التحرير.");
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

    private sealed class MutableTimeProvider(DateTime utcNow)
        : TimeProvider
    {
        private DateTime _utcNow = utcNow;

        public override DateTimeOffset GetUtcNow()
            => new(_utcNow);

        public void Advance(TimeSpan duration)
        {
            _utcNow = _utcNow.Add(duration);
        }
    }
}
