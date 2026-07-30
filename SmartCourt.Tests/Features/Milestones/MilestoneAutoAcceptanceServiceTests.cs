using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using SmartCourt.Features.Milestones;
using SmartCourt.Features.Milestones.Entities;
using SmartCourt.Features.Milestones.Enums;
using SmartCourt.Features.Payments.Entities;
using SmartCourt.Features.Payments.Enums;
using SmartCourt.Features.Payments.FundingVerification;
using SmartCourt.Infrastructure.Providers.Events;
using SmartCourt.Infrastructure.Providers.Jobs;
using SmartCourt.Persistence;
using Xunit;

namespace SmartCourt.Tests.Features.Milestones;

public sealed class MilestoneAutoAcceptanceServiceTests
{
    private readonly DateTime _utcNow =
        new(2026, 8, 19, 9, 0, 0, DateTimeKind.Utc);

    [Fact]
    public async Task ExactSevenDayBoundary_AutoAcceptsAndStartsHoldOnce()
    {
        await using var context = CreateContext();
        var chain = await AddSubmittedFundingChainAsync(
            context,
            autoAcceptEligibleAt: _utcNow);
        var service = CreateService(context);

        var first = await service.AutoAcceptAsync(
            chain.Milestone.Id,
            chain.Hold.Id,
            1,
            CancellationToken.None);
        var second = await service.AutoAcceptAsync(
            chain.Milestone.Id,
            chain.Hold.Id,
            1,
            CancellationToken.None);

        Assert.Equal(JobExecutionOutcome.Completed, first.Outcome);
        Assert.Equal("MilestoneAutoAccepted", first.Reason);
        Assert.Equal(JobExecutionOutcome.NoOp, second.Outcome);
        Assert.Equal("MilestoneNoLongerSubmitted", second.Reason);
        Assert.Equal(
            MilestoneStatus.AcceptedHold,
            chain.Milestone.Status);
        Assert.Equal(
            MilestoneAcceptanceSource.Automatic,
            chain.Milestone.AcceptanceSource);
        Assert.Equal(_utcNow, chain.Milestone.AcceptedAt);
        Assert.Equal(_utcNow, chain.Milestone.HoldStartsAt);
        Assert.Equal(
            _utcNow.AddDays(14),
            chain.Milestone.HoldExpiresAt);
        Assert.Null(chain.Milestone.AutoAcceptEligibleAt);
        Assert.Null(chain.Milestone.AutoAcceptJobId);
        Assert.Equal(EscrowHoldStatus.Funded, chain.Hold.Status);
        Assert.Equal(_utcNow, chain.Hold.HoldStartsAt);
        Assert.Equal(_utcNow.AddDays(14), chain.Hold.HoldExpiresAt);

        var history =
            await context.MilestoneStateHistories.SingleAsync();
        Assert.Null(history.ActorUserId);
        Assert.Equal(
            MilestoneStatus.AcceptedHold,
            history.NewStatus);
        var message = await context.OutboxMessages.SingleAsync(
            item =>
                item.EventType
                    == ContractPaymentEventTypes.MilestoneAutoAccepted);
        var payload =
            JsonSerializer.Deserialize<MilestoneAutoAcceptedEventPayload>(
                message.Payload,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
        Assert.NotNull(payload);
        Assert.Equal(chain.Milestone.Id, payload.MilestoneId);
        Assert.Equal(chain.Hold.Id, payload.EscrowHoldId);
        Assert.Equal(1, payload.SubmissionVersion);
    }

    [Fact]
    public async Task BeforeSevenDayBoundary_IsDiagnosticNoOp()
    {
        await using var context = CreateContext();
        var chain = await AddSubmittedFundingChainAsync(
            context,
            autoAcceptEligibleAt: _utcNow.AddTicks(1));
        var service = CreateService(context);

        var result = await service.AutoAcceptAsync(
            chain.Milestone.Id,
            chain.Hold.Id,
            1,
            CancellationToken.None);

        Assert.Equal(JobExecutionOutcome.NoOp, result.Outcome);
        Assert.Equal("AutoAcceptDeadlineNotElapsed", result.Reason);
        Assert.Equal(
            MilestoneStatus.Submitted,
            chain.Milestone.Status);
        Assert.Null(chain.Milestone.AcceptedAt);
        Assert.Null(chain.Hold.HoldStartsAt);
        Assert.Empty(await context.MilestoneStateHistories.ToListAsync());
        Assert.Empty(await context.OutboxMessages.ToListAsync());
    }

    [Fact]
    public async Task StaleVersionOrHold_IsNoOpWithoutDomainMutation()
    {
        await using var versionContext = CreateContext();
        var versionChain = await AddSubmittedFundingChainAsync(
            versionContext,
            autoAcceptEligibleAt: _utcNow);
        var versionResult =
            await CreateService(versionContext).AutoAcceptAsync(
                versionChain.Milestone.Id,
                versionChain.Hold.Id,
                2,
                CancellationToken.None);

        Assert.Equal(JobExecutionOutcome.NoOp, versionResult.Outcome);
        Assert.Equal("SubmissionVersionIsStale", versionResult.Reason);
        Assert.Equal(
            MilestoneStatus.Submitted,
            versionChain.Milestone.Status);
        Assert.Empty(
            await versionContext.OutboxMessages.ToListAsync());

        await using var holdContext = CreateContext();
        var holdChain = await AddSubmittedFundingChainAsync(
            holdContext,
            autoAcceptEligibleAt: _utcNow);
        var holdResult =
            await CreateService(holdContext).AutoAcceptAsync(
                holdChain.Milestone.Id,
                Guid.NewGuid(),
                1,
                CancellationToken.None);

        Assert.Equal(JobExecutionOutcome.NoOp, holdResult.Outcome);
        Assert.Equal(
            "SubmissionDoesNotMatchJobHold",
            holdResult.Reason);
        Assert.Equal(
            MilestoneStatus.Submitted,
            holdChain.Milestone.Status);
        Assert.Empty(await holdContext.OutboxMessages.ToListAsync());
    }

    [Fact]
    public async Task InvalidFundingOrPendingChangeRequest_IsNoOp()
    {
        await using var fundingContext = CreateContext();
        var fundingChain = await AddSubmittedFundingChainAsync(
            fundingContext,
            autoAcceptEligibleAt: _utcNow);
        fundingChain.PaymentTransaction.Amount += 1m;
        await fundingContext.SaveChangesAsync();

        var fundingResult =
            await CreateService(fundingContext).AutoAcceptAsync(
                fundingChain.Milestone.Id,
                fundingChain.Hold.Id,
                1,
                CancellationToken.None);

        Assert.Equal(JobExecutionOutcome.NoOp, fundingResult.Outcome);
        Assert.Equal(
            "MilestoneFundingChainIsInvalid",
            fundingResult.Reason);
        Assert.Equal(
            MilestoneStatus.Submitted,
            fundingChain.Milestone.Status);
        Assert.Empty(
            await fundingContext.OutboxMessages.ToListAsync());

        await using var requestContext = CreateContext();
        var requestChain = await AddSubmittedFundingChainAsync(
            requestContext,
            autoAcceptEligibleAt: _utcNow);
        requestContext.MilestoneChangeRequests.Add(
            new MilestoneChangeRequest(
                Guid.NewGuid(),
                requestChain.Milestone.Id,
                Guid.NewGuid(),
                null,
                21,
                null,
                "طلب تمديد معلق.",
                _utcNow.AddDays(-1)));
        await requestContext.SaveChangesAsync();

        var requestResult =
            await CreateService(requestContext).AutoAcceptAsync(
                requestChain.Milestone.Id,
                requestChain.Hold.Id,
                1,
                CancellationToken.None);

        Assert.Equal(JobExecutionOutcome.NoOp, requestResult.Outcome);
        Assert.Equal(
            "PendingMilestoneChangeRequestExists",
            requestResult.Reason);
        Assert.Equal(
            MilestoneStatus.Submitted,
            requestChain.Milestone.Status);
        Assert.Empty(
            await requestContext.OutboxMessages.ToListAsync());
    }

    [Fact]
    public async Task SupersededReviewState_IsNoOp()
    {
        await using var context = CreateContext();
        var chain = await AddSubmittedFundingChainAsync(
            context,
            autoAcceptEligibleAt: _utcNow);
        chain.Milestone.Status = MilestoneStatus.FundedInProgress;
        chain.Milestone.SubmittedAt = null;
        chain.Milestone.AutoAcceptEligibleAt = null;
        chain.Milestone.AutoAcceptJobId = null;
        await context.SaveChangesAsync();

        var result = await CreateService(context).AutoAcceptAsync(
            chain.Milestone.Id,
            chain.Hold.Id,
            1,
            CancellationToken.None);

        Assert.Equal(JobExecutionOutcome.NoOp, result.Outcome);
        Assert.Equal("MilestoneNoLongerSubmitted", result.Reason);
        Assert.Equal(
            MilestoneStatus.FundedInProgress,
            chain.Milestone.Status);
        Assert.Empty(await context.MilestoneStateHistories.ToListAsync());
        Assert.Empty(await context.OutboxMessages.ToListAsync());
    }

    private MilestoneAutoAcceptanceService CreateService(
        ApplicationDbContext context)
    {
        var timeProvider = new FixedTimeProvider(_utcNow);
        return new MilestoneAutoAcceptanceService(
            context,
            new MilestoneFundingVerifier(context),
            new OutboxWriter(context, timeProvider),
            timeProvider,
            NullLogger<MilestoneAutoAcceptanceService>.Instance);
    }

    private async Task<SubmittedFundingChain>
        AddSubmittedFundingChainAsync(
            ApplicationDbContext context,
            DateTime autoAcceptEligibleAt)
    {
        var contractId = Guid.NewGuid();
        var lawyerUserId = Guid.NewGuid();
        var fundedAt = _utcNow.AddDays(-7);
        var milestone = new Milestone(
            Guid.NewGuid(),
            contractId,
            "تقديم المذكرة النهائية",
            null,
            1,
            1_000m,
            14,
            null,
            fundedAt)
        {
            Status = MilestoneStatus.Submitted,
            FundedAt = fundedAt,
            SubmittedAt = fundedAt,
            AutoAcceptEligibleAt = autoAcceptEligibleAt,
            AutoAcceptJobId = "auto-accept-job",
            SubmissionVersion = 1,
            RowVersion = [1, 2, 3, 4]
        };
        var account = new EscrowAccount(
            Guid.NewGuid(),
            contractId,
            fundedAt);
        var transactionId = Guid.NewGuid();
        var hold = new EscrowHold(
            Guid.NewGuid(),
            account.Id,
            contractId,
            milestone.Id,
            milestone.Amount,
            50m,
            950m,
            transactionId,
            fundedAt,
            fundedAt);
        var paymentTransaction = new PaymentTransaction(
            transactionId,
            contractId,
            milestone.Id,
            PaymentOperationType.Deposit,
            "MockPaymentProvider",
            $"funding-{Guid.NewGuid():N}",
            milestone.Amount,
            fundedAt)
        {
            EscrowHoldId = hold.Id,
            ProviderTransactionId =
                $"provider-{Guid.NewGuid():N}",
            Status = PaymentTransactionStatus.Completed,
            ProcessedAt = fundedAt,
            UpdatedAt = fundedAt
        };
        var submission = new MilestoneSubmission(
            Guid.NewGuid(),
            milestone.Id,
            hold.Id,
            lawyerUserId,
            1,
            "تم تسليم أعمال المرحلة.",
            fundedAt);
        context.Milestones.Add(milestone);
        context.EscrowAccounts.Add(account);
        context.EscrowHolds.Add(hold);
        context.PaymentTransactions.Add(paymentTransaction);
        context.MilestoneSubmissions.Add(submission);
        await context.SaveChangesAsync();
        return new SubmittedFundingChain(
            milestone,
            hold,
            paymentTransaction);
    }

    private ApplicationDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(
                $"milestone-auto-accept-{Guid.NewGuid():N}")
            .Options;
        return new ApplicationDbContext(
            options,
            new FixedTimeProvider(_utcNow));
    }

    private sealed record SubmittedFundingChain(
        Milestone Milestone,
        EscrowHold Hold,
        PaymentTransaction PaymentTransaction);

    private sealed class FixedTimeProvider(DateTime utcNow)
        : TimeProvider
    {
        public override DateTimeOffset GetUtcNow()
            => new(utcNow);
    }
}
