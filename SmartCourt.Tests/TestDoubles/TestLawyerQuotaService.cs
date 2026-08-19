using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SmartCourt.Features.LawyerSubscription;
using SmartCourt.Features.LawyerSubscription.DTOs;
using SmartCourt.Features.LawyerSubscription.Enums;
using SmartCourt.Features.ChatAgent;
using SmartCourt.Features.LawyerSubscription.Entities;

namespace SmartCourt.Tests.TestDoubles;

public class TestLawyerQuotaService : ILawyerQuotaService
{
    public Task<QuotaReservation> ConsumeQuotaAsync(Guid lawyerId, int tokenAmount, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(new QuotaReservation { TotalReservedTokens = tokenAmount, FreeReservedTokens = tokenAmount, PaidReservedTokens = 0 });
    }

    public Task<QuotaReservation> ReserveQuotaAsync(Guid lawyerId, int estimatedMaxTokens, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(new QuotaReservation { TotalReservedTokens = estimatedMaxTokens, FreeReservedTokens = estimatedMaxTokens, PaidReservedTokens = 0 });
    }

    public Task SettleQuotaAsync(Guid lawyerId, QuotaReservation reservation, int actualTokensUsed, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    public Task<LawyerQuotaInfoResponse> GetQuotaAsync(Guid lawyerId, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(new LawyerQuotaInfoResponse(10000m, 0m, 10000m, 0m, 10000m, "Free", DateTimeOffset.UtcNow));
    }

    public Task RefundAsync(Guid lawyerId, int tokenAmount, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    public Task<LawyerQuotaTransactionListDto> GetQuotaTransactionsAsync(Guid lawyerId, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(new LawyerQuotaTransactionListDto(new List<LawyerQuotaTransactionDto>(), 0));
    }

    public Task<LawyerQuotaHistoryResponse> GetQuotaHistoryAsync(Guid lawyerId, int days, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(new LawyerQuotaHistoryResponse(new List<LawyerDailyQuotaUsageDto>()));
    }

    public Task<LawyerSubscription> ChangeSubscriptionAsync(Guid lawyerId, LawyerPlanType newPlan, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(new LawyerSubscription { LawyerId = lawyerId, PlanType = newPlan, DailyTokenLimit = 10000, StartedAt = DateTimeOffset.UtcNow });
    }

    public Task<LawyerSubscription> GetOrCreateSubscriptionAsync(Guid lawyerId, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(new LawyerSubscription { LawyerId = lawyerId, PlanType = LawyerPlanType.Free, DailyTokenLimit = 10000, StartedAt = DateTimeOffset.UtcNow });
    }
}
