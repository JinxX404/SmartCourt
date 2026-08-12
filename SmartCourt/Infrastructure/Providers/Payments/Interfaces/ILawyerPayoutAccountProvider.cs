namespace SmartCourt.Infrastructure.Providers.Payments;

public interface ILawyerPayoutAccountProvider
{
    ProviderPayoutAccountSettings Settings { get; }

    Task<ProviderPayoutAccountResult> CreateAccountAsync(
        ProviderPayoutAccountCreateRequest request,
        CancellationToken cancellationToken);

    Task<ProviderPayoutAccountResult> GetAccountAsync(
        string providerAccountId,
        CancellationToken cancellationToken);

    Task<ProviderOnboardingLinkResult> CreateOnboardingLinkAsync(
        ProviderOnboardingLinkRequest request,
        CancellationToken cancellationToken);

    Task<string> CreateDashboardLinkAsync(
        string providerAccountId,
        CancellationToken cancellationToken);
}

public sealed record ProviderPayoutAccountSettings(
    string ProviderCode,
    bool SandboxOnly,
    string DefaultCountry,
    string ReturnUrl,
    string RefreshUrl);

public sealed record ProviderPayoutAccountCreateRequest(
    Guid LawyerUserId,
    string Email,
    string Country,
    string ProviderIdempotencyKey);

public sealed record ProviderPayoutAccountResult(
    string ProviderAccountId,
    string ProviderStatus,
    bool DetailsSubmitted,
    bool TransfersEnabled,
    bool PayoutsEnabled,
    bool IsLive,
    string Country,
    string DefaultCurrency,
    string? MaskedDestination);

public sealed record ProviderOnboardingLinkRequest(
    string ProviderAccountId,
    string ReturnUrl,
    string RefreshUrl,
    string ProviderIdempotencyKey);

public sealed record ProviderOnboardingLinkResult(
    string Url,
    DateTime ExpiresAt);
