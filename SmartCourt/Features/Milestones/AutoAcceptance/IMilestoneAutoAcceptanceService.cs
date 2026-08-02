using SmartCourt.Infrastructure.Providers.Jobs;

namespace SmartCourt.Features.Milestones;

public interface IMilestoneAutoAcceptanceService
{
    Task<JobExecutionResult> AutoAcceptAsync(
        Guid milestoneId,
        Guid escrowHoldId,
        int submissionVersion,
        CancellationToken cancellationToken);
}
