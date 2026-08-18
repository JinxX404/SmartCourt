using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using SmartCourt.Common.Configuration;
using SmartCourt.Common.Exceptions;
using SmartCourt.Features.Admin.Quotas.DTOs;
using SmartCourt.Features.ChatAgent.Entities;
using SmartCourt.Persistence;
using SmartCourt.Common.Domain;

namespace SmartCourt.Features.Admin.Quotas;

internal sealed class AdminQuotaService : IAdminQuotaService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly TimeProvider _timeProvider;
    private readonly SmartCourt.Features.ChatAgent.IQuotaService _quotaService;
    private readonly QuotaOptions _quotaOptions;

    public AdminQuotaService(
        ApplicationDbContext dbContext,
        TimeProvider timeProvider,
        SmartCourt.Features.ChatAgent.IQuotaService quotaService,
        IOptions<QuotaOptions> quotaOptions)
    {
        _dbContext = dbContext;
        _timeProvider = timeProvider;
        _quotaService = quotaService;
        _quotaOptions = quotaOptions.Value;
    }

    public async Task SetGlobalDailyLimitAsync(UpdateDailyLimitRequest request, CancellationToken cancellationToken = default)
    {
        await SetLimitAsync(QuotaProfile.GlobalProfileId, CreditConverter.ToTokens(request.DailyCreditLimit), cancellationToken);
    }

    public async Task SetClientDailyLimitAsync(Guid clientId, UpdateDailyLimitRequest request, CancellationToken cancellationToken = default)
    {
        // Ensure the ID actually belongs to a Client
        var isClient = await _dbContext.Users
            .Where(u => u.Id == clientId)
            .Select(u => _dbContext.UserRoles.Any(ur => ur.UserId == u.Id && _dbContext.Roles.Any(r => r.Id == ur.RoleId && r.Name == "Client")))
            .FirstOrDefaultAsync(cancellationToken);

        if (!isClient)
        {
            throw new BusinessException("هذا المستخدم ليس عميلاً. لا يمكنك تطبيق الحصص إلا على العملاء.");
        }

        await SetLimitAsync(clientId, CreditConverter.ToTokens(request.DailyCreditLimit), cancellationToken);
    }

    private async Task SetLimitAsync(Guid clientId, int dailyLimit, CancellationToken cancellationToken)
    {
        var profile = await _dbContext.QuotaProfiles
            .FirstOrDefaultAsync(p => p.ClientId == clientId, cancellationToken);

        if (profile == null)
        {
            profile = QuotaProfile.Create(clientId, dailyLimit);
            _dbContext.QuotaProfiles.Add(profile);
        }
        else
        {
            profile.UpdateLimit(dailyLimit);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task AdjustClientQuotaAsync(Guid clientId, AdjustQuotaRequest request, Guid adminId, CancellationToken cancellationToken = default)
    {
        // Ensure the ID actually belongs to a Client
        var isClient = await _dbContext.Users
            .Where(u => u.Id == clientId)
            .Select(u => _dbContext.UserRoles.Any(ur => ur.UserId == u.Id && _dbContext.Roles.Any(r => r.Id == ur.RoleId && r.Name == "Client")))
            .FirstOrDefaultAsync(cancellationToken);

        if (!isClient)
        {
            throw new BusinessException("هذا المستخدم ليس عميلاً. لا يمكنك تعديل رصيد إلا للعملاء.");
        }

        // 1. Ensure Ledger exists
        var ledger = await _dbContext.QuotaLedgers
            .FirstOrDefaultAsync(l => l.ClientId == clientId, cancellationToken);

        if (ledger == null)
        {
            ledger = QuotaLedger.Create(clientId);
            _dbContext.QuotaLedgers.Add(ledger);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        // 2. Adjust balance atomically (to avoid concurrency issues with consumption)
        int tokensToAdjust = CreditConverter.ToTokens(request.CreditAmount);

        var rowsAffected = await _dbContext.QuotaLedgers
            .Where(x => x.ClientId == clientId && (tokensToAdjust >= 0 || x.AdditionalTokenBalance >= -tokensToAdjust))
            .ExecuteUpdateAsync(s => s.SetProperty(x => x.AdditionalTokenBalance, x => x.AdditionalTokenBalance + tokensToAdjust), cancellationToken);

        if (rowsAffected == 0 && tokensToAdjust < 0)
        {
            throw new BusinessException("الرصيد الحالي لا يكفي لخصم هذه القيمة.");
        }

        // 3. Log the transaction
        var transaction = QuotaTransaction.Create(
            Guid.NewGuid(),
            clientId,
            tokensToAdjust,
            request.Reason,
            $"Admin_{adminId}",
            _timeProvider.GetUtcNow()
        );

        _dbContext.QuotaTransactions.Add(transaction);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<SmartCourt.Features.ChatAgent.DTOs.QuotaInfoResponse> GetClientQuotaAsync(Guid clientId, CancellationToken cancellationToken = default)
    {
        var isClient = await _dbContext.Users
            .Where(u => u.Id == clientId)
            .Select(u => _dbContext.UserRoles.Any(ur => ur.UserId == u.Id && _dbContext.Roles.Any(r => r.Id == ur.RoleId && r.Name == "Client")))
            .FirstOrDefaultAsync(cancellationToken);

        if (!isClient)
        {
            throw new BusinessException("هذا المستخدم ليس عميلاً.");
        }

        return await _quotaService.GetQuotaAsync(clientId, cancellationToken);
    }

    public async Task<SmartCourt.Features.ChatAgent.DTOs.QuotaTransactionListDto> GetClientQuotaTransactionsAsync(Guid clientId, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var isClient = await _dbContext.Users
            .Where(u => u.Id == clientId)
            .Select(u => _dbContext.UserRoles.Any(ur => ur.UserId == u.Id && _dbContext.Roles.Any(r => r.Id == ur.RoleId && r.Name == "Client")))
            .FirstOrDefaultAsync(cancellationToken);

        if (!isClient)
        {
            throw new BusinessException("هذا المستخدم ليس عميلاً.");
        }

        return await _quotaService.GetQuotaTransactionsAsync(clientId, page, pageSize, cancellationToken);
    }

    public async Task<SmartCourt.Features.ChatAgent.Monetization.DTOs.TokenBundlePurchaseListDto> GetPurchasesAsync(int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var query = _dbContext.TokenBundlePaymentTransactions
            .AsNoTracking();

        int totalCount = await query.CountAsync(cancellationToken);

        var purchases = await query
            .OrderByDescending(x => x.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new SmartCourt.Features.ChatAgent.Monetization.DTOs.TokenBundlePurchaseDto(
                x.Id,
                x.BundleId,
                x.PriceEgp,
                CreditConverter.ToCredits(x.TokenAmount),
                x.Status.ToString(),
                x.FailureReason,
                x.CreatedAt,
                x.UpdatedAt))
            .ToListAsync(cancellationToken);

        return new SmartCourt.Features.ChatAgent.Monetization.DTOs.TokenBundlePurchaseListDto(purchases, totalCount);
    }

    public async Task<GlobalDailyLimitResponse> GetGlobalDailyLimitAsync(CancellationToken cancellationToken = default)
    {
        var globalProfile = await _dbContext.QuotaProfiles
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.ClientId == QuotaProfile.GlobalProfileId, cancellationToken);

        var limit = globalProfile != null ? globalProfile.DailyTokenLimit : _quotaOptions.DailyFreeTokens;
        
        return new GlobalDailyLimitResponse(CreditConverter.ToCredits(limit));
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

    public async Task<AdminQuotaClientSummaryListDto> GetClientsQuotaSummaryAsync(string? search, bool? isExhausted, bool? hasAdditionalBalance, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var today = GetToday();
        var defaultDailyLimitResponse = await GetGlobalDailyLimitAsync(cancellationToken);
        int defaultDailyTokens = CreditConverter.ToTokens(defaultDailyLimitResponse.DailyCreditLimit);

        var clientRole = await _dbContext.Roles.FirstOrDefaultAsync(r => r.Name == "Client", cancellationToken);
        if (clientRole == null)
            return new AdminQuotaClientSummaryListDto(new System.Collections.Generic.List<AdminQuotaClientSummaryDto>(), 0);

        var query = _dbContext.Users
            .AsNoTracking()
            .Where(u => _dbContext.UserRoles.Any(ur => ur.UserId == u.Id && ur.RoleId == clientRole.Id));

        if (!string.IsNullOrWhiteSpace(search))
        {
            var lowerSearch = search.ToLower();
            query = query.Where(u => u.FullName.ToLower().Contains(lowerSearch) || (u.Email != null && u.Email.ToLower().Contains(lowerSearch)));
        }

        var projectedQuery = query.Select(u => new
        {
            u.Id,
            u.FullName,
            u.Email,
            ProfileLimit = _dbContext.QuotaProfiles.Where(p => p.ClientId == u.Id).Select(p => (int?)p.DailyTokenLimit).FirstOrDefault(),
            ConsumedDaily = _dbContext.DailyUsages.Where(d => d.ClientId == u.Id && d.UsageDate == today).Select(d => (int?)d.ConsumedTokens).FirstOrDefault(),
            AdditionalBalance = _dbContext.QuotaLedgers.Where(l => l.ClientId == u.Id).Select(l => (int?)l.AdditionalTokenBalance).FirstOrDefault()
        });

        var filteredQuery = projectedQuery.Select(x => new
        {
            x.Id,
            x.FullName,
            x.Email,
            DailyTokenLimit = x.ProfileLimit ?? defaultDailyTokens,
            ConsumedDailyTokens = x.ConsumedDaily ?? 0,
            AvailableAdditionalTokens = x.AdditionalBalance ?? 0
        });

        if (isExhausted.HasValue && isExhausted.Value)
        {
            filteredQuery = filteredQuery.Where(x => x.ConsumedDailyTokens >= x.DailyTokenLimit);
        }

        if (hasAdditionalBalance.HasValue && hasAdditionalBalance.Value)
        {
            filteredQuery = filteredQuery.Where(x => x.AvailableAdditionalTokens > 0);
        }

        int totalCount = await filteredQuery.CountAsync(cancellationToken);

        var results = await filteredQuery
            .OrderBy(x => x.FullName)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var dtos = results.Select(x => new AdminQuotaClientSummaryDto(
            x.Id,
            x.FullName,
            x.Email ?? string.Empty,
            CreditConverter.ToCredits(x.DailyTokenLimit),
            CreditConverter.ToCredits(x.ConsumedDailyTokens),
            CreditConverter.ToCredits(x.AvailableAdditionalTokens),
            CreditConverter.ToCredits(Math.Max(0, x.DailyTokenLimit - x.ConsumedDailyTokens) + x.AvailableAdditionalTokens)
        )).ToList();

        return new AdminQuotaClientSummaryListDto(dtos, totalCount);
    }
}
