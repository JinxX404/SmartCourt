using SmartCourt.Features.Payments.DTOs;

namespace SmartCourt.Features.Payments;

public sealed class PaymentWebhookService(
    IPaymentEscrowService paymentEscrowService) : IPaymentWebhookService
{
    public async Task<PaymentActionResultDto> HandleWebhookAsync(
        PaymentWebhookRequest request,
        string? eventIdHeader,
        string? timestampHeader,
        string? signatureHeader,
        string rawBody,
        CancellationToken cancellationToken)
    {
        return await paymentEscrowService.HandleWebhookAsync(
            request,
            eventIdHeader,
            timestampHeader,
            signatureHeader,
            rawBody,
            cancellationToken);
    }
}
