using Microsoft.EntityFrameworkCore;
using SmartCourt.Common.Exceptions;
using SmartCourt.Features.Payments.DTOs;
using SmartCourt.Features.Payments.Entities;
using SmartCourt.Features.Payments.Enums;
using SmartCourt.Infrastructure.Providers.Payments;
using SmartCourt.Interfaces;
using SmartCourt.Persistence;

namespace SmartCourt.Features.Payments;

public sealed class LawyerPayoutAccountService(
    ApplicationDbContext dbContext,
    ICurrentUserService currentUserService,
    ILawyerPayoutAccountProvider payoutAccountProvider,
    TimeProvider timeProvider) : ILawyerPayoutAccountService
{
    private ProviderPayoutAccountSettings ProviderSettings =>
        payoutAccountProvider.Settings;

    public async Task<LawyerPayoutAccountDto?> GetAsync(
        CancellationToken cancellationToken)
    {
        var lawyerUserId = GetActorUserId();
        var account = await dbContext.LawyerPayoutAccounts
            .SingleOrDefaultAsync(
                item => item.LawyerUserId == lawyerUserId
                    && item.ProviderCode == ProviderSettings.ProviderCode,
                cancellationToken);
        if (account is null)
        {
            return null;
        }

        await SynchronizeAsync(account, cancellationToken);
        return Map(account);
    }

    public async Task<PayoutAccountLinkDto> CreateOnboardingLinkAsync(
        CancellationToken cancellationToken)
    {
        var lawyerUserId = GetActorUserId();
        var user = await dbContext.Users.AsNoTracking().SingleAsync(
            item => item.Id == lawyerUserId,
            cancellationToken);
        if (string.IsNullOrWhiteSpace(user.Email))
        {
            throw new BusinessException(
                "يجب أن يكون للمحامي بريد إلكتروني موثق لبدء إعداد حساب السحب.");
        }

        var account = await dbContext.LawyerPayoutAccounts
            .SingleOrDefaultAsync(
                item => item.LawyerUserId == lawyerUserId
                    && item.ProviderCode == ProviderSettings.ProviderCode,
                cancellationToken);
        if (account is null)
        {
            var providerAccount = await payoutAccountProvider.CreateAccountAsync(
                new ProviderPayoutAccountCreateRequest(
                    lawyerUserId,
                    user.Email,
                    ProviderSettings.DefaultCountry,
                    $"payout-account-{lawyerUserId:N}"),
                cancellationToken);
            account = new LawyerPayoutAccount(
                Guid.NewGuid(),
                lawyerUserId,
                ProviderSettings.ProviderCode,
                providerAccount.ProviderAccountId,
                providerAccount.IsLive,
                UtcNow);
            Apply(account, providerAccount);
            dbContext.LawyerPayoutAccounts.Add(account);
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        if (account.Status == LawyerPayoutAccountStatus.Enabled)
        {
            throw new BusinessException(
                "حساب السحب لدى المحامي مفعّل بالفعل.");
        }

        var link = await payoutAccountProvider.CreateOnboardingLinkAsync(
            new ProviderOnboardingLinkRequest(
                account.ProviderAccountId,
                ProviderSettings.ReturnUrl,
                ProviderSettings.RefreshUrl,
                $"payout-onboarding-{account.Id:N}-{UtcNow:yyyyMMddHHmm}"),
            cancellationToken);
        account.Status = LawyerPayoutAccountStatus.Onboarding;
        account.UpdatedAt = UtcNow;
        await dbContext.SaveChangesAsync(cancellationToken);
        return new PayoutAccountLinkDto(link.Url, link.ExpiresAt);
    }

    public async Task<PayoutAccountLinkDto> CreateDashboardLinkAsync(
        CancellationToken cancellationToken)
    {
        var lawyerUserId = GetActorUserId();
        var account = await dbContext.LawyerPayoutAccounts.SingleOrDefaultAsync(
            item => item.LawyerUserId == lawyerUserId
                && item.ProviderCode == ProviderSettings.ProviderCode,
            cancellationToken)
            ?? throw new BusinessException(
                "لم يتم إنشاء حساب سحب لهذا المحامي بعد.");
        var url = await payoutAccountProvider.CreateDashboardLinkAsync(
            account.ProviderAccountId,
            cancellationToken);
        return new PayoutAccountLinkDto(url, null);
    }

    public async Task<LawyerPayoutAccountDto> LinkSandboxAccountAsync(
        LinkLawyerPayoutAccountRequest request,
        CancellationToken cancellationToken)
    {
        if (!ProviderSettings.SandboxOnly)
        {
            throw new ForbiddenAccessException(
                "ربط حساب مزود الدفع يدويًا متاح في بيئة الاختبار فقط.");
        }

        if (request.LawyerUserId == Guid.Empty
            || string.IsNullOrWhiteSpace(request.ProviderAccountId))
        {
            throw new BusinessException(
                "معرّف المحامي ومعرّف حساب مزود الدفع مطلوبان.");
        }

        _ = await dbContext.Users.AsNoTracking().SingleOrDefaultAsync(
                item => item.Id == request.LawyerUserId,
                cancellationToken)
            ?? throw new BusinessException("المحامي المطلوب غير موجود.");
        if (await dbContext.LawyerPayoutAccounts.AnyAsync(
                item => item.ProviderCode == ProviderSettings.ProviderCode
                    && (item.LawyerUserId == request.LawyerUserId
                        || item.ProviderAccountId == request.ProviderAccountId),
                cancellationToken))
        {
            throw new ConflictException(
                "المحامي أو حساب مزود الدفع مرتبط مسبقًا بحساب سحب.");
        }

        var providerAccount = await payoutAccountProvider.GetAccountAsync(
            request.ProviderAccountId.Trim(),
            cancellationToken);
        if (providerAccount.IsLive)
        {
            throw new ForbiddenAccessException(
                "لا يمكن ربط حساب مزود دفع حي من مسار إعداد الاختبار.");
        }

        var account = new LawyerPayoutAccount(
            Guid.NewGuid(),
            request.LawyerUserId,
            ProviderSettings.ProviderCode,
            providerAccount.ProviderAccountId,
            providerAccount.IsLive,
            UtcNow);
        Apply(account, providerAccount);
        dbContext.LawyerPayoutAccounts.Add(account);
        await dbContext.SaveChangesAsync(cancellationToken);
        return Map(account);
    }

    public async Task SynchronizeProviderAccountAsync(
        string providerAccountId,
        CancellationToken cancellationToken)
    {
        var account = await dbContext.LawyerPayoutAccounts
            .SingleOrDefaultAsync(
                item => item.ProviderAccountId == providerAccountId,
                cancellationToken);
        if (account is not null)
        {
            await SynchronizeAsync(account, cancellationToken);
        }
    }

    private async Task SynchronizeAsync(
        LawyerPayoutAccount account,
        CancellationToken cancellationToken)
    {
        var providerAccount = await payoutAccountProvider.GetAccountAsync(
            account.ProviderAccountId,
            cancellationToken);
        Apply(account, providerAccount);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private void Apply(
        LawyerPayoutAccount account,
        ProviderPayoutAccountResult result)
    {
        if (!string.Equals(
                account.ProviderAccountId,
                result.ProviderAccountId,
                StringComparison.Ordinal))
        {
            throw new BusinessException(
                "معرّف حساب السحب لدى المزود لا يطابق الحساب المحلي.");
        }

        account.DetailsSubmitted = result.DetailsSubmitted;
        account.TransfersEnabled = result.TransfersEnabled;
        account.PayoutsEnabled = result.PayoutsEnabled;
        account.Country = result.Country.ToUpperInvariant();
        account.DefaultCurrency = result.DefaultCurrency.ToLowerInvariant();
        account.MaskedDestination = result.MaskedDestination;
        account.LastProviderStatus = result.ProviderStatus;
        account.Status = result.DetailsSubmitted
            && result.TransfersEnabled
            && result.PayoutsEnabled
                ? LawyerPayoutAccountStatus.Enabled
                : string.Equals(
                    result.ProviderStatus,
                    "restricted",
                    StringComparison.OrdinalIgnoreCase)
                    ? LawyerPayoutAccountStatus.Restricted
                    : LawyerPayoutAccountStatus.Onboarding;
        account.LastSynchronizedAt = UtcNow;
        account.UpdatedAt = UtcNow;
    }

    private Guid GetActorUserId()
    {
        if (!currentUserService.IsAuthenticated
            || !currentUserService.UserId.HasValue
            || currentUserService.UserId.Value == Guid.Empty)
        {
            throw new AuthenticationException(
                "يجب تسجيل الدخول للوصول إلى حساب السحب.");
        }

        return currentUserService.UserId.Value;
    }

    private static LawyerPayoutAccountDto Map(LawyerPayoutAccount account)
        => new(
            account.Id,
            account.ProviderCode,
            account.Status.ToString(),
            account.DetailsSubmitted,
            account.TransfersEnabled,
            account.PayoutsEnabled,
            account.Country,
            account.DefaultCurrency,
            account.MaskedDestination,
            account.LastSynchronizedAt);

    private DateTime UtcNow => timeProvider.GetUtcNow().UtcDateTime;
}
