using System.Security.Cryptography;
using System.Text;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using SmartCourt.Common.Exceptions;

namespace SmartCourt.Common.RateLimiting;

public sealed class AccountKeyRateLimiter : IAccountKeyRateLimiter, IDisposable
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly PartitionedRateLimiter<string> _forgotPasswordAccountLimiter =
        CreateLimiter(3, TimeSpan.FromHours(1));
    private readonly PartitionedRateLimiter<string> _resendVerificationMinuteLimiter =
        CreateLimiter(1, TimeSpan.FromMinutes(1));
    private readonly PartitionedRateLimiter<string> _resendVerificationHourLimiter =
        CreateLimiter(3, TimeSpan.FromHours(1));
    private readonly PartitionedRateLimiter<string> _resetPasswordAccountLimiter =
        CreateLimiter(5, TimeSpan.FromHours(1));
    private readonly PartitionedRateLimiter<string> _resetPasswordTokenLimiter =
        CreateLimiter(5, TimeSpan.FromHours(1));
    private readonly PartitionedRateLimiter<string> _confirmEmailUserIdLimiter =
        CreateLimiter(5, TimeSpan.FromHours(1));

    public AccountKeyRateLimiter(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    public void CheckForgotPassword(string email)
    {
        EnsureAllowed(_forgotPasswordAccountLimiter, NormalizeEmail(email));
    }

    public void CheckResendVerification(string email)
    {
        var normalizedEmail = NormalizeEmail(email);
        EnsureAllowed(_resendVerificationMinuteLimiter, normalizedEmail);
        EnsureAllowed(_resendVerificationHourLimiter, normalizedEmail);
    }

    public void CheckResetPassword(string email, string token)
    {
        EnsureAllowed(_resetPasswordAccountLimiter, NormalizeEmail(email));
        EnsureAllowed(_resetPasswordTokenLimiter, token);
    }

    public void CheckConfirmEmail(string userId)
    {
        EnsureAllowed(_confirmEmailUserIdLimiter, userId);
    }

    public void Dispose()
    {
        _forgotPasswordAccountLimiter.Dispose();
        _resendVerificationMinuteLimiter.Dispose();
        _resendVerificationHourLimiter.Dispose();
        _resetPasswordAccountLimiter.Dispose();
        _resetPasswordTokenLimiter.Dispose();
        _confirmEmailUserIdLimiter.Dispose();
    }

    private string NormalizeEmail(string email)
    {
        using var scope = _scopeFactory.CreateScope();
        var lookupNormalizer = scope.ServiceProvider.GetRequiredService<ILookupNormalizer>();
        return lookupNormalizer.NormalizeEmail(email ?? string.Empty) ?? string.Empty;
    }

    private static void EnsureAllowed(PartitionedRateLimiter<string> limiter, string value)
    {
        using var lease = limiter.AttemptAcquire(value);
        if (!lease.IsAcquired)
        {
            throw new TooManyRequestsException();
        }
    }

    private static PartitionedRateLimiter<string> CreateLimiter(int permitLimit, TimeSpan window)
    {
        return PartitionedRateLimiter.Create<string, string>(value =>
            RateLimitPartition.GetFixedWindowLimiter(
                Hash(value),
                _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = permitLimit,
                    Window = window,
                    QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                    QueueLimit = 0,
                    AutoReplenishment = true
                }));
    }

    private static string Hash(string value)
    {
        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value ?? string.Empty)));
    }
}
