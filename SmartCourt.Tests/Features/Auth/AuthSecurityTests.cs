using System.Security.Claims;
using SmartCourt.Common;
using SmartCourt.Features.Auth.Enums;
using Xunit;

namespace SmartCourt.Tests.Features.Auth;

public sealed class AuthSecurityTests
{
    [Theory]
    [InlineData(UserStatus.Active, true)]
    [InlineData(UserStatus.PendingReview, true)]
    [InlineData(UserStatus.Unverified, false)]
    [InlineData(UserStatus.Suspended, false)]
    [InlineData(UserStatus.Rejected, false)]
    [InlineData(UserStatus.Deleted, false)]
    public void IsAccessEligible_ReturnsExpectedResult(UserStatus status, bool expected)
    {
        var user = new ApplicationUser
        {
            EmailConfirmed = true,
            Status = status
        };

        var result = AuthSecurity.IsAccessEligible(user);

        Assert.Equal(expected, result);
    }

    [Fact]
    public void IsAccessEligible_ReturnsFalse_WhenEmailIsUnconfirmed()
    {
        var user = new ApplicationUser
        {
            EmailConfirmed = false,
            Status = UserStatus.Active
        };

        var result = AuthSecurity.IsAccessEligible(user);

        Assert.False(result);
    }

    [Fact]
    public void HasValidSecurityStamp_ReturnsTrue_WhenStampMatches()
    {
        var user = new ApplicationUser { SecurityStamp = "current-stamp" };
        var principal = CreatePrincipal("current-stamp");

        var result = AuthSecurity.HasValidSecurityStamp(user, principal);

        Assert.True(result);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("old-stamp")]
    public void HasValidSecurityStamp_ReturnsFalse_WhenStampIsMissingOrDifferent(string? tokenSecurityStamp)
    {
        var user = new ApplicationUser { SecurityStamp = "current-stamp" };
        var principal = CreatePrincipal(tokenSecurityStamp);

        var result = AuthSecurity.HasValidSecurityStamp(user, principal);

        Assert.False(result);
    }

    private static ClaimsPrincipal CreatePrincipal(string? securityStamp)
    {
        var claims = new List<Claim>();
        if (securityStamp is not null)
        {
            claims.Add(new Claim(AuthSecurity.SecurityStampClaimType, securityStamp));
        }

        return new ClaimsPrincipal(new ClaimsIdentity(claims));
    }
}
