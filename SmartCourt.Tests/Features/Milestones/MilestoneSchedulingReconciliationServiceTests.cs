using Microsoft.EntityFrameworkCore;
using SmartCourt.Features.Milestones;
using SmartCourt.Features.Milestones.Entities;
using SmartCourt.Features.Milestones.Enums;
using SmartCourt.Features.Payments.Entities;
using SmartCourt.Infrastructure.Providers.Jobs;
using SmartCourt.Persistence;
using Xunit;

namespace SmartCourt.Tests.Features.Milestones;

public sealed class MilestoneSchedulingReconciliationServiceTests
{
    private static readonly DateTime NowUtc =
        new(2026, 8, 3, 10, 0, 0, DateTimeKind.Utc);

    [Fact]
    public async Task Reconcile_SchedulesMissingAutoAcceptAndExpiredRelease()
    {
        await using var context = CreateContext();
        var submitted = CreateMilestone(
            MilestoneStatus.Submitted,
            1);
        submitted.AutoAcceptEligibleAt = NowUtc.AddDays(-1);
        var submittedHoldId = Guid.NewGuid();
        context.Milestones.Add(submitted);
        context.MilestoneSubmissions.Add(
            new MilestoneSubmission(
                Guid.NewGuid(),
                submitted.Id,
                submittedHoldId,
                Guid.NewGuid(),
                1,
                "Submitted",
                NowUtc.AddDays(-8)));

        var accepted = CreateMilestone(
            MilestoneStatus.AcceptedHold,
            1);
        accepted.HoldExpiresAt = NowUtc.AddMinutes(-1);
        var acceptedHoldId = Guid.NewGuid();
        context.Milestones.Add(accepted);
        context.MilestoneSubmissions.Add(
            new MilestoneSubmission(
                Guid.NewGuid(),
                accepted.Id,
                acceptedHoldId,
                Guid.NewGuid(),
                1,
                "Accepted",
                NowUtc.AddDays(-20)));

        var expense = CreateMilestone(
            MilestoneStatus.ReleasePending,
            0,
            MilestoneType.Expense);
        expense.FundedAt = NowUtc.AddMinutes(-2);
        var expenseHold = new EscrowHold(
            Guid.NewGuid(),
            Guid.NewGuid(),
            expense.ContractId,
            expense.Id,
            1_000m,
            50m,
            950m,
            Guid.NewGuid(),
            expense.FundedAt.Value,
            expense.FundedAt.Value);
        context.Milestones.Add(expense);
        context.EscrowHolds.Add(expenseHold);
        await context.SaveChangesAsync();

        var scheduler = new RecordingScheduler();
        var service = new MilestoneSchedulingReconciliationService(
            context,
            scheduler,
            new FixedTimeProvider(NowUtc));

        var result = await service.ReconcileAsync(
            CancellationToken.None);

        Assert.Equal(JobExecutionOutcome.Completed, result.Outcome);
        Assert.Equal(3, result.AffectedCount);
        Assert.NotNull(submitted.AutoAcceptJobId);
        Assert.Contains(
            (submitted.Id, submittedHoldId, 1, submitted.AutoAcceptEligibleAt.Value),
            scheduler.AutoAcceptCalls);
        Assert.Contains(
            (acceptedHoldId, NowUtc),
            scheduler.ReleaseCalls);
        Assert.Contains(
            (expenseHold.Id, NowUtc),
            scheduler.ReleaseCalls);
    }

    [Fact]
    public async Task Reconcile_WithNoCandidatesReturnsStructuredNoOp()
    {
        await using var context = CreateContext();
        var service = new MilestoneSchedulingReconciliationService(
            context,
            new RecordingScheduler(),
            new FixedTimeProvider(NowUtc));

        var result = await service.ReconcileAsync(
            CancellationToken.None);

        Assert.Equal(JobExecutionOutcome.NoOp, result.Outcome);
        Assert.Equal("NoMissingSchedulesFound", result.Reason);
    }

    private static ApplicationDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase($"schedule-reconcile-{Guid.NewGuid():N}")
            .Options;
        return new ApplicationDbContext(options);
    }

    private static Milestone CreateMilestone(
        MilestoneStatus status,
        int submissionVersion,
        MilestoneType type = MilestoneType.Standard)
    {
        var milestone = new Milestone(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Milestone",
            null,
            1,
            1_000m,
            type == MilestoneType.Standard ? 10 : null,
            NowUtc.AddDays(10),
            null,
            NowUtc,
            type);
        milestone.Status = status;
        milestone.SubmissionVersion = submissionVersion;
        return milestone;
    }

    private sealed class FixedTimeProvider(DateTime utcNow)
        : TimeProvider
    {
        public override DateTimeOffset GetUtcNow()
        {
            return new DateTimeOffset(utcNow);
        }
    }

    private sealed class RecordingScheduler : IContractJobScheduler
    {
        public List<(Guid MilestoneId, Guid HoldId, int Version, DateTimeOffset RunAt)>
            AutoAcceptCalls { get; } = [];
        public List<(Guid HoldId, DateTimeOffset RunAt)> ReleaseCalls { get; } = [];

        public Task<string> ScheduleAutoAcceptAsync(
            Guid milestoneId,
            Guid escrowHoldId,
            int submissionVersion,
            DateTimeOffset runAtUtc,
            CancellationToken cancellationToken)
        {
            AutoAcceptCalls.Add((
                milestoneId,
                escrowHoldId,
                submissionVersion,
                runAtUtc));
            return Task.FromResult($"auto-{AutoAcceptCalls.Count}");
        }

        public Task<string> ScheduleReleaseExpiredHoldAsync(
            Guid escrowHoldId,
            DateTimeOffset runAtUtc,
            CancellationToken cancellationToken)
        {
            ReleaseCalls.Add((escrowHoldId, runAtUtc));
            return Task.FromResult($"release-{ReleaseCalls.Count}");
        }

        public Task<string> ScheduleProviderReconciliationAsync(
            Guid paymentTransactionId,
            DateTimeOffset runAtUtc,
            CancellationToken cancellationToken)
        {
            return Task.FromResult("provider-reconcile");
        }

        public Task<string> ScheduleProviderRetryAsync(
            Guid paymentTransactionId,
            DateTimeOffset runAtUtc,
            CancellationToken cancellationToken)
        {
            return Task.FromResult("provider-retry");
        }

        public Task<string> ScheduleSchedulingReconciliationAsync(
            DateTimeOffset runAtUtc,
            CancellationToken cancellationToken)
        {
            return Task.FromResult("schedule-reconcile");
        }

        public Task<string> SchedulePendingWalletProjectionReconciliationAsync(
            DateTimeOffset runAtUtc,
            CancellationToken cancellationToken)
        {
            return Task.FromResult("wallet-reconcile");
        }

        public Task<string> ScheduleOutboxDispatchAsync(
            int batchSize,
            DateTimeOffset runAtUtc,
            CancellationToken cancellationToken)
        {
            return Task.FromResult("outbox");
        }
    }
}
