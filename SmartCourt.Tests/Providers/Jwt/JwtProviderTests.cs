using System.IdentityModel.Tokens.Jwt;
using Microsoft.Extensions.Options;
using SmartCourt.Common.Extensions;
using SmartCourt.Features.Auth.Enums;
using SmartCourt.Providers.Jwt;
using Xunit;

namespace SmartCourt.Tests.Providers.Jwt;

public sealed class JwtProviderTests
{
    [Fact]
    public void GenerateToken_IncludesSecurityStampClaim()
    {
        var provider = CreateProvider();
        var user = CreateUser();

        var result = provider.GenerateToken(user, ["Client"]);
        var token = new JwtSecurityTokenHandler().ReadJwtToken(result.Token);

        Assert.Equal(
            user.SecurityStamp,
            token.Claims.Single(claim => claim.Type == ApplicationUserExtensions.SecurityStampClaimType).Value);
    }

    [Fact]
    public void GenerateToken_ExpiresAfterFifteenMinutes()
    {
        var provider = CreateProvider();
        var user = CreateUser();

        var beforeGeneration = DateTime.UtcNow;
        var result = provider.GenerateToken(user, ["Client"]);

        Assert.InRange(
            result.ExpiresAt,
            beforeGeneration.AddMinutes(15),
            DateTime.UtcNow.AddMinutes(15));
    }

    private static JwtProvider CreateProvider()
    {
        return new JwtProvider(Options.Create(new JwtOptions
        {
            Secret = "test-secret-key-that-is-at-least-32-bytes-long",
            Issuer = "SmartCourt.Tests",
            Audience = "SmartCourt.Tests",
            ExpiresInMinutes = 15
        }));
    }

    private static ApplicationUser CreateUser()
    {
        return new ApplicationUser
        {
            Id = Guid.NewGuid(),
            Email = "client@example.com",
            FullName = "Test Client",
            EmailConfirmed = true,
            Status = UserStatus.Active,
            SecurityStamp = "current-stamp"
        };
    }
}
