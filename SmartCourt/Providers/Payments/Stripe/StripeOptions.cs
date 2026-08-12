namespace SmartCourt.Providers.Payments.Stripe;

public sealed class StripeOptions
{
    public const string SectionName = "Stripe";
    public const string ProviderCode = "StripeConnect";

    public string SecretKey { get; set; } = string.Empty;
    public string PublishableKey { get; set; } = string.Empty;
    public string PlatformWebhookSecret { get; set; } = string.Empty;
    public string ConnectWebhookSecret { get; set; } = string.Empty;
    public int WebhookToleranceSeconds { get; set; } = 300;
    public int MaxNetworkRetries { get; set; } = 2;
    public string ConnectReturnUrl { get; set; } =
        "http://localhost:5173/wallet/payout-account/return";
    public string ConnectRefreshUrl { get; set; } =
        "http://localhost:5173/wallet/payout-account/refresh";
    public string DefaultConnectedAccountCountry { get; set; } = "US";
    public bool SandboxOnly { get; set; } = true;
}
