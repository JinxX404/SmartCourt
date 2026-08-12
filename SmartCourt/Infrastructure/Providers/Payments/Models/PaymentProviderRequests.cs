namespace SmartCourt.Infrastructure.Providers.Payments;

public interface IProviderOperationRequest
{
    decimal Amount { get; }
    string Currency { get; }
    Guid BusinessId { get; }
    string ProviderIdempotencyKey { get; }
    Guid CorrelationId { get; }
}

public abstract record PaymentProviderRequest(
    decimal Amount,
    string Currency,
    Guid BusinessId,
    string ProviderIdempotencyKey,
    Guid CorrelationId) : IProviderOperationRequest;

public sealed record ProviderDepositRequest(
    decimal Amount,
    string Currency,
    Guid BusinessId,
    string ProviderIdempotencyKey,
    Guid CorrelationId,
    string PaymentMethodReference,
    string ConfirmationTokenReference = "",
    string CustomerReference = "")
    : PaymentProviderRequest(
        Amount,
        Currency,
        BusinessId,
        ProviderIdempotencyKey,
        CorrelationId);

public sealed record ProviderDepositRetryRequest(
    decimal Amount,
    string Currency,
    Guid BusinessId,
    string ProviderIdempotencyKey,
    Guid CorrelationId,
    string OriginalProviderIdempotencyKey,
    string? OriginalProviderTransactionId,
    string PaymentMethodReference = "",
    string ConfirmationTokenReference = "",
    string CustomerReference = "")
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
    Guid CorrelationId,
    string SourcePaymentProviderTransactionId = "",
    string SourceChargeProviderTransactionId = "",
    string DestinationAccountId = "",
    decimal GrossBusinessAmount = 0m)
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
    string Reason,
    string SourcePaymentProviderTransactionId = "")
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
    Guid CorrelationId,
    string DestinationReference = "",
    string ConnectedAccountId = "",
    ProviderMoney? PayoutMoney = null)
    : PaymentProviderRequest(
        Amount,
        Currency,
        BusinessId,
        ProviderIdempotencyKey,
        CorrelationId);
