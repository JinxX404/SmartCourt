using SmartCourt.Features.Payments.DTOs;
using SmartCourt.Infrastructure.Providers.Jobs;

namespace SmartCourt.Features.Payments;

public interface IPaymentEscrowService
{
    Task<PaymentDto> FundAsync(
        Guid milestoneId,
        FundMilestoneRequest request,
        string? idempotencyKey,
        CancellationToken cancellationToken);

    Task<PaymentHistoryDto> GetContractPaymentsAsync(
        Guid contractId,
        CancellationToken cancellationToken);

    Task<PaymentDto> GetMilestonePaymentAsync(
        Guid milestoneId,
        CancellationToken cancellationToken);

    Task<PaymentDto> RetryAsync(
        Guid paymentTransactionId,
        string? idempotencyKey,
        CancellationToken cancellationToken);

    Task<PaymentActionResultDto> HandleWebhookAsync(
        PaymentWebhookRequest request,
        string? eventIdHeader,
        string? timestampHeader,
        string? signatureHeader,
        string rawBody,
        CancellationToken cancellationToken);

    Task<JobExecutionResult> ReconcileProviderTransactionAsync(
        Guid paymentTransactionId,
        CancellationToken cancellationToken);
}
