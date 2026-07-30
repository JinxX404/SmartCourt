using SmartCourt.Common.Exceptions;
using SmartCourt.Features.Milestones;
using SmartCourt.Infrastructure.Providers.Jobs;

namespace SmartCourt.Features.Payments;

public sealed class PaymentContractJobOperations(
    IPaymentEscrowService paymentEscrowService,
    IMilestoneAutoAcceptanceService milestoneAutoAcceptanceService,
    IEscrowReleaseService escrowReleaseService)
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

    public async Task<JobExecutionResult> AutoAcceptMilestoneAsync(
        Guid milestoneId,
        Guid escrowHoldId,
        int submissionVersion,
        CancellationToken cancellationToken)
    {
        return await milestoneAutoAcceptanceService.AutoAcceptAsync(
            milestoneId,
            escrowHoldId,
            submissionVersion,
            cancellationToken);
    }

    public async Task<JobExecutionResult> ReleaseExpiredHoldAsync(
        Guid escrowHoldId,
        CancellationToken cancellationToken)
    {
        return await escrowReleaseService.ReleaseExpiredHoldAsync(
            escrowHoldId,
            cancellationToken);
    }

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
