using Microsoft.AspNetCore.Mvc;
using SmartCourt.Common.Models;
using SmartCourt.Common.RateLimiting;
using SmartCourt.Features.Auth.Enums;
using SmartCourt.Features.Auth.ResendVerification;
using SmartCourt.Features.Auth.ResendVerification.DTOs;
using SmartCourt.Features.Auth.Shared;
using Xunit;

namespace SmartCourt.Tests.Features.Auth;

public sealed class ResendVerificationServiceTests
{
    [Theory]
    [InlineData(UserStatus.Unverified, false, true)]
    [InlineData(UserStatus.Unverified, true, false)]
    [InlineData(UserStatus.Active, false, false)]
    [InlineData(UserStatus.PendingReview, false, false)]
    [InlineData(UserStatus.Suspended, false, false)]
    [InlineData(UserStatus.Rejected, false, false)]
    [InlineData(UserStatus.Deleted, false, false)]
    public async Task OnlyUnverifiedAccountsReceiveVerificationEmail(
        UserStatus status,
        bool emailConfirmed,
        bool shouldSend)
    {
        await using var testContext = await PasswordServiceTestContext.CreateAsync();
        var user = await testContext.CreateUserAsync(status, emailConfirmed);
        var authHelper = new CapturingAuthHelperService();
        var service = new ResendVerificationService(testContext.UserManager, authHelper);

        await service.ResendVerificationEmailAsync(user.Email!, CancellationToken.None);

        Assert.Equal(shouldSend ? 1 : 0, authHelper.ConfirmationRequests.Count);
    }

    [Fact]
    public async Task LookupUsesIdentityNormalization()
    {
        await using var testContext = await PasswordServiceTestContext.CreateAsync();
        var user = await testContext.CreateUserAsync(UserStatus.Unverified, emailConfirmed: false);
        var authHelper = new CapturingAuthHelperService();
        var service = new ResendVerificationService(testContext.UserManager, authHelper);

        await service.ResendVerificationEmailAsync(user.Email!.ToUpperInvariant(), CancellationToken.None);

        Assert.Single(authHelper.ConfirmationRequests);
    }

    [Fact]
    public async Task MissingEmailDoesNotQueueOrThrow()
    {
        await using var testContext = await PasswordServiceTestContext.CreateAsync();
        var authHelper = new CapturingAuthHelperService();
        var service = new ResendVerificationService(testContext.UserManager, authHelper);

        var exception = await Record.ExceptionAsync(() =>
            service.ResendVerificationEmailAsync("missing@example.com", CancellationToken.None));

        Assert.Null(exception);
        Assert.Empty(authHelper.ConfirmationRequests);
    }

    [Fact]
    public async Task ControllerReturnsGenericMessageOnlyResponse()
    {
        var controller = new ResendVerificationController(
            new NoOpResendVerificationService(),
            new NoOpAccountKeyRateLimiter());

        var result = await controller.HandleAsync(
            new ResendVerificationRequest("user@example.com"),
            CancellationToken.None);

        var value = Assert.IsType<OkObjectResult>(result).Value;
        var response = Assert.IsType<ApiResponse>(value);
        Assert.True(response.Success);
        Assert.False(string.IsNullOrWhiteSpace(response.Message));
        Assert.IsNotType<ApiResponse<string>>(value);
    }

    private sealed class CapturingAuthHelperService : IAuthHelperService
    {
        public List<ApplicationUser> ConfirmationRequests { get; } = [];

        public Task EnsureRoleExistsAsync(string roleName)
            => throw new NotSupportedException();

        public Task SendConfirmationEmailAsync(
            ApplicationUser user,
            CancellationToken cancellationToken = default)
        {
            ConfirmationRequests.Add(user);
            return Task.CompletedTask;
        }

        public Task SendChangeEmailConfirmationAsync(
            ApplicationUser user,
            string newEmail,
            CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public string GenerateRefreshToken()
            => throw new NotSupportedException();

        public string HashRefreshToken(string refreshToken)
            => throw new NotSupportedException();

        public void RevokeAllActiveRefreshTokens(ApplicationUser applicationUser)
            => throw new NotSupportedException();
    }

    private sealed class NoOpResendVerificationService : IResendVerificationService
    {
        public Task ResendVerificationEmailAsync(string email, CancellationToken cancellationToken = default)
            => Task.CompletedTask;
    }

    private sealed class NoOpAccountKeyRateLimiter : IAccountKeyRateLimiter
    {
        public void CheckForgotPassword(string email) { }
        public void CheckResendVerification(string email) { }
        public void CheckResetPassword(string email, string token) { }
        public void CheckConfirmEmail(string userId) { }
    }
}
