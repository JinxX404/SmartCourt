using Microsoft.EntityFrameworkCore;
using SmartCourt.Features.Milestones.Enums;
using SmartCourt.Infrastructure.Providers.Jobs;
using SmartCourt.Persistence;

namespace SmartCourt.Features.Milestones;

public sealed class MilestoneSchedulingReconciliationService
    : IMilestoneSchedulingReconciliationService
{
    private const int BatchSize = 100;

    private readonly ApplicationDbContext _dbContext;
    private readonly IContractJobScheduler _jobScheduler;
    private readonly TimeProvider _timeProvider;

    public MilestoneSchedulingReconciliationService(
        ApplicationDbContext dbContext,
        IContractJobScheduler jobScheduler,
        TimeProvider timeProvider)
    {
        _dbContext = dbContext;
        _jobScheduler = jobScheduler;
        _timeProvider = timeProvider;
    }

    public async Task<JobExecutionResult> ReconcileAsync(
        CancellationToken cancellationToken)
    {
        var scheduled = await ReconcileAutoAcceptAsync(cancellationToken);
        scheduled += await ReconcileExpiredHoldsAsync(cancellationToken);
        scheduled += await ReconcileExpenseReleasesAsync(cancellationToken);
        return scheduled == 0
            ? JobExecutionResult.NoOp("NoMissingSchedulesFound")
            : JobExecutionResult.Completed(
                "MissingSchedulesRecovered",
                scheduled);
    }

    private async Task<int> ReconcileAutoAcceptAsync(
        CancellationToken cancellationToken)
    {
        var milestones = await _dbContext.Milestones
            .Where(milestone =>
                milestone.Type == MilestoneType.Standard
                && milestone.Status == MilestoneStatus.Submitted
                && milestone.AutoAcceptEligibleAt != null
                && milestone.AutoAcceptJobId == null)
            .OrderBy(milestone => milestone.AutoAcceptEligibleAt)
            .Take(BatchSize)
            .ToListAsync(cancellationToken);
        var scheduled = 0;
        foreach (var milestone in milestones)
        {
            var submission = await _dbContext.MilestoneSubmissions
                .AsNoTracking()
                .SingleOrDefaultAsync(
                    item =>
                        item.MilestoneId == milestone.Id
                        && item.Version == milestone.SubmissionVersion,
                    cancellationToken);
            if (submission is null)
            {
                continue;
            }

            var jobId = await _jobScheduler.ScheduleAutoAcceptAsync(
                milestone.Id,
                submission.EscrowHoldId,
                milestone.SubmissionVersion,
                milestone.AutoAcceptEligibleAt!.Value,
                cancellationToken);
            milestone.AutoAcceptJobId = jobId;
            scheduled++;
        }

        if (scheduled > 0)
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        return scheduled;
    }

    private async Task<int> ReconcileExpiredHoldsAsync(
        CancellationToken cancellationToken)
    {
        var now = _timeProvider.GetUtcNow().UtcDateTime;
        var milestones = await _dbContext.Milestones
            .AsNoTracking()
            .Where(milestone =>
                milestone.Type == MilestoneType.Standard
                && milestone.Status == MilestoneStatus.AcceptedHold
                && milestone.HoldExpiresAt != null
                && milestone.HoldExpiresAt <= now)
            .OrderBy(milestone => milestone.HoldExpiresAt)
            .Take(BatchSize)
            .ToListAsync(cancellationToken);
        var scheduled = 0;
        foreach (var milestone in milestones)
        {
            var submission = await _dbContext.MilestoneSubmissions
                .AsNoTracking()
                .SingleOrDefaultAsync(
                    item =>
                        item.MilestoneId == milestone.Id
                        && item.Version == milestone.SubmissionVersion,
                    cancellationToken);
            if (submission is null)
            {
                continue;
            }

            await _jobScheduler.ScheduleReleaseExpiredHoldAsync(
                submission.EscrowHoldId,
                now,
                cancellationToken);
            scheduled++;
        }

        return scheduled;
    }

    private async Task<int> ReconcileExpenseReleasesAsync(
        CancellationToken cancellationToken)
    {
        var now = _timeProvider.GetUtcNow().UtcDateTime;
        var milestones = await _dbContext.Milestones
            .AsNoTracking()
            .Where(milestone =>
                milestone.Type == MilestoneType.Expense
                && milestone.Status == MilestoneStatus.ReleasePending
                && milestone.FundedAt != null)
            .OrderBy(milestone => milestone.FundedAt)
            .Take(BatchSize)
            .ToListAsync(cancellationToken);
        var scheduled = 0;
        foreach (var milestone in milestones)
        {
            var escrowHoldId = await _dbContext.EscrowHolds
                .AsNoTracking()
                .Where(hold => hold.MilestoneId == milestone.Id
                    && hold.Status
                        == SmartCourt.Features.Payments.Enums.EscrowHoldStatus.Funded)
                .Select(hold => (Guid?)hold.Id)
                .SingleOrDefaultAsync(cancellationToken);
            if (!escrowHoldId.HasValue)
            {
                continue;
            }

            await _jobScheduler.ScheduleReleaseExpiredHoldAsync(
                escrowHoldId.Value,
                now,
                cancellationToken);
            scheduled++;
        }

        return scheduled;
    }
}
