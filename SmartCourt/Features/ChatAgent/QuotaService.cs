using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SmartCourt.Common.Configuration;
using SmartCourt.Common.Domain;
using SmartCourt.Common.Exceptions;
using SmartCourt.Features.ChatAgent.DTOs;
using SmartCourt.Features.ChatAgent.Entities;
using SmartCourt.Persistence;

namespace SmartCourt.Features.ChatAgent;

internal sealed class QuotaService : IQuotaService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly TimeProvider _timeProvider;
    private readonly ILogger<QuotaService> _logger;
    private readonly QuotaOptions _quotaOptions;

    public QuotaService(ApplicationDbContext dbContext, TimeProvider timeProvider, ILogger<QuotaService> logger, IOptions<QuotaOptions> quotaOptions)
    {
        _dbContext = dbContext;
        _timeProvider = timeProvider;
        _logger = logger;
        _quotaOptions = quotaOptions.Value;
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
            var utcNow = _timeProvider.GetUtcNow();
            var midnight = utcNow.UtcDateTime.Date;
            return new DateTimeOffset(midnight, TimeSpan.Zero);
        }
    }

    private DateTimeOffset GetNextResetAt()
    {
        try
        {
            var tz = TimeZoneInfo.FindSystemTimeZoneById(_quotaOptions.Timezone);
            var dateInTz = TimeZoneInfo.ConvertTimeFromUtc(_timeProvider.GetUtcNow().UtcDateTime, tz);
            var nextMidnight = dateInTz.Date.AddDays(1);
            return new DateTimeOffset(nextMidnight, tz.GetUtcOffset(nextMidnight));
        }
        catch (Exception ex) when (ex is TimeZoneNotFoundException or InvalidTimeZoneException)
        {
            var utcNow = _timeProvider.GetUtcNow();
            var nextMidnight = utcNow.UtcDateTime.Date.AddDays(1);
            return new DateTimeOffset(nextMidnight, TimeSpan.Zero);
        }
    }

    public async Task<QuotaInfoResponse> GetQuotaAsync(Guid clientId, CancellationToken cancellationToken = default)
    {
        var date = GetToday();

        int dailyLimit = await GetDailyLimitAsync(clientId, cancellationToken);

        var usage = await _dbContext.DailyUsages
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.ClientId == clientId && x.UsageDate == date, cancellationToken);
        
        int consumedDaily = usage?.ConsumedTokens ?? 0;
        int remainingDaily = Math.Max(0, dailyLimit - consumedDaily);

        var ledger = await _dbContext.QuotaLedgers
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.ClientId == clientId, cancellationToken);
        
        int availableAdditional = ledger?.AdditionalTokenBalance ?? 0;

        return new QuotaInfoResponse(
            CreditConverter.ToCredits(dailyLimit),
            CreditConverter.ToCredits(consumedDaily),
            CreditConverter.ToCredits(remainingDaily),
            CreditConverter.ToCredits(availableAdditional),
            CreditConverter.ToCredits(remainingDaily + availableAdditional),
            GetNextResetAt()
        );
    }

    public async Task<QuotaHistoryResponse> GetQuotaHistoryAsync(Guid clientId, int days, CancellationToken cancellationToken = default)
    {
        var endDate = GetToday();
        var startDate = endDate.AddDays(-days + 1);

        var usages = await _dbContext.DailyUsages
            .AsNoTracking()
            .Where(x => x.ClientId == clientId && x.UsageDate >= startDate && x.UsageDate <= endDate)
            .OrderBy(x => x.UsageDate)
            .ToListAsync(cancellationToken);

        var usageDtos = usages.Select(x => new DailyQuotaUsageDto(
            x.UsageDate.ToString("yyyy-MM-dd"),
            CreditConverter.ToCredits(x.ConsumedTokens)
        )).ToList();

        return new QuotaHistoryResponse(usageDtos);
    }

    public async Task<QuotaTransactionListDto> GetQuotaTransactionsAsync(Guid clientId, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var query = _dbContext.QuotaTransactions
            .AsNoTracking()
            .Where(x => x.ClientId == clientId);

        int totalCount = await query.CountAsync(cancellationToken);

        var transactions = await query
            .OrderByDescending(x => x.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new QuotaTransactionDto(
                x.Id,
                CreditConverter.ToCredits(x.Amount),
                x.Reason.ToString(),
                x.ReferenceId,
                x.CreatedAt))
            .ToListAsync(cancellationToken);

        return new QuotaTransactionListDto(transactions, totalCount);
    }

    public async Task<QuotaReservation> ConsumeQuotaAsync(Guid clientId, int tokenAmount, CancellationToken cancellationToken = default)
    {
        var date = GetToday();

        int dailyLimit = await GetDailyLimitAsync(clientId, cancellationToken);

        // Ensure DailyUsage row exists for today
        var usageExists = await _dbContext.DailyUsages
            .AnyAsync(x => x.ClientId == clientId && x.UsageDate == date, cancellationToken);
        
        var newUsage = DailyUsage.Create(clientId, date);
        if (!usageExists)
        {
            try
            {
                _dbContext.DailyUsages.Add(newUsage);
                await _dbContext.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateException ex) when (IsUniqueConstraintViolation(ex))
            {
                // Another concurrent request may have inserted it, which is fine
                _dbContext.Entry(newUsage).State = EntityState.Detached;
            }
        }

        using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

        // 1. Try to consume from Daily Free Quota entirely
        var dailyRowsAffected = await _dbContext.DailyUsages
            .Where(x => x.ClientId == clientId && x.UsageDate == date && (x.ConsumedTokens + tokenAmount) <= dailyLimit)
            .ExecuteUpdateAsync(s => s.SetProperty(x => x.ConsumedTokens, x => x.ConsumedTokens + tokenAmount), cancellationToken);
        
        if (dailyRowsAffected > 0)
        {
            await transaction.CommitAsync(cancellationToken);
            return new QuotaReservation
            {
                TotalReservedTokens = tokenAmount,
                FreeReservedTokens = tokenAmount,
                PaidReservedTokens = 0
            }; // Success, entirely covered by free daily quota
        }

        // 2. Daily Free Quota is exhausted or partially exhausted. Fetch current daily usage.
        var currentUsage = await _dbContext.DailyUsages
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.ClientId == clientId && x.UsageDate == date, cancellationToken);
        
        if (currentUsage == null)
        {
            throw new InvalidOperationException("فشل في استهلاك الرصيد بسبب مشكلة في النظام: تعذر إنشاء أو الوصول إلى سجل الاستهلاك اليومي.");
        }

        int consumedDaily = currentUsage.ConsumedTokens;
        int remainingDaily = Math.Max(0, dailyLimit - consumedDaily);

        // 3. Consume whatever is left of the free daily quota
        int amountToConsumeFromPaid = tokenAmount;
        int freeTokensConsumed = 0;

        if (remainingDaily > 0)
        {
            // Atomically consume the rest of the daily quota
            var partialDailyRows = await _dbContext.DailyUsages
                .Where(x => x.ClientId == clientId && x.UsageDate == date && x.ConsumedTokens == consumedDaily)
                .ExecuteUpdateAsync(s => s.SetProperty(x => x.ConsumedTokens, x => x.ConsumedTokens + remainingDaily), cancellationToken);

            if (partialDailyRows > 0)
            {
                amountToConsumeFromPaid -= remainingDaily;
                freeTokensConsumed = remainingDaily;
            }
            else
            {
                // Another thread might have consumed the remaining daily. Just attempt to consume entirely from paid.
            }
        }

        // 4. Consume the remainder from Purchased Tokens (QuotaLedger)
        if (amountToConsumeFromPaid > 0)
        {
            var ledgerRowsAffected = await _dbContext.QuotaLedgers
                .Where(x => x.ClientId == clientId && x.AdditionalTokenBalance >= amountToConsumeFromPaid)
                .ExecuteUpdateAsync(s => s.SetProperty(x => x.AdditionalTokenBalance, x => x.AdditionalTokenBalance - amountToConsumeFromPaid), cancellationToken);

            if (ledgerRowsAffected > 0)
            {
                // Log the deduction of paid tokens
                var transactionRecord = QuotaTransaction.Create(
                    Guid.NewGuid(),
                    clientId,
                    -amountToConsumeFromPaid,
                    QuotaTransactionReason.LlmConsumption,
                    null,
                    _timeProvider.GetUtcNow()
                );
                _dbContext.QuotaTransactions.Add(transactionRecord);
                await _dbContext.SaveChangesAsync(cancellationToken);
                
                await transaction.CommitAsync(cancellationToken);
                return new QuotaReservation
                {
                    TotalReservedTokens = tokenAmount,
                    FreeReservedTokens = freeTokensConsumed,
                    PaidReservedTokens = amountToConsumeFromPaid
                }; // Success
            }

            // Both free and paid quotas exhausted
            throw new InsufficientQuotaException(
                dailyLimit,
                consumedDaily, // Might be stale due to concurrency, but good enough for exception context
                tokenAmount,
                GetNextResetAt(),
                "رصيدك غير كافٍ. يرجى الانتظار حتى يوم غد أو شراء باقة كلمات جديدة."
            );
        }
        
        await transaction.CommitAsync(cancellationToken);
        return new QuotaReservation
        {
            TotalReservedTokens = tokenAmount,
            FreeReservedTokens = freeTokensConsumed,
            PaidReservedTokens = 0
        };
    }

    public async Task RefundAsync(Guid clientId, int tokenAmount, CancellationToken cancellationToken = default)
    {
        var date = GetToday();

        await _dbContext.DailyUsages
            .Where(x => x.ClientId == clientId && x.UsageDate == date && x.ConsumedTokens > 0)
            .ExecuteUpdateAsync(s => s.SetProperty(x => x.ConsumedTokens, x => x.ConsumedTokens > tokenAmount ? x.ConsumedTokens - tokenAmount : 0), cancellationToken);
    }

    public async Task<QuotaReservation> ReserveQuotaAsync(Guid clientId, int estimatedMaxTokens, CancellationToken cancellationToken = default)
    {
        return await ConsumeQuotaAsync(clientId, estimatedMaxTokens, cancellationToken);
    }

    public async Task SettleQuotaAsync(Guid clientId, QuotaReservation reservation, int actualTokensUsed, CancellationToken cancellationToken = default)
    {
        int unusedTokens = reservation.TotalReservedTokens - actualTokensUsed;
        if (unusedTokens > 0)
        {
            // Refund from paid first, then free
            int refundToPaid = Math.Min(reservation.PaidReservedTokens, unusedTokens);
            int refundToFree = unusedTokens - refundToPaid;

            if (refundToPaid > 0)
            {
                await _dbContext.QuotaLedgers
                    .Where(x => x.ClientId == clientId)
                    .ExecuteUpdateAsync(s => s.SetProperty(x => x.AdditionalTokenBalance, x => x.AdditionalTokenBalance + refundToPaid), cancellationToken);

                var transactionRecord = QuotaTransaction.Create(
                    Guid.NewGuid(),
                    clientId,
                    refundToPaid,
                    "Refund for unused tokens",
                    null,
                    _timeProvider.GetUtcNow()
                );
                _dbContext.QuotaTransactions.Add(transactionRecord);
                await _dbContext.SaveChangesAsync(cancellationToken);
            }

            if (refundToFree > 0)
            {
                await RefundAsync(clientId, refundToFree, cancellationToken);
            }
        }
        else if (unusedTokens < 0)
        {
            int uncoveredTokens = Math.Abs(unusedTokens);
            _logger.LogCritical(
                "CRITICAL QUOTA ANOMALY: Actual usage exceeded reservation. ClientId: {ClientId}. Reserved: {ReservedTokens}. Actual: {ActualTokens}. Uncovered: {UncoveredTokens}. " +
                "This indicates the 1:1 token-to-character heuristic was breached. The overage is absorbed by the system to prevent negative ledger balances.",
                clientId, reservation.TotalReservedTokens, actualTokensUsed, uncoveredTokens);
        }
    }

    private async Task<int> GetDailyLimitAsync(Guid clientId, CancellationToken cancellationToken)
    {
        var profile = await _dbContext.QuotaProfiles
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.ClientId == clientId, cancellationToken);

        if (profile != null)
        {
            return profile.DailyTokenLimit;
        }

        var globalProfile = await _dbContext.QuotaProfiles
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.ClientId == QuotaProfile.GlobalProfileId, cancellationToken);

        if (globalProfile != null)
        {
            return globalProfile.DailyTokenLimit;
        }

        return _quotaOptions.DailyFreeTokens;
    }

    private static bool IsUniqueConstraintViolation(DbUpdateException ex)
    {
        var innerEx = ex.InnerException;
        if (innerEx == null)
            return false;

        var message = innerEx.Message.ToLowerInvariant();
        return message.Contains("unique constraint") 
            || message.Contains("duplicate key") 
            || message.Contains("unique index");
    }

    public async Task<DefaultQuotaResponse> GetDefaultQuotaAsync(CancellationToken cancellationToken = default)
    {
        var globalProfile = await _dbContext.QuotaProfiles
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.ClientId == QuotaProfile.GlobalProfileId, cancellationToken);

        var limitTokens = globalProfile != null ? globalProfile.DailyTokenLimit : _quotaOptions.DailyFreeTokens;
        return new DefaultQuotaResponse(CreditConverter.ToCredits(limitTokens));
    }
}

