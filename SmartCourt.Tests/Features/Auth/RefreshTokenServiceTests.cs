using SmartCourt.Common.Entities;
using SmartCourt.Common.Exceptions;
using SmartCourt.Features.Auth.Enums;
using SmartCourt.Features.Auth.RefreshToken;
using SmartCourt.Features.Auth.Shared;
using SmartCourt.Interfaces.Providers;
using SmartCourt.Providers.Jwt;
using System.Security.Cryptography;
using System.Text;
using Xunit;

namespace SmartCourt.Tests.Features.Auth;

public sealed class RefreshTokenServiceTests
{
    private const string RefreshToken = "release-gate-refresh-token";

    [Theory]
    [InlineData(UserStatus.Suspended)]
    [InlineData(UserStatus.Rejected)]
    [InlineData(UserStatus.Deleted)]
    public async Task BlockedAccount_RefreshRequestFails(UserStatus status)
    {
        await using var testContext = await PasswordServiceTestContext.CreateAsync();
        var user = await testContext.CreateUserAsync(status, emailConfirmed: true);
        var storedUser = await testContext.ReloadUserAsync(user.Id);
        var authHelper = new TestAuthHelperService();
        storedUser.RefreshTokens.Single().HashedToken = authHelper.HashRefreshToken(RefreshToken);
        await testContext.DbContext.SaveChangesAsync();
        testContext.DbContext.ChangeTracker.Clear();
        var jwtProvider = new TestJwtProvider();
        var service = new RefreshTokenService(
            testContext.UserManager,
            jwtProvider,
            authHelper);

        await Assert.ThrowsAsync<AuthenticationException>(() =>
            service.GetRefreshTokenAsync(RefreshToken, CancellationToken.None));

        Assert.Equal(0, jwtProvider.GenerateTokenCalls);
        var unchangedUser = await testContext.ReloadUserAsync(user.Id);
        Assert.All(unchangedUser.RefreshTokens, token => Assert.True(token.IsActive));
    }

    private sealed class TestJwtProvider : IJwtProvider
    {
        public int GenerateTokenCalls { get; private set; }

        public TokenResult GenerateToken(ApplicationUser user, IEnumerable<string> roles)
        {
            GenerateTokenCalls++;
            return new TokenResult("access-token", DateTime.UtcNow.AddMinutes(15), 900);
        }

        public string? ValidateToken(string token, bool validateLifetime = true)
            => throw new NotSupportedException();
    }

    private sealed class TestAuthHelperService : IAuthHelperService
    {
        public Task EnsureRoleExistsAsync(string roleName)
            => throw new NotSupportedException();

        public Task SendConfirmationEmailAsync(
            ApplicationUser user,
            CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public string GenerateRefreshToken()
            => throw new NotSupportedException();

        public string HashRefreshToken(string refreshToken)
        {
            var bytes = Encoding.UTF8.GetBytes(refreshToken);
            return Convert.ToBase64String(SHA256.HashData(bytes));
        }

        public void RevokeAllActiveRefreshTokens(ApplicationUser applicationUser)
            => throw new NotSupportedException();
    }
}
