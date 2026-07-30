namespace SmartCourt.Infrastructure.Providers.Payments;

public interface IPaymentReconciliationProvider
{
    Task<ProviderResult?> GetDepositStatusAsync(
        ProviderDepositStatusRequest request,
        CancellationToken cancellationToken);
}

public sealed record ProviderDepositStatusRequest(
    decimal Amount,
    string Currency,
    Guid BusinessId,
    string ProviderIdempotencyKey,
    Guid CorrelationId);
