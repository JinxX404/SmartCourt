namespace SmartCourt.Infrastructure.Providers.Payments;

public interface IPaymentReconciliationProvider
{
    Task<ProviderResult?> GetDepositStatusAsync(
        ProviderDepositStatusRequest request,
        CancellationToken cancellationToken);

    Task<ProviderResult?> GetReleaseStatusAsync(
        ProviderReleaseStatusRequest request,
        CancellationToken cancellationToken)
        => Task.FromResult<ProviderResult?>(null);

    Task<ProviderResult?> GetRefundStatusAsync(
        ProviderRefundStatusRequest request,
        CancellationToken cancellationToken)
        => Task.FromResult<ProviderResult?>(null);

    Task<ProviderResult?> GetWithdrawalStatusAsync(
        ProviderWithdrawalStatusRequest request,
        CancellationToken cancellationToken)
        => Task.FromResult<ProviderResult?>(null);
}

public sealed record ProviderDepositStatusRequest(
    decimal Amount,
    string Currency,
    Guid BusinessId,
    string ProviderIdempotencyKey,
    Guid CorrelationId);

public sealed record ProviderReleaseStatusRequest(
    decimal Amount,
    string Currency,
    Guid BusinessId,
    string ProviderIdempotencyKey,
    Guid CorrelationId);

public sealed record ProviderRefundStatusRequest(
    decimal Amount,
    string Currency,
    Guid BusinessId,
    string ProviderIdempotencyKey,
    Guid CorrelationId);

public sealed record ProviderWithdrawalStatusRequest(
    decimal Amount,
    string Currency,
    Guid BusinessId,
    string ProviderIdempotencyKey,
    Guid CorrelationId);
