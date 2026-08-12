using Microsoft.Extensions.Options;
using SmartCourt.Common.Exceptions;
using SmartCourt.Infrastructure.Providers.Payments;

namespace SmartCourt.Providers.Payments.Stripe;

public sealed class StripeWebhookVerifier(
    IOptions<StripeOptions> options,
    global::Stripe.StripeClient stripeClient) : IPaymentProviderWebhookVerifier
{
    private readonly StripeOptions _options = options.Value;

    public ProviderWebhookEvent Verify(
        string rawBody,
        string signature,
        ProviderWebhookScope scope)
    {
        if (string.IsNullOrWhiteSpace(rawBody)
            || string.IsNullOrWhiteSpace(signature))
        {
            throw new BusinessException(
                "Stripe webhook body and Stripe-Signature are required.");
        }

        var secret = scope == ProviderWebhookScope.Platform
            ? _options.PlatformWebhookSecret
            : _options.ConnectWebhookSecret;
        try
        {
            if (scope == ProviderWebhookScope.ConnectedAccounts
                && rawBody.Contains("\"object\":\"v2.core.event\"", StringComparison.Ordinal))
            {
                var notification = stripeClient.ParseEventNotification(
                    rawBody,
                    signature,
                    secret,
                    _options.WebhookToleranceSeconds);
                var relatedObjectId = ExtractRelatedObjectId(rawBody);
                return new ProviderWebhookEvent(
                    notification.Id,
                    notification.Type,
                    relatedObjectId,
                    relatedObjectId,
                    notification.Livemode);
            }

            var stripeEvent = global::Stripe.EventUtility.ConstructEvent(
                rawBody,
                signature,
                secret,
                _options.WebhookToleranceSeconds,
                throwOnApiVersionMismatch: false);
            var objectId = (stripeEvent.Data.Object
                as global::Stripe.IHasId)?.Id;
            return new ProviderWebhookEvent(
                stripeEvent.Id,
                stripeEvent.Type,
                objectId,
                stripeEvent.Account,
                stripeEvent.Livemode);
        }
        catch (global::Stripe.StripeException exception)
        {
            throw new BusinessException(
                "Stripe webhook signature is invalid or expired.",
                exception);
        }
    }

    private static string? ExtractRelatedObjectId(string rawBody)
    {
        using var document = System.Text.Json.JsonDocument.Parse(rawBody);
        return document.RootElement.TryGetProperty(
                "related_object",
                out var related)
            && related.TryGetProperty("id", out var id)
                ? id.GetString()
                : null;
    }
}
