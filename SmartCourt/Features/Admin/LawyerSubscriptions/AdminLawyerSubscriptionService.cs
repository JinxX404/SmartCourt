using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SmartCourt.Common.Configuration;
using SmartCourt.Common.Domain;
using SmartCourt.Common.Exceptions;
using SmartCourt.Features.Admin.LawyerSubscriptions.DTOs;
using SmartCourt.Features.LawyerSubscription;
using SmartCourt.Features.LawyerSubscription.DTOs;
using SmartCourt.Features.LawyerSubscription.Entities;
using SmartCourt.Features.LawyerSubscription.Enums;
using SmartCourt.Persistence;

namespace SmartCourt.Features.Admin.LawyerSubscriptions;

internal sealed class AdminLawyerSubscriptionService : IAdminLawyerSubscriptionService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ILawyerQuotaService _lawyerQuotaService;
    private readonly TimeProvider _timeProvider;
    private readonly LawyerPlanOptions _planOptions;

    public AdminLawyerSubscriptionService(
        ApplicationDbContext dbContext,
        ILawyerQuotaService lawyerQuotaService,
        TimeProvider timeProvider,
        Microsoft.Extensions.Options.IOptions<SmartCourt.Common.Configuration.LawyerPlanOptions> planOptions)
    {
        _dbContext = dbContext;
        _lawyerQuotaService = lawyerQuotaService;
        _timeProvider = timeProvider;
        _planOptions = planOptions.Value;
    }

    public async Task<AdminLawyerSubscriptionListDto> GetLawyersSubscriptionSummaryAsync(string? search, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Users
            .Where(u => _dbContext.UserRoles.Any(ur => ur.UserId == u.Id && _dbContext.Roles.Any(r => r.Id == ur.RoleId && r.Name == "Lawyer")));

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(u => u.FullName.Contains(search) || (u.Email != null && u.Email.Contains(search)));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var lawyers = await query
            .OrderBy(u => u.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(u => new
            {
                u.Id,
                u.FullName,
                u.Email,
                Subscription = _dbContext.LawyerSubscriptions.FirstOrDefault(s => s.LawyerId == u.Id),
                DailyUsage = _dbContext.LawyerDailyUsages.FirstOrDefault(d => d.LawyerId == u.Id),
                Ledger = _dbContext.LawyerQuotaLedgers.FirstOrDefault(l => l.LawyerId == u.Id)
            })
            .ToListAsync(cancellationToken);

        var items = lawyers.Select(l => {
            var planType = l.Subscription?.PlanType ?? LawyerPlanType.Free;
            var planTypeStr = planType.ToString();
            var planDef = _planOptions.Plans.FirstOrDefault(p => p.PlanType == planTypeStr) 
                          ?? _planOptions.Plans.First(p => p.PlanType == LawyerPlanType.Free.ToString());

            return new AdminLawyerSubscriptionSummaryDto(
                LawyerId: l.Id,
                FirstName: l.FullName,
                LastName: "",
                Email: l.Email ?? string.Empty,
                PlanName: planType.ToString(),
                DailyCreditLimit: CreditConverter.ToCredits(planDef.DailyTokenLimit),
                PurchasedCreditBalance: l.Ledger != null ? CreditConverter.ToCredits(l.Ledger.PurchasedTokenBalance) : 0,
                CreatedAt: DateTimeOffset.MinValue
            );
        }).ToList();

        return new AdminLawyerSubscriptionListDto(items, totalCount, page, pageSize);
    }

    public async Task<LawyerQuotaInfoResponse> GetLawyerQuotaAsync(Guid lawyerId, CancellationToken cancellationToken = default)
    {
        await EnsureIsLawyerAsync(lawyerId, cancellationToken);
        return await _lawyerQuotaService.GetQuotaAsync(lawyerId, cancellationToken);
    }

    public async Task<LawyerQuotaTransactionListDto> GetLawyerQuotaTransactionsAsync(Guid lawyerId, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        await EnsureIsLawyerAsync(lawyerId, cancellationToken);
        return await _lawyerQuotaService.GetQuotaTransactionsAsync(lawyerId, page, pageSize, cancellationToken);
    }

    public async Task AdjustLawyerQuotaAsync(Guid lawyerId, AdminAdjustLawyerTokensRequest request, Guid adminId, CancellationToken cancellationToken = default)
    {
        await EnsureIsLawyerAsync(lawyerId, cancellationToken);

        var ledger = await _dbContext.LawyerQuotaLedgers
            .FirstOrDefaultAsync(l => l.LawyerId == lawyerId, cancellationToken);

        if (ledger == null)
        {
            ledger = new LawyerQuotaLedger { LawyerId = lawyerId, PurchasedTokenBalance = 0 };
            _dbContext.LawyerQuotaLedgers.Add(ledger);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        int tokensToAdjust = CreditConverter.ToTokens(request.CreditAmount);

        var rowsAffected = await _dbContext.LawyerQuotaLedgers
            .Where(x => x.LawyerId == lawyerId && (tokensToAdjust >= 0 || x.PurchasedTokenBalance >= -tokensToAdjust))
            .ExecuteUpdateAsync(s => s.SetProperty(x => x.PurchasedTokenBalance, x => x.PurchasedTokenBalance + tokensToAdjust), cancellationToken);

        if (rowsAffected == 0)
        {
            throw new BusinessException("فشل تعديل الرصيد. قد لا يوجد رصيد كافٍ لخصمه.");
        }

        var transactionTypeStr = tokensToAdjust >= 0 ? "AdminAdjustmentCredit" : "AdminAdjustmentDebit";

        var transaction = new LawyerQuotaTransaction
        {
            Id = Guid.NewGuid(),
            LawyerId = lawyerId,
            Amount = Math.Abs(tokensToAdjust),
            Reason = $"Admin Adjustment: {request.Reason} (AdminId: {adminId}) - {transactionTypeStr}",
            CreatedAt = _timeProvider.GetUtcNow()
        };

        _dbContext.LawyerQuotaTransactions.Add(transaction);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task ChangeLawyerPlanAsync(Guid lawyerId, AdminChangeLawyerPlanRequest request, Guid adminId, CancellationToken cancellationToken = default)
    {
        await EnsureIsLawyerAsync(lawyerId, cancellationToken);

        if (!Enum.TryParse<LawyerPlanType>(request.PlanType, true, out var planType))
        {
            throw new BusinessException("نوع الخطة غير صحيح.");
        }

        await _lawyerQuotaService.ChangeSubscriptionAsync(lawyerId, planType, cancellationToken);
        
        // Log manual plan change?
        var transaction = new LawyerQuotaTransaction
        {
            Id = Guid.NewGuid(),
            LawyerId = lawyerId,
            Amount = 0,
            Reason = $"Admin changed plan to {planType}. Reason: {request.Reason} (AdminId: {adminId})",
            CreatedAt = _timeProvider.GetUtcNow()
        };
        
        _dbContext.LawyerQuotaTransactions.Add(transaction);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task EnsureIsLawyerAsync(Guid userId, CancellationToken cancellationToken)
    {
        var isLawyer = await _dbContext.Users
            .Where(u => u.Id == userId)
            .Select(u => _dbContext.UserRoles.Any(ur => ur.UserId == u.Id && _dbContext.Roles.Any(r => r.Id == ur.RoleId && r.Name == "Lawyer")))
            .FirstOrDefaultAsync(cancellationToken);

        if (!isLawyer)
        {
            throw new BusinessException("هذا المستخدم ليس محامياً. لا يمكنك استخدام هذه الواجهة إلا للمحامين.");
        }
    }
}
