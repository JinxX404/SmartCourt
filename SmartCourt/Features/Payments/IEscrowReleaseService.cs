using SmartCourt.Infrastructure.Providers.Jobs;

namespace SmartCourt.Features.Payments;

public interface IEscrowReleaseService
{
    Task<JobExecutionResult> ReleaseExpiredHoldAsync(
        Guid escrowHoldId,
        CancellationToken cancellationToken);
}
