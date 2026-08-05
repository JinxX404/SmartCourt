using SmartCourt.Common.Entities;
using SmartCourt.Features.Auth.Enums;
using SmartCourt.Features.Users.Lawyers;
using SmartCourt.Features.Users.Lawyers.DTOs;
using Xunit;

namespace SmartCourt.Tests.Features.Users.Lawyers;

public sealed class LawyerQueryExtensionsTests
{
    [Fact]
    public void WherePublicLawyer_ReturnsConfirmedActiveLawyer()
    {
        var lawyerId = Guid.NewGuid();
        var lawyer = CreateLawyer(lawyerId, UserStatus.Active, emailConfirmed: true);

        var result = new[] { lawyer }.AsQueryable().WherePublicLawyer(lawyerId).SingleOrDefault();

        Assert.Same(lawyer, result);
    }

    [Fact]
    public void WherePublicLawyer_ExcludesClientWithoutLawyerProfile()
    {
        var userId = Guid.NewGuid();
        var client = new ApplicationUser
        {
            Id = userId,
            Status = UserStatus.Active,
            EmailConfirmed = true
        };

        var result = new[] { client }.AsQueryable().WherePublicLawyer(userId).SingleOrDefault();

        Assert.Null(result);
    }

    [Fact]
    public void WherePublicLawyer_ExcludesUnconfirmedLawyer()
    {
        var lawyerId = Guid.NewGuid();
        var lawyer = CreateLawyer(lawyerId, UserStatus.Active, emailConfirmed: false);

        var result = new[] { lawyer }.AsQueryable().WherePublicLawyer(lawyerId).SingleOrDefault();

        Assert.Null(result);
    }

    [Theory]
    [InlineData(UserStatus.Unverified)]
    [InlineData(UserStatus.PendingReview)]
    [InlineData(UserStatus.Suspended)]
    [InlineData(UserStatus.Rejected)]
    [InlineData(UserStatus.Deleted)]
    public void WherePublicLawyer_ExcludesNonActiveLawyer(UserStatus status)
    {
        var lawyerId = Guid.NewGuid();
        var lawyer = CreateLawyer(lawyerId, status, emailConfirmed: true);

        var result = new[] { lawyer }.AsQueryable().WherePublicLawyer(lawyerId).SingleOrDefault();

        Assert.Null(result);
    }

    [Fact]
    public void WherePublicLawyer_ExcludesMismatchedId()
    {
        var lawyer = CreateLawyer(Guid.NewGuid(), UserStatus.Active, emailConfirmed: true);

        var result = new[] { lawyer }.AsQueryable().WherePublicLawyer(Guid.NewGuid()).SingleOrDefault();

        Assert.Null(result);
    }

    [Fact]
    public void PublicResponse_DoesNotExposeInternalStatus()
    {
        Assert.Null(typeof(PublicLawyerProfileResponse).GetProperty("Status"));
    }

    private static ApplicationUser CreateLawyer(
        Guid lawyerId,
        UserStatus status,
        bool emailConfirmed)
    {
        return new ApplicationUser
        {
            Id = lawyerId,
            Status = status,
            EmailConfirmed = emailConfirmed,
            LawyerProfile = new LawyerProfile { UserId = lawyerId }
        };
    }
}
