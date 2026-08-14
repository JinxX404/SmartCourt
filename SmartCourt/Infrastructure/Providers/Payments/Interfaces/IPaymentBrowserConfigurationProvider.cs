namespace SmartCourt.Infrastructure.Providers.Payments;

public interface IPaymentBrowserConfigurationProvider
{
    ProviderBrowserConfiguration BrowserConfiguration { get; }
}

public sealed record ProviderBrowserConfiguration(
    string ProviderCode,
    string PublishableKey,
    string Currency,
    bool SandboxOnly,
    bool IsTestEnvironment,
    bool ConfirmationTokensEnabled,
    bool SavedPaymentMethodsEnabled);
