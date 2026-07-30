using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using SmartCourt.Features.Payments;
using SmartCourt.Features.Contracts.Entities;
using SmartCourt.Features.Contracts.Enums;
using SmartCourt.Features.Milestones.Entities;
using SmartCourt.Features.Milestones.Enums;
using SmartCourt.Features.Payments.Entities;
using SmartCourt.Features.Payments.Enums;
using SmartCourt.Infrastructure.Persistence.Enums;
using SmartCourt.Infrastructure.Providers.Events;
using SmartCourt.Infrastructure.Providers.Payments;
using SmartCourt.Persistence;
using Xunit;

namespace SmartCourt.Tests.Features.Payments;

public sealed class ContractTerminationSettlementServiceTests
{
    private readonly DateTime _utcNow =
        new(2026, 7, 30, 12, 0, 0, DateTimeKind.Utc);

    [Fact]
    public async Task EligibleUnstartedHold_IsRefundedExactlyOnce()
    {
        await using var context = CreateContext();
        var state = await AddFundedStateAsync(context);
        var provider = new TestPaymentProvider(
            ProviderOperationOutcome.Succeeded);
        var service = CreateService(context, provider);

        var result = await service.SettleForTerminationAsync(
            state.Contract.Id,
            state.Contract.ClientUserId,
            "اتفق الطرفان على إنهاء العقد ورد تمويل المرحلة غير المنفذة.",
            Guid.NewGuid(),
            CancellationToken.None);

        Assert.True(result.Completed);
        Assert.Equal(1_000m, result.GrossAmount);
        Assert.Equal(1_000m, result.ClientRefundAmount);
        Assert.Equal(EscrowHoldStatus.Refunded, state.Hold.Status);
        Assert.Equal(SettlementType.Refund, state.Hold.SettlementType);
        Assert.Equal(MilestoneStatus.Refunded, state.Milestone.Status);
        Assert.Equal(1_000m, state.Account.TotalRefunded);
        Assert.Equal(0m, state.Wallet.PendingBalance);
        Assert.Equal(1, provider.RefundCalls);
        var ledger = await context.EscrowLedgerEntries.SingleAsync();
        Assert.Equal(LedgerTransactionType.Refund, ledger.TransactionType);
        Assert.Equal(1_000m, ledger.Amount);
        Assert.Equal(0m, ledger.RunningBalance);
        Assert.Equal(
            PaymentTransactionStatus.Completed,
            (await context.PaymentTransactions.SingleAsync()).Status);
        Assert.Equal(
            IdempotencyStatus.Completed,
            (await context.IdempotencyRecords.SingleAsync()).Status);
        Assert.Single(
            await context.OutboxMessages
                .Where(item =>
                    item.EventType
                    == ContractPaymentEventTypes.FundsRefunded)
                .ToListAsync());

        var replay = await service.SettleForTerminationAsync(
            state.Contract.Id,
            state.Contract.ClientUserId,
            "اتفق الطرفان على إنهاء العقد ورد تمويل المرحلة غير المنفذة.",
            Guid.NewGuid(),
            CancellationToken.None);

        Assert.True(replay.Completed);
        Assert.Equal(1, provider.RefundCalls);
        Assert.Single(await context.EscrowLedgerEntries.ToListAsync());
        Assert.Single(await context.PaymentTransactions.ToListAsync());
    }

    [Fact]
    public async Task ProviderFailure_LeavesRefundPendingAndRetryable()
    {
        await using var context = CreateContext();
        var state = await AddFundedStateAsync(context);
        var provider = new TestPaymentProvider(
            ProviderOperationOutcome.Failed,
            ProviderOperationOutcome.Succeeded);
        var service = CreateService(context, provider);

        var first = await service.SettleForTerminationAsync(
            state.Contract.Id,
            state.Contract.ClientUserId,
            "إنهاء العقد ورد التمويل.",
            Guid.NewGuid(),
            CancellationToken.None);

        Assert.False(first.Completed);
        Assert.Equal(EscrowHoldStatus.Funded, state.Hold.Status);
        Assert.Equal(MilestoneStatus.FundedInProgress, state.Milestone.Status);
        Assert.Empty(await context.EscrowLedgerEntries.ToListAsync());
        Assert.Equal(
            PaymentTransactionStatus.Processing,
            (await context.PaymentTransactions.SingleAsync()).Status);

        var second = await service.SettleForTerminationAsync(
            state.Contract.Id,
            state.Contract.ClientUserId,
            "إنهاء العقد ورد التمويل.",
            Guid.NewGuid(),
            CancellationToken.None);

        Assert.True(second.Completed);
        Assert.Equal(2, provider.RefundCalls);
        Assert.Equal(EscrowHoldStatus.Refunded, state.Hold.Status);
        Assert.Single(await context.PaymentTransactions.ToListAsync());
        Assert.Single(await context.EscrowLedgerEntries.ToListAsync());
    }

    [Theory]
    [InlineData(MilestoneStatus.Submitted)]
    [InlineData(MilestoneStatus.AcceptedHold)]
    [InlineData(MilestoneStatus.Disputed)]
    public async Task StartedOrDisputedMilestone_RequiresSeparateSettlement(
        MilestoneStatus status)
    {
        await using var context = CreateContext();
        var state = await AddFundedStateAsync(context);
        state.Milestone.Status = status;
        if (status == MilestoneStatus.Submitted)
        {
            state.Milestone.SubmittedAt = _utcNow.AddMinutes(-10);
        }

        if (status == MilestoneStatus.AcceptedHold)
        {
            state.Milestone.SubmittedAt = _utcNow.AddHours(-1);
            state.Milestone.AcceptedAt = _utcNow.AddMinutes(-30);
            state.Milestone.HoldStartsAt = _utcNow.AddMinutes(-30);
            state.Milestone.HoldExpiresAt = _utcNow.AddDays(14);
            state.Hold.HoldStartsAt = state.Milestone.HoldStartsAt;
            state.Hold.HoldExpiresAt = state.Milestone.HoldExpiresAt;
        }

        if (status == MilestoneStatus.Disputed)
        {
            state.Hold.Status = EscrowHoldStatus.Frozen;
        }

        await context.SaveChangesAsync();
        var provider = new TestPaymentProvider(
            ProviderOperationOutcome.Succeeded);
        var service = CreateService(context, provider);

        var result = await service.SettleForTerminationAsync(
            state.Contract.Id,
            state.Contract.ClientUserId,
            "طلب إنهاء العقد.",
            Guid.NewGuid(),
            CancellationToken.None);

        Assert.False(result.Completed);
        Assert.Equal(0, provider.RefundCalls);
        Assert.Empty(await context.PaymentTransactions.ToListAsync());
        Assert.Empty(await context.EscrowLedgerEntries.ToListAsync());
    }

    private ContractTerminationSettlementService CreateService(
        ApplicationDbContext context,
        IPaymentProvider provider)
    {
        var timeProvider = new FixedTimeProvider(_utcNow);
        return new ContractTerminationSettlementService(
            context,
            provider,
            new OutboxWriter(context, timeProvider),
            timeProvider,
            NullLogger<ContractTerminationSettlementService>.Instance);
    }

    private async Task<FundedState> AddFundedStateAsync(
        ApplicationDbContext context)
    {
        var clientUserId = Guid.NewGuid();
        var lawyerUserId = Guid.NewGuid();
        var contract = new Contract(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            clientUserId,
            lawyerUserId,
            "عقد تمثيل قانوني",
            "شروط عقد صالحة لاختبار إنهاء العقد.",
            _utcNow.AddDays(-2))
        {
            Status = ContractStatus.Active,
            ActivatedAt = _utcNow.AddDays(-1)
        };
        var milestone = new Milestone(
            Guid.NewGuid(),
            contract.Id,
            "المرحلة الأولى",
            null,
            1,
            1_000m,
            14,
            null,
            _utcNow.AddDays(-1))
        {
            Status = MilestoneStatus.FundedInProgress,
            FundedAt = _utcNow.AddHours(-1)
        };
        var account = new EscrowAccount(
            Guid.NewGuid(),
            contract.Id,
            _utcNow.AddDays(-1))
        {
            TotalDeposited = 1_000m
        };
        var hold = new EscrowHold(
            Guid.NewGuid(),
            account.Id,
            contract.Id,
            milestone.Id,
            1_000m,
            50m,
            950m,
            Guid.NewGuid(),
            _utcNow.AddHours(-1),
            _utcNow.AddHours(-1));
        var wallet = new LawyerWallet(
            Guid.NewGuid(),
            lawyerUserId,
            _utcNow.AddDays(-1))
        {
            PendingBalance = 950m
        };
        context.AddRange(contract, milestone, account, hold, wallet);
        await context.SaveChangesAsync();
        return new FundedState(
            contract,
            milestone,
            account,
            hold,
            wallet);
    }

    private ApplicationDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(
                $"termination-settlement-{Guid.NewGuid():N}")
            .Options;
        return new ApplicationDbContext(
            options,
            new FixedTimeProvider(_utcNow));
    }

    private sealed record FundedState(
        Contract Contract,
        Milestone Milestone,
        EscrowAccount Account,
        EscrowHold Hold,
        LawyerWallet Wallet);

    private sealed class TestPaymentProvider(
        params ProviderOperationOutcome[] outcomes) : IPaymentProvider
    {
        private readonly Queue<ProviderOperationOutcome> _outcomes =
            new(outcomes);

        public int RefundCalls { get; private set; }

        public Task<ProviderResult> RefundAsync(
            ProviderRefundRequest request,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            RefundCalls++;
            var outcome = _outcomes.Dequeue();
            return Task.FromResult(
                new ProviderResult(
                    request.Amount,
                    request.Currency,
                    request.BusinessId,
                    request.ProviderIdempotencyKey,
                    request.CorrelationId,
                    outcome,
                    outcome == ProviderOperationOutcome.Succeeded
                        ? $"refund-{Guid.NewGuid():N}"
                        : null,
                    outcome == ProviderOperationOutcome.Succeeded
                        ? null
                        : "تعذر تنفيذ عملية رد التمويل."));
        }

        public Task<ProviderResult> DepositAsync(
            ProviderDepositRequest request,
            CancellationToken cancellationToken)
            => throw new NotSupportedException();

        public Task<ProviderResult> RetryDepositAsync(
            ProviderDepositRetryRequest request,
            CancellationToken cancellationToken)
            => throw new NotSupportedException();

        public Task<ProviderResult> ReleaseAsync(
            ProviderReleaseRequest request,
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
