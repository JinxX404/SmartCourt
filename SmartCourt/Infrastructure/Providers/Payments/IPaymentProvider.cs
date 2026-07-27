namespace SmartCourt.Infrastructure.Providers.Payments;

public interface IPaymentProvider
{
    Task<ProviderResult> DepositAsync(
        ProviderDepositRequest request,
        CancellationToken cancellationToken);

    Task<ProviderResult> ReleaseAsync(
        ProviderReleaseRequest request,
        CancellationToken cancellationToken);

    Task<ProviderResult> RefundAsync(
        ProviderRefundRequest request,
        CancellationToken cancellationToken);

    Task<ProviderResult> WithdrawAsync(
        ProviderWithdrawalRequest request,
        CancellationToken cancellationToken);
}
