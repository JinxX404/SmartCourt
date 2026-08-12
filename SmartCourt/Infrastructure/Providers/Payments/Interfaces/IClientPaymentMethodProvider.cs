namespace SmartCourt.Infrastructure.Providers.Payments;

public interface IClientPaymentMethodProvider
{
    Task<ProviderCustomerResult> CreateCustomerAsync(
        ProviderCustomerCreateRequest request,
        CancellationToken cancellationToken);

    Task<ProviderSetupIntentResult> CreateSetupIntentAsync(
        ProviderSetupIntentRequest request,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<ProviderPaymentMethodResult>> ListPaymentMethodsAsync(
        string providerCustomerId,
        CancellationToken cancellationToken);

    Task SetDefaultPaymentMethodAsync(
        string providerCustomerId,
        string paymentMethodId,
        CancellationToken cancellationToken);

    Task RemovePaymentMethodAsync(
        string providerCustomerId,
        string paymentMethodId,
        CancellationToken cancellationToken);
}

public sealed record ProviderCustomerCreateRequest(
    Guid ClientUserId,
    string Email,
    string Name,
    string ProviderIdempotencyKey);

public sealed record ProviderCustomerResult(
    string ProviderCustomerId,
    bool IsLive);

public sealed record ProviderSetupIntentRequest(
    Guid ClientUserId,
    string ProviderCustomerId,
    string ProviderIdempotencyKey);

public sealed record ProviderSetupIntentResult(
    string ProviderSetupIntentId,
    string ClientSecret,
    string Status,
    bool IsLive);

public sealed record ProviderPaymentMethodResult(
    string ProviderPaymentMethodId,
    string Type,
    string? Brand,
    string? Last4,
    long? ExpiryMonth,
    long? ExpiryYear,
    string? HolderName,
    bool IsDefault);
