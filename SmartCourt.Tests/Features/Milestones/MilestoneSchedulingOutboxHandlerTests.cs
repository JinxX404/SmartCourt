using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using SmartCourt.Features.Milestones.Entities;
using SmartCourt.Features.Milestones.Enums;
using SmartCourt.Features.Milestones.Events;
using SmartCourt.Infrastructure.Persistence.Entities;
using SmartCourt.Infrastructure.Providers.Events;
using SmartCourt.Infrastructure.Providers.Jobs;
using SmartCourt.Persistence;
using Xunit;

namespace SmartCourt.Tests.Features.Milestones;

public sealed class MilestoneSchedulingOutboxHandlerTests
{
    private static readonly DateTime NowUtc =
        new(2026, 8, 3, 10, 0, 0, DateTimeKind.Utc);

    [Fact]
    public async Task SubmittedEvent_SchedulesExactVersionAndStoresDiagnosticJobId()
    {
        await using var context = CreateContext();
        var milestone = CreateMilestone(MilestoneStatus.Submitted, 2);
        var holdId = Guid.NewGuid();
        milestone.AutoAcceptEligibleAt = NowUtc.AddDays(7);
        context.Milestones.Add(milestone);
        context.MilestoneSubmissions.Add(
            new MilestoneSubmission(
                Guid.NewGuid(),
                milestone.Id,
                holdId,
                Guid.NewGuid(),
                2,
                "Completed work",
                NowUtc));
        await context.SaveChangesAsync();
        var scheduler = new RecordingScheduler();
        var handler = new MilestoneSchedulingOutboxHandler(
            context,
            scheduler);

        await handler.HandleAsync(
            CreateMessage(
                ContractPaymentEventTypes.MilestoneSubmitted,
                new MilestoneSubmissionEventPayload(
                    milestone.Id,
                    holdId,
                    2)),
            CancellationToken.None);

        Assert.Equal("job-1", milestone.AutoAcceptJobId);
        Assert.Equal(
            (milestone.Id, holdId, 2, milestone.AutoAcceptEligibleAt.Value),
            scheduler.AutoAcceptCall);
    }

    [Fact]
    public async Task StaleSubmissionEvent_CompletesWithoutScheduling()
    {
        await using var context = CreateContext();
        var milestone = CreateMilestone(MilestoneStatus.Submitted, 4);
        milestone.AutoAcceptEligibleAt = NowUtc.AddDays(7);
        context.Milestones.Add(milestone);
        await context.SaveChangesAsync();
        var scheduler = new RecordingScheduler();
        var handler = new MilestoneSchedulingOutboxHandler(
            context,
            scheduler);

        await handler.HandleAsync(
            CreateMessage(
                ContractPaymentEventTypes.MilestoneSubmitted,
                new MilestoneSubmissionEventPayload(
                    milestone.Id,
                    Guid.NewGuid(),
                    3)),
            CancellationToken.None);

        Assert.Null(scheduler.AutoAcceptCall);
        Assert.Null(milestone.AutoAcceptJobId);
    }

    [Fact]
    public async Task SchedulingFailure_RemainsObservableForOutboxRetry()
    {
        await using var context = CreateContext();
        var milestone = CreateMilestone(MilestoneStatus.Submitted, 1);
        var holdId = Guid.NewGuid();
        milestone.AutoAcceptEligibleAt = NowUtc.AddDays(7);
        context.Milestones.Add(milestone);
        context.MilestoneSubmissions.Add(
            new MilestoneSubmission(
                Guid.NewGuid(),
                milestone.Id,
                holdId,
                Guid.NewGuid(),
                1,
                "Completed work",
                NowUtc));
        await context.SaveChangesAsync();
        var scheduler = new RecordingScheduler
        {
            FailScheduling = true
        };
        var handler = new MilestoneSchedulingOutboxHandler(
            context,
            scheduler);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => handler.HandleAsync(
                CreateMessage(
                    ContractPaymentEventTypes.MilestoneSubmitted,
                    new MilestoneSubmissionEventPayload(
                        milestone.Id,
                        holdId,
                        1)),
                CancellationToken.None));

        Assert.Null(milestone.AutoAcceptJobId);
    }

    [Fact]
    public async Task AcceptedEvent_SchedulesReleaseAtAuthoritativeExpiry()
    {
        await using var context = CreateContext();
        var milestone = CreateMilestone(MilestoneStatus.AcceptedHold, 1);
        var holdId = Guid.NewGuid();
        milestone.HoldExpiresAt = NowUtc.AddDays(14);
        context.Milestones.Add(milestone);
        await context.SaveChangesAsync();
        var scheduler = new RecordingScheduler();
        var handler = new MilestoneSchedulingOutboxHandler(
            context,
            scheduler);

        await handler.HandleAsync(
            CreateMessage(
                ContractPaymentEventTypes.MilestoneAccepted,
                new MilestoneAcceptanceEventPayload(
                    milestone.Id,
                    holdId)),
            CancellationToken.None);

        Assert.Equal(
            (holdId, milestone.HoldExpiresAt.Value),
            scheduler.ReleaseCall);
    }

    private static ApplicationDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase($"job-handler-{Guid.NewGuid():N}")
            .Options;
        return new ApplicationDbContext(options);
    }

    private static Milestone CreateMilestone(
        MilestoneStatus status,
        int submissionVersion)
    {
        var milestone = new Milestone(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Milestone",
            null,
            1,
            1_000m,
            10,
            NowUtc.AddDays(10),
            NowUtc);
        milestone.Status = status;
        milestone.SubmissionVersion = submissionVersion;
        return milestone;
    }

    private static OutboxMessage CreateMessage(
        string eventType,
        object payload)
    {
        return new OutboxMessage(
            Guid.NewGuid(),
            eventType,
            1,
            JsonSerializer.Serialize(
                payload,
                new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                }),
            "Milestone",
            Guid.NewGuid(),
            Guid.NewGuid(),
            NowUtc,
            NowUtc);
    }

    private sealed class RecordingScheduler : IContractJobScheduler
    {
        public bool FailScheduling { get; init; }
        public (Guid MilestoneId, Guid HoldId, int Version, DateTime RunAt)?
            AutoAcceptCall { get; private set; }
        public (Guid HoldId, DateTime RunAt)?
            ReleaseCall { get; private set; }

        public Task<string> ScheduleAutoAcceptAsync(
            Guid milestoneId,
            Guid escrowHoldId,
            int submissionVersion,
            DateTime runAtUtc,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (FailScheduling)
            {
                throw new InvalidOperationException("scheduler unavailable");
            }

            AutoAcceptCall = (
                milestoneId,
                escrowHoldId,
                submissionVersion,
                runAtUtc);
            return Task.FromResult("job-1");
        }

        public Task<string> ScheduleReleaseExpiredHoldAsync(
            Guid escrowHoldId,
            DateTime runAtUtc,
            CancellationToken cancellationToken)
        {
            ReleaseCall = (escrowHoldId, runAtUtc);
            return Task.FromResult("job-2");
        }

        public Task<string> ScheduleProviderReconciliationAsync(
            Guid paymentTransactionId,
            DateTime runAtUtc,
            CancellationToken cancellationToken)
        {
            return Task.FromResult("job-3");
        }

        public Task<string> ScheduleProviderRetryAsync(
            Guid paymentTransactionId,
            DateTime runAtUtc,
            CancellationToken cancellationToken)
        {
            return Task.FromResult("job-4");
        }

        public Task<string> ScheduleSchedulingReconciliationAsync(
            DateTime runAtUtc,
            CancellationToken cancellationToken)
        {
            return Task.FromResult("job-5");
        }

        public Task<string> SchedulePendingWalletProjectionReconciliationAsync(
            DateTime runAtUtc,
            CancellationToken cancellationToken)
        {
            return Task.FromResult("job-6");
        }

        public Task<string> ScheduleOutboxDispatchAsync(
            int batchSize,
            DateTime runAtUtc,
            CancellationToken cancellationToken)
        {
            return Task.FromResult("job-7");
        }
    }
}
