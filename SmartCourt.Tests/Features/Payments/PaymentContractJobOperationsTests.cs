using SmartCourt.Features.Milestones;
using SmartCourt.Features.Milestones.Entities;
using SmartCourt.Features.Payments;
using SmartCourt.Features.Payments.DTOs;
using SmartCourt.Features.Payments.Entities;
using SmartCourt.Infrastructure.Providers.Jobs;
using SmartCourt.Infrastructure.Providers.Payments;
using SmartCourt.Providers.Payments;
using Xunit;

namespace SmartCourt.Tests.Features.Payments;

public sealed class PaymentContractJobOperationsTests
{
    [Fact]
    public async Task RetryProviderTransactionAsync_UsesSafeReconciliationPath()
    {
        var expected = JobExecutionResult.Completed(
            "ProviderTransactionReconciled");
        var payments = new RecordingPaymentEscrowBoundary(expected);
        var operations = new PaymentContractJobOperations(
            payments,
            new UnusedAutoAcceptanceBoundary(),
            new UnusedEscrowReleaseBoundary(),
            new WalletReconciliationService(new UnusedWalletBoundary()));
        var paymentTransactionId = Guid.NewGuid();

        var result = await operations.RetryProviderTransactionAsync(
            paymentTransactionId,
            CancellationToken.None);

        Assert.Same(expected, result);
        Assert.Equal(paymentTransactionId, payments.ReconciledTransactionId);
    }

    private sealed class RecordingPaymentEscrowBoundary(
        JobExecutionResult result) : IPaymentEscrowService, IPaymentReconciliationService
    {
        public Guid? ReconciledTransactionId { get; private set; }

        public Task<JobExecutionResult> ReconcileProviderTransactionAsync(
            Guid paymentTransactionId,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ReconciledTransactionId = paymentTransactionId;
            return Task.FromResult(result);
        }

        public Task<PaymentDto> FundAsync(
            Guid milestoneId,
            FundMilestoneRequest request,
            string? idempotencyKey,
            CancellationToken cancellationToken)
            => throw new NotSupportedException();

        public Task<PaymentHistoryDto> GetContractPaymentsAsync(
            Guid contractId,
            CancellationToken cancellationToken)
            => throw new NotSupportedException();

        public Task<PaymentDto> GetMilestonePaymentAsync(
            Guid milestoneId,
            CancellationToken cancellationToken)
            => throw new NotSupportedException();

        public Task<PaymentDto> RetryAsync(
            Guid paymentTransactionId,
            string? idempotencyKey,
            CancellationToken cancellationToken)
            => throw new NotSupportedException();

        public Task<PaymentDto> CompleteFundingAsync(
            Milestone milestone,
            Guid lawyerUserId,
            PaymentTransaction paymentTransaction,
            ProviderResult providerResult,
            Guid? reservationId,
            Guid? actorUserId,
            Guid correlationId,
            CancellationToken cancellationToken)
            => throw new NotSupportedException();

        public Task<PaymentActionResultDto> FinalizeFailedExternalResultAsync(
            Milestone milestone,
            PaymentTransaction paymentTransaction,
            string? providerTransactionId,
            Guid? reservationId,
            Guid correlationId,
            CancellationToken cancellationToken)
            => throw new NotSupportedException();

        public Task<Guid?> FindProcessingFundingReservationIdAsync(
            Guid milestoneId,
            CancellationToken cancellationToken)
            => Task.FromResult<Guid?>(null);

        public Task<PaymentActionResultDto> HandleWebhookAsync(
            PaymentWebhookRequest request,
            string? eventIdHeader,
            string? timestampHeader,
            string? signatureHeader,
            string rawBody,
            CancellationToken cancellationToken)
            => throw new NotSupportedException();
    }

    private sealed class UnusedAutoAcceptanceBoundary
        : IMilestoneAutoAcceptanceService
    {
        public Task<JobExecutionResult> AutoAcceptAsync(
            Guid milestoneId,
            Guid escrowHoldId,
            int submissionVersion,
            CancellationToken cancellationToken)
            => throw new NotSupportedException();
    }

    private sealed class UnusedEscrowReleaseBoundary
        : IEscrowReleaseService
    {
        public Task<JobExecutionResult> ReleaseExpiredHoldAsync(
            Guid escrowHoldId,
            CancellationToken cancellationToken)
            => throw new NotSupportedException();
    }

    private sealed class UnusedWalletBoundary : IWalletService
    {
        public Task<WalletDto> GetAsync(
            CancellationToken cancellationToken)
            => throw new NotSupportedException();

        public Task<PaymentActionResultDto> WithdrawAsync(
            CreateWithdrawalRequest request,
            string? idempotencyKey,
            CancellationToken cancellationToken)
            => throw new NotSupportedException();

        public Task<JobExecutionResult> ReconcilePendingWithdrawalsAsync(
            CancellationToken cancellationToken)
            => throw new NotSupportedException();
    }
}
