using SmartCourt.Infrastructure.Providers.Jobs;

namespace SmartCourt.Features.Milestones;

public interface IMilestoneSchedulingReconciliationService
{
    Task<JobExecutionResult> ReconcileAsync(
        CancellationToken cancellationToken);
}
