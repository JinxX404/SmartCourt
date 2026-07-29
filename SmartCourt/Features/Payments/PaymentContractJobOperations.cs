using SmartCourt.Common.Exceptions;
using SmartCourt.Infrastructure.Providers.Jobs;

namespace SmartCourt.Features.Payments;

public sealed class PaymentContractJobOperations(
    IPaymentEscrowService paymentEscrowService)
    : IContractJobOperations
{
    public async Task<JobExecutionResult> ReconcileProviderTransactionAsync(
        Guid paymentTransactionId,
        CancellationToken cancellationToken)
    {
        return await paymentEscrowService
            .ReconcileProviderTransactionAsync(
                paymentTransactionId,
                cancellationToken);
    }

    public Task<JobExecutionResult> AutoAcceptMilestoneAsync(
        Guid milestoneId,
        Guid escrowHoldId,
        int submissionVersion,
        CancellationToken cancellationToken)
        => throw NotAvailable();

    public Task<JobExecutionResult> ReleaseExpiredHoldAsync(
        Guid escrowHoldId,
        CancellationToken cancellationToken)
        => throw NotAvailable();

    public Task<JobExecutionResult> RetryProviderTransactionAsync(
        Guid paymentTransactionId,
        CancellationToken cancellationToken)
        => throw NotAvailable();

    public Task<JobExecutionResult> ReconcilePendingWalletProjectionsAsync(
        CancellationToken cancellationToken)
        => throw NotAvailable();

    private static BusinessException NotAvailable()
        => new(
            "عملية الخلفية المطلوبة لم تُنفذ بعد ضمن المرحلة الحالية.");
}
