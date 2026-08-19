using System;
using System.Threading;
using System.Threading.Tasks;
using SmartCourt.Features.ChatAgent;
using SmartCourt.Features.ChatAgent.DTOs;

namespace SmartCourt.Tests.TestDoubles;

public class TestQuotaService : IQuotaService
{
    public Task<QuotaReservation> ConsumeQuotaAsync(Guid clientId, int tokenAmount, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(new QuotaReservation { TotalReservedTokens = tokenAmount, FreeReservedTokens = tokenAmount, PaidReservedTokens = 0 });
    }

    public Task<QuotaInfoResponse> GetQuotaAsync(Guid clientId, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(new QuotaInfoResponse(100, 0, 100, 0, 100, DateTimeOffset.UtcNow));
    }

    public Task RefundAsync(Guid clientId, int tokenAmount, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    public Task<QuotaTransactionListDto> GetQuotaTransactionsAsync(Guid clientId, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(new QuotaTransactionListDto(new List<QuotaTransactionDto>(), 0));
    }

    public Task<QuotaHistoryResponse> GetQuotaHistoryAsync(Guid clientId, int days, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(new QuotaHistoryResponse(new List<DailyQuotaUsageDto>()));
    }

    public Task<QuotaReservation> ReserveQuotaAsync(Guid clientId, int estimatedMaxTokens, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(new QuotaReservation { TotalReservedTokens = estimatedMaxTokens, FreeReservedTokens = estimatedMaxTokens, PaidReservedTokens = 0 });
    }

    public Task SettleQuotaAsync(Guid clientId, QuotaReservation reservation, int actualTokensUsed, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    public Task<DefaultQuotaResponse> GetDefaultQuotaAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(new DefaultQuotaResponse(100));
    }
}
