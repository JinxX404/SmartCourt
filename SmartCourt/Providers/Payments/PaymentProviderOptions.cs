namespace SmartCourt.Providers.Payments;

public sealed class PaymentProviderOptions
{
    public const string SectionName = "PaymentProvider";

    public bool UseMockProvider { get; set; }

    public string WebhookSecret { get; set; } =
        "local-mock-payment-webhook-secret";

    public string Warning { get; set; } =
        "The mock payment provider is not regulated escrow and must never be enabled silently in production.";
}
