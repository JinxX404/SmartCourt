namespace SmartCourt.Providers.Payments;

public sealed class PaymobOptions
{
    public const string SectionName = "Paymob";

    public const string ProviderCode = "PaymobMarketPlace";

    public string BaseUrl { get; set; } = "https://api.paymob.com/v1";

    public string ApiKey { get; set; } = "";

    public string WebhookSecret { get; set; } = "";

    public int TimeoutSeconds { get; set; } = 15;

    public string PaymentsPath { get; set; } = "/payments";

    public string RetryPath { get; set; } = "/payments/retry";

    public string ReleasesPath { get; set; } = "/payouts/releases";

    public string RefundsPath { get; set; } = "/refunds";

    public string WithdrawalsPath { get; set; } = "/payouts/withdrawals";

    public string StatusPath { get; set; } = "/operations";

    public string Warning { get; set; } =
        "Paymob MarketPlace is not regulated escrow and must never be assumed to hold funds without a signed payout schedule.";
}