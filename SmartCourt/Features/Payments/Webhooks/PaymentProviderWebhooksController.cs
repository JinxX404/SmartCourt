using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using SmartCourt.Common.Exceptions;
using SmartCourt.Common.Models;
using SmartCourt.Common.RateLimiting;
using SmartCourt.Features.Payments.DTOs;
using SmartCourt.Infrastructure.Providers.Payments;
using SmartCourt.Providers.Payments;

namespace SmartCourt.Features.Payments;

[ApiController]
[Route("api/payment-providers/stripe/webhooks")]
[AllowAnonymous]
public sealed class PaymentProviderWebhooksController(
    IPaymentProviderWebhookVerifier webhookVerifier,
    PaymentProviderWebhookService webhookService,
    IOptions<PaymentProviderOptions> paymentProviderOptions)
    : ControllerBase
{
    [HttpPost("platform")]
    [SecurityRateLimit(RateLimitPolicyNames.PaymentWebhook)]
    public Task<ActionResult<ApiResponse<PaymentActionResultDto>>> PlatformAsync(
        CancellationToken cancellationToken)
        => HandleAsync(ProviderWebhookScope.Platform, cancellationToken);

    [HttpPost("connect")]
    [SecurityRateLimit(RateLimitPolicyNames.PaymentWebhook)]
    public Task<ActionResult<ApiResponse<PaymentActionResultDto>>> ConnectAsync(
        CancellationToken cancellationToken)
        => HandleAsync(
            ProviderWebhookScope.ConnectedAccounts,
            cancellationToken);

    private async Task<ActionResult<ApiResponse<PaymentActionResultDto>>>
        HandleAsync(
            ProviderWebhookScope scope,
            CancellationToken cancellationToken)
    {
        var signature = Request.Headers["Provider-Signature"].ToString();
        if (string.IsNullOrWhiteSpace(signature))
        {
            signature = Request.Headers["Stripe-Signature"].ToString();
        }
        var rawBody = await ReadBoundedBodyAsync(
            paymentProviderOptions.Value.WebhookMaximumBodySizeBytes,
            cancellationToken);
        var providerEvent = webhookVerifier.Verify(
            rawBody,
            signature,
            scope);
        var result = await webhookService.HandleAsync(
            providerEvent,
            cancellationToken);
        return Ok(ApiResponse<PaymentActionResultDto>.Ok(result));
    }

    private async Task<string> ReadBoundedBodyAsync(
        int maximumBytes,
        CancellationToken cancellationToken)
    {
        if (Request.ContentLength > maximumBytes)
        {
            throw new PayloadTooLargeException(
                "Payment-provider webhook payload is too large.");
        }

        using var body = new MemoryStream();
        var buffer = new byte[Math.Min(maximumBytes + 1, 16_384)];
        while (true)
        {
            var count = await Request.Body.ReadAsync(
                buffer,
                cancellationToken);
            if (count == 0)
            {
                break;
            }

            if (body.Length + count > maximumBytes)
            {
                throw new PayloadTooLargeException(
                    "Payment-provider webhook payload is too large.");
            }

            await body.WriteAsync(
                buffer.AsMemory(0, count),
                cancellationToken);
        }

        return Encoding.UTF8.GetString(
            body.GetBuffer(),
            0,
            checked((int)body.Length));
    }
}
