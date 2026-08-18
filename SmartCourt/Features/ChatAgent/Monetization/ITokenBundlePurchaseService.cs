using SmartCourt.Features.ChatAgent.Monetization.DTOs;

namespace SmartCourt.Features.ChatAgent.Monetization;

public interface ITokenBundlePurchaseService
{
    Task<TokenBundlePurchaseResponse> PurchaseBundleAsync(
        string bundleId,
        string confirmationTokenReference,
        string? idempotencyKey,
        CancellationToken cancellationToken = default);

    Task<TokenBundlePurchaseListDto> GetPurchasesAsync(
        Guid clientId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);
}
