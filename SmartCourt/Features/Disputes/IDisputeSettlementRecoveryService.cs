using SmartCourt.Infrastructure.Providers.Jobs;

namespace SmartCourt.Features.Disputes;

public interface IDisputeSettlementRecoveryService
{
    Task<JobExecutionResult> RecoverPendingSettlementsAsync(
        CancellationToken cancellationToken);
}
