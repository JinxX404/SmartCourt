using SmartCourt.Features.ChatAgent.DTOs;

namespace SmartCourt.Features.ChatAgent;

public interface IQuotaService
{
    Task<QuotaInfoResponse> GetQuotaAsync(Guid clientId, CancellationToken cancellationToken = default);
    Task<QuotaHistoryResponse> GetQuotaHistoryAsync(Guid clientId, int days, CancellationToken cancellationToken = default);
    Task<QuotaTransactionListDto> GetQuotaTransactionsAsync(Guid clientId, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<QuotaReservation> ConsumeQuotaAsync(Guid clientId, int tokenAmount, CancellationToken cancellationToken = default);
    Task RefundAsync(Guid clientId, int tokenAmount, CancellationToken cancellationToken = default);
    
    // New Reserve-Execute-Settle pattern
    Task<QuotaReservation> ReserveQuotaAsync(Guid clientId, int estimatedMaxTokens, CancellationToken cancellationToken = default);
    Task SettleQuotaAsync(Guid clientId, QuotaReservation reservation, int actualTokensUsed, CancellationToken cancellationToken = default);

    Task<DefaultQuotaResponse> GetDefaultQuotaAsync(CancellationToken cancellationToken = default);
}
