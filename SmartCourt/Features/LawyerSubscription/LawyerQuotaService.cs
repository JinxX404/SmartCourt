using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SmartCourt.Common.Configuration;
using SmartCourt.Common.Domain;
using SmartCourt.Common.Exceptions;
using SmartCourt.Features.ChatAgent;
using SmartCourt.Features.ChatAgent.Entities;
using SmartCourt.Features.LawyerSubscription.DTOs;
using SmartCourt.Features.LawyerSubscription.Entities;
using SmartCourt.Features.LawyerSubscription.Enums;
using SmartCourt.Persistence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SmartCourt.Features.LawyerSubscription;

internal sealed class LawyerQuotaService : ILawyerQuotaService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly TimeProvider _timeProvider;
    private readonly ILogger<LawyerQuotaService> _logger;
    private readonly QuotaOptions _quotaOptions;
    private readonly LawyerPlanOptions _planOptions;

    public LawyerQuotaService(
        ApplicationDbContext dbContext,
        TimeProvider timeProvider,
        ILogger<LawyerQuotaService> logger,
        IOptions<QuotaOptions> quotaOptions,
        IOptions<LawyerPlanOptions> planOptions)
    {
        _dbContext = dbContext;
        _timeProvider = timeProvider;
        _logger = logger;
        _quotaOptions = quotaOptions.Value;
        _planOptions = planOptions.Value;
    }

    private DateTimeOffset GetToday()
    {
        try
        {
            var tz = TimeZoneInfo.FindSystemTimeZoneById(_quotaOptions.Timezone);
            var dateInTz = TimeZoneInfo.ConvertTimeFromUtc(_timeProvider.GetUtcNow().UtcDateTime, tz);
            var midnight = dateInTz.Date;
            return new DateTimeOffset(midnight, tz.GetUtcOffset(midnight));
        }
        catch (Exception ex) when (ex is TimeZoneNotFoundException or InvalidTimeZoneException)
        {
            _logger.LogWarning(ex, "Invalid timezone '{Timezone}' configured for quota reset. Falling back to UTC.", _quotaOptions.Timezone);
            var utcNow = _timeProvider.GetUtcNow();
            var midnight = utcNow.UtcDateTime.Date;
            return new DateTimeOffset(midnight, TimeSpan.Zero);
        }
    }

    private DateTimeOffset GetNextResetTime()
    {
        var today = GetToday();
        return today.AddDays(1);
    }

    public async Task<Entities.LawyerSubscription> GetOrCreateSubscriptionAsync(Guid lawyerId, CancellationToken cancellationToken = default)
    {
        var now = _timeProvider.GetUtcNow();
        var subscription = await _dbContext.LawyerSubscriptions
            .FirstOrDefaultAsync(s => s.LawyerId == lawyerId, cancellationToken);

        if (subscription == null)
        {
            var freePlanDef = _planOptions.Plans.FirstOrDefault(p => p.PlanType.Equals(LawyerPlanType.Free.ToString(), StringComparison.OrdinalIgnoreCase));
            if (freePlanDef == null)
                throw new InvalidOperationException("Free plan definition is missing from configuration.");

            subscription = new Entities.LawyerSubscription
            {
                LawyerId = lawyerId,
                PlanType = LawyerPlanType.Free,
                DailyTokenLimit = freePlanDef.DailyTokenLimit,
                StartedAt = now,
                ExpiresAt = null,
                IsActive = true
            };

            _dbContext.LawyerSubscriptions.Add(subscription);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return subscription;
        }

        // Lazy expiry check
        if (subscription.ExpiresAt.HasValue && subscription.ExpiresAt.Value <= now)
        {
            var freePlanDef = _planOptions.Plans.FirstOrDefault(p => p.PlanType.Equals(LawyerPlanType.Free.ToString(), StringComparison.OrdinalIgnoreCase));
            if (freePlanDef != null && subscription.PlanType != LawyerPlanType.Free)
            {
                subscription.PlanType = LawyerPlanType.Free;
                subscription.DailyTokenLimit = freePlanDef.DailyTokenLimit;
                subscription.ExpiresAt = null;
                await _dbContext.SaveChangesAsync(cancellationToken);
            }
        }

        return subscription;
    }

    public async Task<Entities.LawyerSubscription> ChangeSubscriptionAsync(Guid lawyerId, LawyerPlanType newPlan, CancellationToken cancellationToken = default)
    {
        var now = _timeProvider.GetUtcNow();
        var subscription = await GetOrCreateSubscriptionAsync(lawyerId, cancellationToken);
        var planDef = _planOptions.Plans.FirstOrDefault(p => p.PlanType.Equals(newPlan.ToString(), StringComparison.OrdinalIgnoreCase));
        
        if (planDef == null)
            throw new InvalidOperationException($"Plan definition for {newPlan} is missing from configuration.");

        subscription.PlanType = newPlan;
        subscription.DailyTokenLimit = planDef.DailyTokenLimit;
        
        if (newPlan == LawyerPlanType.Free)
        {
            subscription.ExpiresAt = null;
        }
        else
        {
            subscription.ExpiresAt = now.AddDays(30);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        return subscription;
    }

    public async Task<LawyerQuotaInfoResponse> GetQuotaAsync(Guid lawyerId, CancellationToken cancellationToken = default)
    {
        var today = GetToday();
        var subscription = await GetOrCreateSubscriptionAsync(lawyerId, cancellationToken);

        var dailyUsage = await _dbContext.LawyerDailyUsages
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.LawyerId == lawyerId && u.UsageDate == today, cancellationToken);

        var ledger = await _dbContext.LawyerQuotaLedgers
            .AsNoTracking()
            .FirstOrDefaultAsync(l => l.LawyerId == lawyerId, cancellationToken);

        int dailyLimit = subscription.DailyTokenLimit;
        int consumedDaily = dailyUsage?.ConsumedTokens ?? 0;
        int remainingDaily = Math.Max(0, dailyLimit - consumedDaily);
        int additionalBalance = ledger?.PurchasedTokenBalance ?? 0;
        int totalRemaining = remainingDaily + additionalBalance;

        return new LawyerQuotaInfoResponse(
            CreditConverter.ToCredits(dailyLimit),
            CreditConverter.ToCredits(consumedDaily),
            CreditConverter.ToCredits(remainingDaily),
            CreditConverter.ToCredits(additionalBalance),
            CreditConverter.ToCredits(totalRemaining),
            subscription.PlanType.ToString(),
            GetNextResetTime()
        );
    }

    public async Task<QuotaReservation> ReserveQuotaAsync(Guid lawyerId, int estimatedMaxTokens, CancellationToken cancellationToken = default)
    {
        var today = GetToday();
        var subscription = await GetOrCreateSubscriptionAsync(lawyerId, cancellationToken);
        var dailyLimit = subscription.DailyTokenLimit;

        // 1. Ensure DailyUsage row exists
        var usageExists = await _dbContext.LawyerDailyUsages.AnyAsync(u => u.LawyerId == lawyerId && u.UsageDate == today, cancellationToken);
        if (!usageExists)
        {
            try
            {
                _dbContext.LawyerDailyUsages.Add(new LawyerDailyUsage { LawyerId = lawyerId, UsageDate = today, ConsumedTokens = 0 });
                await _dbContext.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateException) { /* Ignore constraint violations from race conditions */ }
        }

        // 2. Try to reserve completely from DailyUsage
        var rowsAffected = await _dbContext.LawyerDailyUsages
            .Where(u => u.LawyerId == lawyerId && u.UsageDate == today && (u.ConsumedTokens + estimatedMaxTokens) <= dailyLimit)
            .ExecuteUpdateAsync(s => s.SetProperty(u => u.ConsumedTokens, u => u.ConsumedTokens + estimatedMaxTokens), cancellationToken);

        if (rowsAffected > 0)
        {
            return new QuotaReservation
            {
                TotalReservedTokens = estimatedMaxTokens,
                FreeReservedTokens = estimatedMaxTokens,
                PaidReservedTokens = 0
            };
        }

        // 3. Fallback: split between DailyUsage and QuotaLedger
        using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

        // Re-read current consumption (with lock/no-tracking depending on isolation, but ExecuteUpdate is safe)
        var currentDaily = await _dbContext.LawyerDailyUsages
            .Where(u => u.LawyerId == lawyerId && u.UsageDate == today)
            .Select(u => u.ConsumedTokens)
            .FirstOrDefaultAsync(cancellationToken);

        int remainingDailyQuota = Math.Max(0, dailyLimit - currentDaily);
        int neededFromLedger = estimatedMaxTokens - remainingDailyQuota;

        if (neededFromLedger > 0)
        {
            // Ensure Ledger exists
            var ledgerExists = await _dbContext.LawyerQuotaLedgers.AnyAsync(l => l.LawyerId == lawyerId, cancellationToken);
            if (!ledgerExists)
            {
                try
                {
                    _dbContext.LawyerQuotaLedgers.Add(new LawyerQuotaLedger { LawyerId = lawyerId, PurchasedTokenBalance = 0 });
                    await _dbContext.SaveChangesAsync(cancellationToken);
                }
                catch (DbUpdateException) { }
            }

            // Deduct from Ledger atomically
            var ledgerRows = await _dbContext.LawyerQuotaLedgers
                .Where(l => l.LawyerId == lawyerId && l.PurchasedTokenBalance >= neededFromLedger)
                .ExecuteUpdateAsync(s => s.SetProperty(l => l.PurchasedTokenBalance, l => l.PurchasedTokenBalance - neededFromLedger), cancellationToken);

            if (ledgerRows == 0)
            {
                throw new BusinessException("رصيد المحامي غير كافٍ. برجاء شراء باقة كلمات جديدة أو ترقية خطتك للاستمرار.");
            }
        }

        if (remainingDailyQuota > 0)
        {
            // Consume whatever is left of the daily quota
            await _dbContext.LawyerDailyUsages
                .Where(u => u.LawyerId == lawyerId && u.UsageDate == today)
                .ExecuteUpdateAsync(s => s.SetProperty(u => u.ConsumedTokens, u => u.ConsumedTokens + remainingDailyQuota), cancellationToken);
        }

        await transaction.CommitAsync(cancellationToken);

        return new QuotaReservation
        {
            TotalReservedTokens = estimatedMaxTokens,
            FreeReservedTokens = remainingDailyQuota,
            PaidReservedTokens = neededFromLedger
        };
    }

    public Task<QuotaReservation> ConsumeQuotaAsync(Guid lawyerId, int exactTokens, CancellationToken cancellationToken = default)
    {
        return ReserveQuotaAsync(lawyerId, exactTokens, cancellationToken);
    }

    public async Task SettleQuotaAsync(Guid lawyerId, QuotaReservation reservation, int actualTokensUsed, CancellationToken cancellationToken = default)
    {
        if (actualTokensUsed > reservation.TotalReservedTokens)
        {
            _logger.LogCritical(
                "Lawyer {LawyerId} exceeded quota reservation. Reserved: {Reserved}, Actual: {Actual}. This indicates a bug in estimation.",
                lawyerId, reservation.TotalReservedTokens, actualTokensUsed);
        }

        int unusedTokens = Math.Max(0, reservation.TotalReservedTokens - actualTokensUsed);

        if (unusedTokens > 0)
        {
            await RefundAsync(lawyerId, unusedTokens, cancellationToken);
        }
    }

    public async Task RefundAsync(Guid lawyerId, int tokenAmount, CancellationToken cancellationToken = default)
    {
        var today = GetToday();

        // 1. Try to refund to daily usage first (we shouldn't go below 0)
        var refundedToDaily = 0;
        
        var currentDaily = await _dbContext.LawyerDailyUsages
            .Where(u => u.LawyerId == lawyerId && u.UsageDate == today)
            .Select(u => u.ConsumedTokens)
            .FirstOrDefaultAsync(cancellationToken);

        if (currentDaily > 0)
        {
            refundedToDaily = Math.Min(currentDaily, tokenAmount);
            await _dbContext.LawyerDailyUsages
                .Where(u => u.LawyerId == lawyerId && u.UsageDate == today)
                .ExecuteUpdateAsync(s => s.SetProperty(u => u.ConsumedTokens, u => u.ConsumedTokens - refundedToDaily), cancellationToken);
        }

        int remainingToRefund = tokenAmount - refundedToDaily;

        if (remainingToRefund > 0)
        {
            // 2. Refund rest to ledger
            var ledgerExists = await _dbContext.LawyerQuotaLedgers.AnyAsync(l => l.LawyerId == lawyerId, cancellationToken);
            if (!ledgerExists)
            {
                try
                {
                    _dbContext.LawyerQuotaLedgers.Add(new LawyerQuotaLedger { LawyerId = lawyerId, PurchasedTokenBalance = remainingToRefund });
                    await _dbContext.SaveChangesAsync(cancellationToken);
                }
                catch (DbUpdateException) { }
            }
            else
            {
                await _dbContext.LawyerQuotaLedgers
                    .Where(l => l.LawyerId == lawyerId)
                    .ExecuteUpdateAsync(s => s.SetProperty(l => l.PurchasedTokenBalance, l => l.PurchasedTokenBalance + remainingToRefund), cancellationToken);
            }
        }
    }

    public async Task<LawyerQuotaHistoryResponse> GetQuotaHistoryAsync(Guid lawyerId, int days, CancellationToken cancellationToken = default)
    {
        var cutOff = GetToday().AddDays(-days);

        var usages = await _dbContext.LawyerDailyUsages
            .AsNoTracking()
            .Where(u => u.LawyerId == lawyerId && u.UsageDate >= cutOff)
            .OrderByDescending(u => u.UsageDate)
            .Select(u => new LawyerDailyQuotaUsageDto(
                u.UsageDate.ToString("yyyy-MM-dd"),
                CreditConverter.ToCredits(u.ConsumedTokens)))
            .ToListAsync(cancellationToken);

        return new LawyerQuotaHistoryResponse(usages);
    }

    public async Task<LawyerQuotaTransactionListDto> GetQuotaTransactionsAsync(Guid lawyerId, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var query = _dbContext.LawyerQuotaTransactions
            .AsNoTracking()
            .Where(t => t.LawyerId == lawyerId);

        var totalCount = await query.CountAsync(cancellationToken);

        var transactions = await query
            .OrderByDescending(t => t.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(t => new LawyerQuotaTransactionDto(
                t.Id,
                CreditConverter.ToCredits(t.Amount),
                t.Reason,
                t.ReferenceId,
                t.CreatedAt))
            .ToListAsync(cancellationToken);

        return new LawyerQuotaTransactionListDto(transactions, totalCount);
    }
}
