using SmartCourt.Infrastructure.Providers.Jobs;

namespace SmartCourt.Features.Contracts;

public interface IContractTerminationRecoveryService
{
    Task<JobExecutionResult> RecoverPendingTerminationsAsync(
        CancellationToken cancellationToken);
}
