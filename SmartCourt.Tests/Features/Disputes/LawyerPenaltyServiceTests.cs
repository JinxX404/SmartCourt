using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SmartCourt.Common.Exceptions;
using SmartCourt.Features.Auth.Enums;
using SmartCourt.Features.Disputes.DTOs;
using SmartCourt.Features.Disputes.Entities;
using SmartCourt.Features.Disputes.Enums;
using SmartCourt.Features.Disputes.Penalties;
using SmartCourt.Features.Users.Integration;
using SmartCourt.Interfaces;
using SmartCourt.Persistence;
using Xunit;

namespace SmartCourt.Tests.Features.Disputes;

public sealed class LawyerPenaltyServiceTests
{
    private static readonly DateTime Now = new(2026, 8, 1, 10, 0, 0, DateTimeKind.Utc);

    [Fact]
    public async Task ListAsync_ReturnsPenaltiesForModerator()
    {
        await using var context = CreateContext();
        var modId = Guid.NewGuid();
        var lawyerId = Guid.NewGuid();
        await AddUserWithRoleAsync(context, modId, "Moderator");
        await AddUserWithRoleAsync(context, lawyerId, "Lawyer");

        var penalty = new LawyerPenalty(
            Guid.NewGuid(),
            lawyerId,
            Guid.NewGuid(),
            PenaltyType.Suspension12Months,
            "إخلال جسيم",
            Now,
            Now.AddMonths(12),
            modId,
            Now);
        context.LawyerPenalties.Add(penalty);
        await context.SaveChangesAsync();

        var timeProvider = new FixedTimeProvider(Now);
        var eligibilityService = new ContractUserEligibilityService(context, timeProvider);
        var currentUserService = new TestCurrentUserService(modId);
        var service = new LawyerPenaltyService(context, currentUserService, eligibilityService, timeProvider);

        var result = await service.ListAsync(new LawyerPenaltyFilterQuery(), CancellationToken.None);

        Assert.Equal(1, result.TotalCount);
        Assert.Single(result.Items);
        Assert.Equal(penalty.Id, result.Items[0].Id);
        Assert.True(result.Items[0].IsActive);
    }

    [Fact]
    public async Task RevokeAsync_SuperAdminCanRevokePenalty()
    {
        await using var context = CreateContext();
        var superAdminId = Guid.NewGuid();
        var lawyerId = Guid.NewGuid();
        await AddUserWithRoleAsync(context, superAdminId, "SuperAdministrator");
        await AddUserWithRoleAsync(context, lawyerId, "Lawyer");

        var penalty = new LawyerPenalty(
            Guid.NewGuid(),
            lawyerId,
            Guid.NewGuid(),
            PenaltyType.Suspension24Months,
            "إخلال جسيم",
            Now,
            Now.AddMonths(24),
            superAdminId,
            Now);
        context.LawyerPenalties.Add(penalty);
        await context.SaveChangesAsync();

        var timeProvider = new FixedTimeProvider(Now);
        var eligibilityService = new ContractUserEligibilityService(context, timeProvider);
        var currentUserService = new TestCurrentUserService(superAdminId);
        var service = new LawyerPenaltyService(context, currentUserService, eligibilityService, timeProvider);

        var revoked = await service.RevokeAsync(
            penalty.Id,
            new RevokeLawyerPenaltyRequest("تم قبول الالتماس وإلغاء العقوبة بعد التحقيق الإضافي."),
            CancellationToken.None);

        Assert.True(revoked.IsRevoked);
        Assert.False(revoked.IsActive);
        Assert.Equal("تم قبول الالتماس وإلغاء العقوبة بعد التحقيق الإضافي.", revoked.RevocationReason);
        Assert.Equal(superAdminId, revoked.RevokedByUserId);

        // Verify eligibility service reflects revocation
        var eligibility = await eligibilityService.FindEligibilityAsync(lawyerId, CancellationToken.None);
        Assert.NotNull(eligibility);
        Assert.True(eligibility.CanActAsLawyer);
    }

    [Fact]
    public async Task EligibilityService_SuppressesCanActAsLawyer_WhenActivePenaltyExists()
    {
        await using var context = CreateContext();
        var lawyerId = Guid.NewGuid();
        await AddUserWithRoleAsync(context, lawyerId, "Lawyer");

        var timeProvider = new FixedTimeProvider(Now);
        var eligibilityService = new ContractUserEligibilityService(context, timeProvider);

        // Before penalty
        var before = await eligibilityService.FindEligibilityAsync(lawyerId, CancellationToken.None);
        Assert.NotNull(before);
        Assert.True(before.CanActAsLawyer);

        // Add active penalty
        var penalty = new LawyerPenalty(
            Guid.NewGuid(),
            lawyerId,
            Guid.NewGuid(),
            PenaltyType.Suspension12Months,
            "سوء سلوك مهني",
            Now,
            Now.AddMonths(12),
            Guid.NewGuid(),
            Now);
        context.LawyerPenalties.Add(penalty);
        await context.SaveChangesAsync();

        // While penalty is active
        var during = await eligibilityService.FindEligibilityAsync(lawyerId, CancellationToken.None);
        Assert.NotNull(during);
        Assert.False(during.CanActAsLawyer);

        // After penalty expires
        var expiredTimeProvider = new FixedTimeProvider(Now.AddMonths(13));
        var expiredEligibilityService = new ContractUserEligibilityService(context, expiredTimeProvider);
        var after = await expiredEligibilityService.FindEligibilityAsync(lawyerId, CancellationToken.None);
        Assert.NotNull(after);
        Assert.True(after.CanActAsLawyer);
    }

    private static ApplicationDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new ApplicationDbContext(options, new FixedTimeProvider(Now));
    }

    private static async Task AddUserWithRoleAsync(ApplicationDbContext context, Guid userId, string roleName)
    {
        var user = new ApplicationUser
        {
            Id = userId,
            UserName = $"user_{userId:N}",
            Email = $"user_{userId:N}@example.test",
            FullName = $"User {userId:N}",
            Status = UserStatus.Active
        };
        var role = new IdentityRole<Guid>
        {
            Id = Guid.NewGuid(),
            Name = roleName,
            NormalizedName = roleName.ToUpperInvariant()
        };
        context.Users.Add(user);
        context.Roles.Add(role);
        context.UserRoles.Add(new IdentityUserRole<Guid>
        {
            UserId = userId,
            RoleId = role.Id
        });
        await context.SaveChangesAsync();
    }

    private sealed class FixedTimeProvider(DateTime value) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => new(value);
    }

    private sealed class TestCurrentUserService(Guid userId) : ICurrentUserService
    {
        public Guid? UserId => userId;
        public bool IsAuthenticated => true;
    }
}
