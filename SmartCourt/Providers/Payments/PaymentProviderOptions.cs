namespace SmartCourt.Providers.Payments;

public sealed class PaymentProviderOptions
{
    public const string SectionName = "PaymentProvider";

    public bool UseMockProvider { get; set; }

    public string ProviderCode { get; set; } = "MockPaymentProvider";

    public string WebhookSecret { get; set; } =
        "local-mock-payment-webhook-secret";

    public int WebhookMaximumBodySizeBytes { get; set; } = 65_536;

    public string[] WebhookAllowedIpRanges { get; set; } = [];

    public int ProcessingSlaMinutes { get; set; } = 1_440;

    public string Warning { get; set; } =
        "The mock payment provider is not regulated escrow and must never be enabled silently in production.";
}
