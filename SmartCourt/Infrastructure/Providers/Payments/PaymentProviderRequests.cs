namespace SmartCourt.Infrastructure.Providers.Payments;

public abstract record PaymentProviderRequest(
    decimal Amount,
    string Currency,
    Guid BusinessId,
    string ProviderIdempotencyKey,
    Guid CorrelationId);

public sealed record ProviderDepositRequest(
    decimal Amount,
    string Currency,
    Guid BusinessId,
    string ProviderIdempotencyKey,
    Guid CorrelationId,
    string PaymentMethodReference)
    : PaymentProviderRequest(
        Amount,
        Currency,
        BusinessId,
        ProviderIdempotencyKey,
        CorrelationId);

public sealed record ProviderReleaseRequest(
    decimal Amount,
    string Currency,
    Guid BusinessId,
    string ProviderIdempotencyKey,
    Guid CorrelationId)
    : PaymentProviderRequest(
        Amount,
        Currency,
        BusinessId,
        ProviderIdempotencyKey,
        CorrelationId);

public sealed record ProviderRefundRequest(
    decimal Amount,
    string Currency,
    Guid BusinessId,
    string ProviderIdempotencyKey,
    Guid CorrelationId,
    string Reason)
    : PaymentProviderRequest(
        Amount,
        Currency,
        BusinessId,
        ProviderIdempotencyKey,
        CorrelationId);

public sealed record ProviderWithdrawalRequest(
    decimal Amount,
    string Currency,
    Guid BusinessId,
    string ProviderIdempotencyKey,
    Guid CorrelationId)
    : PaymentProviderRequest(
        Amount,
        Currency,
        BusinessId,
        ProviderIdempotencyKey,
        CorrelationId);
