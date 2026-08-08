using System.Security.Claims;
using SmartCourt.Common.Extensions;
using SmartCourt.Features.Auth.Enums;
using Xunit;

namespace SmartCourt.Tests.Common.Extensions;

public sealed class ApplicationUserExtensionsTests
{
    [Theory]
    [InlineData(UserStatus.Active, true)]
    [InlineData(UserStatus.PendingReview, true)]
    [InlineData(UserStatus.Unverified, true)]
    [InlineData(UserStatus.Suspended, false)]
    [InlineData(UserStatus.Rejected, true)]
    [InlineData(UserStatus.Deleted, false)]
    public void IsAccessEligible_ReturnsExpectedResult(UserStatus status, bool expected)
    {
        var user = new ApplicationUser
        {
            EmailConfirmed = true,
            Status = status
        };

        var result = user.IsAccessEligible();

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

        var result = user.IsAccessEligible();

        Assert.False(result);
    }

    [Fact]
    public void HasValidSecurityStamp_ReturnsTrue_WhenStampMatches()
    {
        var user = new ApplicationUser { SecurityStamp = "current-stamp" };
        var principal = CreatePrincipal("current-stamp");

        var result = user.HasValidSecurityStamp(principal);

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

        var result = user.HasValidSecurityStamp(principal);

        Assert.False(result);
    }

    private static ClaimsPrincipal CreatePrincipal(string? securityStamp)
    {
        var claims = new List<Claim>();
        if (securityStamp is not null)
        {
            claims.Add(new Claim(ApplicationUserExtensions.SecurityStampClaimType, securityStamp));
        }

        return new ClaimsPrincipal(new ClaimsIdentity(claims));
    }
}
