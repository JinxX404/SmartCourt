using SmartCourt.Infrastructure.Providers.Jobs;

namespace SmartCourt.Features.Payments;

public interface IPaymentReconciliationService
{
    Task<JobExecutionResult> ReconcileProviderTransactionAsync(
        Guid paymentTransactionId,
        CancellationToken cancellationToken);
}
