using System;
using System.Threading;
using System.Threading.Tasks;
using SmartCourt.Features.Admin.LawyerSubscriptions.DTOs;
using SmartCourt.Features.LawyerSubscription.DTOs;

namespace SmartCourt.Features.Admin.LawyerSubscriptions;

public interface IAdminLawyerSubscriptionService
{
    Task<AdminLawyerSubscriptionListDto> GetLawyersSubscriptionSummaryAsync(string? search, int page, int pageSize, CancellationToken cancellationToken = default);
    
    Task<LawyerQuotaInfoResponse> GetLawyerQuotaAsync(Guid lawyerId, CancellationToken cancellationToken = default);
    
    Task<LawyerQuotaTransactionListDto> GetLawyerQuotaTransactionsAsync(Guid lawyerId, int page, int pageSize, CancellationToken cancellationToken = default);
    
    Task AdjustLawyerQuotaAsync(Guid lawyerId, AdminAdjustLawyerTokensRequest request, Guid adminId, CancellationToken cancellationToken = default);
    
    Task ChangeLawyerPlanAsync(Guid lawyerId, AdminChangeLawyerPlanRequest request, Guid adminId, CancellationToken cancellationToken = default);
}
