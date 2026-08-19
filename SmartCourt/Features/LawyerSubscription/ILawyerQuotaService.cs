using SmartCourt.Features.ChatAgent;
using SmartCourt.Features.ChatAgent.Entities;
using SmartCourt.Features.LawyerSubscription.DTOs;
using SmartCourt.Features.LawyerSubscription.Entities;
using SmartCourt.Features.LawyerSubscription.Enums;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SmartCourt.Features.LawyerSubscription;

public interface ILawyerQuotaService
{
    // Quota info
    Task<LawyerQuotaInfoResponse> GetQuotaAsync(Guid lawyerId, CancellationToken cancellationToken = default);
    Task<LawyerQuotaHistoryResponse> GetQuotaHistoryAsync(Guid lawyerId, int days, CancellationToken cancellationToken = default);
    Task<LawyerQuotaTransactionListDto> GetQuotaTransactionsAsync(Guid lawyerId, int page, int pageSize, CancellationToken cancellationToken = default);

    // Reserve-Execute-Settle
    Task<QuotaReservation> ReserveQuotaAsync(Guid lawyerId, int estimatedMaxTokens, CancellationToken cancellationToken = default);
    Task<QuotaReservation> ConsumeQuotaAsync(Guid lawyerId, int exactTokens, CancellationToken cancellationToken = default);
    Task SettleQuotaAsync(Guid lawyerId, QuotaReservation reservation, int actualTokensUsed, CancellationToken cancellationToken = default);
    Task RefundAsync(Guid lawyerId, int tokenAmount, CancellationToken cancellationToken = default);

    // Subscription management
    Task<Entities.LawyerSubscription> GetOrCreateSubscriptionAsync(Guid lawyerId, CancellationToken cancellationToken = default);
    Task<Entities.LawyerSubscription> ChangeSubscriptionAsync(Guid lawyerId, LawyerPlanType newPlan, CancellationToken cancellationToken = default);
}
