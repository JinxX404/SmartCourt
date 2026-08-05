using SmartCourt.Common.Entities;
using SmartCourt.Common.Enums;
using SmartCourt.Common.Exceptions;
using SmartCourt.Features.Auth.Enums;
using SmartCourt.Features.Auth.Shared;
using SmartCourt.Features.Users.Lawyers;
using SmartCourt.Features.Users.Lawyers.DTOs;
using SmartCourt.Interfaces;
using SmartCourt.Tests.Features.Auth;
using Xunit;

namespace SmartCourt.Tests.Features.Users.Lawyers;

public sealed class LawyerServiceReleaseTests
{
    [Fact]
    public async Task PublicProfile_ClientReturnsNotFound()
    {
        await using var testContext = await PasswordServiceTestContext.CreateAsync();
        var client = await testContext.CreateUserAsync();
        var service = CreateService(testContext, client.Id);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            service.GetPublicProfileAsync(client.Id, CancellationToken.None));
    }

    [Fact]
    public async Task PublicProfile_NonActiveLawyerReturnsNotFound()
    {
        await using var testContext = await PasswordServiceTestContext.CreateAsync();
        var lawyer = await testContext.CreateUserAsync(
            UserStatus.PendingReview,
            emailConfirmed: true);
        await AddLawyerProfileAsync(testContext, lawyer.Id);
        var service = CreateService(testContext, lawyer.Id);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            service.GetPublicProfileAsync(lawyer.Id, CancellationToken.None));
    }

    [Fact]
    public async Task PublicProfile_ActiveConfirmedLawyerSucceeds()
    {
        await using var testContext = await PasswordServiceTestContext.CreateAsync();
        var lawyer = await testContext.CreateUserAsync(
            UserStatus.Active,
            emailConfirmed: true);
        await AddLawyerProfileAsync(testContext, lawyer.Id);
        var service = CreateService(testContext, lawyer.Id);

        var response = await service.GetPublicProfileAsync(
            lawyer.Id,
            CancellationToken.None);

        Assert.Equal(lawyer.Id, response.Id);
    }

    [Fact]
    public async Task UpdateProfile_InvalidEnumFailsValidation()
    {
        await using var testContext = await PasswordServiceTestContext.CreateAsync();
        var lawyer = await testContext.CreateUserAsync();
        var request = CreateUpdateRequest();
        request.Level = (LawyerLevel)127;
        var service = CreateService(testContext, lawyer.Id);

        var exception = await Assert.ThrowsAsync<ValidationException>(() =>
            service.UpdateProfileAsync(request, CancellationToken.None));

        Assert.Contains(nameof(request.Level), exception.Errors.Keys);
    }

    private static LawyerService CreateService(
        PasswordServiceTestContext testContext,
        Guid userId)
    {
        return new LawyerService(
            testContext.UserManager,
            testContext.DbContext,
            new TestCurrentUserService(userId),
            new TestAuthHelperService());
    }

    private static async Task AddLawyerProfileAsync(
        PasswordServiceTestContext testContext,
        Guid userId)
    {
        var user = await testContext.ReloadUserAsync(userId);
        user.LawyerProfile = new LawyerProfile
        {
            UserId = userId,
            Level = LawyerLevel.GeneralRegistration,
            IsAvailable = true
        };
        await testContext.DbContext.SaveChangesAsync();
        testContext.DbContext.ChangeTracker.Clear();
    }

    private static UpdateLawyerProfileRequest CreateUpdateRequest()
    {
        return new UpdateLawyerProfileRequest
        {
            PhoneNumber = "+201012345678",
            Level = LawyerLevel.GeneralRegistration,
            Address = "Cairo"
        };
    }

    private sealed class TestCurrentUserService(Guid userId) : ICurrentUserService
    {
        public Guid? UserId { get; } = userId;
        public bool IsAuthenticated => true;
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
            => throw new NotSupportedException();

        public void RevokeAllActiveRefreshTokens(ApplicationUser applicationUser)
            => throw new NotSupportedException();
    }
}
