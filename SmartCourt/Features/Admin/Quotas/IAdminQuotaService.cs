using SmartCourt.Features.Admin.Quotas.DTOs;

namespace SmartCourt.Features.Admin.Quotas;

public interface IAdminQuotaService
{
    Task SetGlobalDailyLimitAsync(UpdateDailyLimitRequest request, CancellationToken cancellationToken = default);
    Task SetClientDailyLimitAsync(Guid clientId, UpdateDailyLimitRequest request, CancellationToken cancellationToken = default);
    Task AdjustClientQuotaAsync(Guid clientId, AdjustQuotaRequest request, Guid adminId, CancellationToken cancellationToken = default);
    Task<SmartCourt.Features.ChatAgent.DTOs.QuotaInfoResponse> GetClientQuotaAsync(Guid clientId, CancellationToken cancellationToken = default);
    Task<SmartCourt.Features.ChatAgent.DTOs.QuotaTransactionListDto> GetClientQuotaTransactionsAsync(Guid clientId, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<SmartCourt.Features.ChatAgent.Monetization.DTOs.TokenBundlePurchaseListDto> GetPurchasesAsync(int page, int pageSize, CancellationToken cancellationToken = default);
    Task<GlobalDailyLimitResponse> GetGlobalDailyLimitAsync(CancellationToken cancellationToken = default);
    Task<AdminQuotaClientSummaryListDto> GetClientsQuotaSummaryAsync(string? search, bool? isExhausted, bool? hasAdditionalBalance, int page, int pageSize, CancellationToken cancellationToken = default);
}
