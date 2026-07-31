using SmartCourt.Infrastructure.Providers.Jobs;

namespace SmartCourt.Features.Payments;

public sealed class PaymentReconciliationService(
    IPaymentEscrowService paymentEscrowService) : IPaymentReconciliationService
{
    public async Task<JobExecutionResult> ReconcileProviderTransactionAsync(
        Guid paymentTransactionId,
        CancellationToken cancellationToken)
    {
        return await paymentEscrowService.ReconcileProviderTransactionAsync(
            paymentTransactionId,
            cancellationToken);
    }
}
