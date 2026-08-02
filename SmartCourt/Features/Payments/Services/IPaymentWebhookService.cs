using SmartCourt.Features.Payments.DTOs;

namespace SmartCourt.Features.Payments;

public interface IPaymentWebhookService
{
    Task<PaymentActionResultDto> HandleWebhookAsync(
        PaymentWebhookRequest request,
        string? eventIdHeader,
        string? timestampHeader,
        string? signatureHeader,
        string rawBody,
        CancellationToken cancellationToken);
}
