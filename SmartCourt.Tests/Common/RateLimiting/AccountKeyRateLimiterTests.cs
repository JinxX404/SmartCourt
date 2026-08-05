using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using SmartCourt.Common.Exceptions;
using SmartCourt.Common.RateLimiting;
using Xunit;

namespace SmartCourt.Tests.Common.RateLimiting;

public sealed class AccountKeyRateLimiterTests
{
    [Fact]
    public void CheckForgotPassword_NormalizesEmailAndIsolatesAccounts()
    {
        using var limiter = CreateLimiter();

        limiter.CheckForgotPassword("User@Example.com");
        limiter.CheckForgotPassword("user@example.com");
        limiter.CheckForgotPassword("USER@EXAMPLE.COM");

        Assert.Throws<TooManyRequestsException>(() =>
            limiter.CheckForgotPassword("user@example.com"));
        limiter.CheckForgotPassword("other@example.com");
    }

    [Fact]
    public void CheckResendVerification_EnforcesCooldownWithoutBlockingAnotherAccount()
    {
        using var limiter = CreateLimiter();

        limiter.CheckResendVerification("user@example.com");

        Assert.Throws<TooManyRequestsException>(() =>
            limiter.CheckResendVerification("USER@example.com"));
        limiter.CheckResendVerification("other@example.com");
    }

    [Fact]
    public void CheckResetPassword_IsolatesTokenBuckets()
    {
        using var limiter = CreateLimiter();

        for (var index = 0; index < 5; index++)
        {
            limiter.CheckResetPassword($"user{index}@example.com", "shared-token");
        }

        Assert.Throws<TooManyRequestsException>(() =>
            limiter.CheckResetPassword("user5@example.com", "shared-token"));
        limiter.CheckResetPassword("user6@example.com", "different-token");
    }

    [Fact]
    public void CheckResetPassword_EnforcesAccountBucketAcrossTokens()
    {
        using var limiter = CreateLimiter();

        for (var index = 0; index < 5; index++)
        {
            limiter.CheckResetPassword("user@example.com", $"token-{index}");
        }

        Assert.Throws<TooManyRequestsException>(() =>
            limiter.CheckResetPassword("user@example.com", "token-5"));
        limiter.CheckResetPassword("other@example.com", "token-6");
    }

    [Fact]
    public void CheckConfirmEmail_IsolatesUserIdBuckets()
    {
        using var limiter = CreateLimiter();

        for (var index = 0; index < 5; index++)
        {
            limiter.CheckConfirmEmail("user-id-1");
        }

        Assert.Throws<TooManyRequestsException>(() =>
            limiter.CheckConfirmEmail("user-id-1"));
        limiter.CheckConfirmEmail("user-id-2");
    }

    private static AccountKeyRateLimiter CreateLimiter()
    {
        var services = new ServiceCollection();
        services.AddScoped<ILookupNormalizer, UpperInvariantLookupNormalizer>();
        var serviceProvider = services.BuildServiceProvider();
        var scopeFactory = serviceProvider.GetRequiredService<IServiceScopeFactory>();
        return new AccountKeyRateLimiter(scopeFactory);
    }
}
