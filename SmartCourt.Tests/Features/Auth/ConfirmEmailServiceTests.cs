using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using SmartCourt.Common.Exceptions;
using SmartCourt.Common.Models;
using SmartCourt.Common.RateLimiting;
using SmartCourt.Features.Auth.ConfirmEmail;
using SmartCourt.Features.Auth.ConfirmEmail.DTOs;
using SmartCourt.Features.Auth.Enums;
using Xunit;

namespace SmartCourt.Tests.Features.Auth;

public sealed class ConfirmEmailServiceTests
{
    private const string InvalidConfirmationMessage =
        "رابط تأكيد البريد الإلكتروني غير صالح أو منتهي الصلاحية.";

    [Fact]
    public async Task ClientConfirmation_TransitionsToActive()
    {
        await using var testContext = await PasswordServiceTestContext.CreateAsync();
        var user = await testContext.CreateUserAsync(
            UserStatus.Unverified,
            emailConfirmed: false,
            role: "Client");
        var token = await testContext.GenerateEncodedEmailConfirmationTokenAsync(user);

        await CreateService(testContext).ConfirmEmailAsync(
            user.Id.ToString(),
            token,
            CancellationToken.None);

        var storedUser = await testContext.ReloadUserAsync(user.Id);
        Assert.True(storedUser.EmailConfirmed);
        Assert.Equal(UserStatus.Active, storedUser.Status);
    }

    [Fact]
    public async Task LawyerConfirmation_TransitionsToPendingReview()
    {
        await using var testContext = await PasswordServiceTestContext.CreateAsync();
        var user = await testContext.CreateUserAsync(
            UserStatus.Unverified,
            emailConfirmed: false,
            role: "Lawyer");
        var token = await testContext.GenerateEncodedEmailConfirmationTokenAsync(user);

        await CreateService(testContext).ConfirmEmailAsync(
            user.Id.ToString(),
            token,
            CancellationToken.None);

        var storedUser = await testContext.ReloadUserAsync(user.Id);
        Assert.True(storedUser.EmailConfirmed);
        Assert.Equal(UserStatus.PendingReview, storedUser.Status);
    }

    [Fact]
    public async Task AlreadyConfirmedExpectedState_IsIdempotent()
    {
        await using var testContext = await PasswordServiceTestContext.CreateAsync();
        var user = await testContext.CreateUserAsync(
            UserStatus.Unverified,
            emailConfirmed: false,
            role: "Client");
        var token = await testContext.GenerateEncodedEmailConfirmationTokenAsync(user);
        var service = CreateService(testContext);

        await service.ConfirmEmailAsync(user.Id.ToString(), token, CancellationToken.None);
        await service.ConfirmEmailAsync(user.Id.ToString(), token, CancellationToken.None);

        var storedUser = await testContext.ReloadUserAsync(user.Id);
        Assert.True(storedUser.EmailConfirmed);
        Assert.Equal(UserStatus.Active, storedUser.Status);
    }

    [Theory]
    [InlineData(UserStatus.Suspended)]
    [InlineData(UserStatus.Rejected)]
    [InlineData(UserStatus.Deleted)]
    public async Task DisallowedState_IsNotReactivated(UserStatus status)
    {
        await using var testContext = await PasswordServiceTestContext.CreateAsync();
        var user = await testContext.CreateUserAsync(
            status,
            emailConfirmed: false,
            role: "Client");
        var token = await testContext.GenerateEncodedEmailConfirmationTokenAsync(user);

        var exception = await Assert.ThrowsAsync<BusinessException>(() =>
            CreateService(testContext).ConfirmEmailAsync(
                user.Id.ToString(),
                token,
                CancellationToken.None));

        Assert.Equal(InvalidConfirmationMessage, exception.Message);
        var storedUser = await testContext.ReloadUserAsync(user.Id);
        Assert.False(storedUser.EmailConfirmed);
        Assert.Equal(status, storedUser.Status);
    }

    [Fact]
    public async Task MalformedMissingAndInvalidInputs_ReturnSameGenericError()
    {
        await using var testContext = await PasswordServiceTestContext.CreateAsync();
        var user = await testContext.CreateUserAsync(
            UserStatus.Unverified,
            emailConfirmed: false,
            role: "Client");
        var service = CreateService(testContext);
        var validUserId = user.Id.ToString();
        var validEncodedToken = WebEncoders.Base64UrlEncode("token"u8.ToArray());

        var malformedId = await Assert.ThrowsAsync<BusinessException>(() =>
            service.ConfirmEmailAsync("not-a-guid", validEncodedToken, CancellationToken.None));
        var missingToken = await Assert.ThrowsAsync<BusinessException>(() =>
            service.ConfirmEmailAsync(validUserId, null!, CancellationToken.None));
        var missingUser = await Assert.ThrowsAsync<BusinessException>(() =>
            service.ConfirmEmailAsync(Guid.NewGuid().ToString(), validEncodedToken, CancellationToken.None));
        var invalidToken = await Assert.ThrowsAsync<BusinessException>(() =>
            service.ConfirmEmailAsync(validUserId, "%%%", CancellationToken.None));

        Assert.Equal(InvalidConfirmationMessage, malformedId.Message);
        Assert.Equal(malformedId.Message, missingToken.Message);
        Assert.Equal(malformedId.Message, missingUser.Message);
        Assert.Equal(malformedId.Message, invalidToken.Message);
    }

    [Fact]
    public async Task InvalidToken_ChangesNothing()
    {
        await using var testContext = await PasswordServiceTestContext.CreateAsync();
        var user = await testContext.CreateUserAsync(
            UserStatus.Unverified,
            emailConfirmed: false,
            role: "Client");

        await Assert.ThrowsAsync<BusinessException>(() =>
            CreateService(testContext).ConfirmEmailAsync(
                user.Id.ToString(),
                WebEncoders.Base64UrlEncode("invalid"u8.ToArray()),
                CancellationToken.None));

        var storedUser = await testContext.ReloadUserAsync(user.Id);
        Assert.False(storedUser.EmailConfirmed);
        Assert.Equal(UserStatus.Unverified, storedUser.Status);
    }

    [Fact]
    public async Task ExpiredToken_ReturnsGenericError()
    {
        await using var testContext = await PasswordServiceTestContext.CreateAsync(TimeSpan.Zero);
        var user = await testContext.CreateUserAsync(
            UserStatus.Unverified,
            emailConfirmed: false,
            role: "Client");
        var token = await testContext.GenerateEncodedEmailConfirmationTokenAsync(user);

        var exception = await Assert.ThrowsAsync<BusinessException>(() =>
            CreateService(testContext).ConfirmEmailAsync(
                user.Id.ToString(),
                token,
                CancellationToken.None));

        Assert.Equal(InvalidConfirmationMessage, exception.Message);
    }

    [Fact]
    public async Task FinalUpdateFailure_RollsBackConfirmation()
    {
        await using var testContext = await PasswordServiceTestContext.CreateAsync();
        var user = await testContext.CreateUserAsync(
            UserStatus.Unverified,
            emailConfirmed: false,
            role: "Client");
        var token = await testContext.GenerateEncodedEmailConfirmationTokenAsync(user);
        testContext.UserManager.FailExplicitUpdate = true;

        await Assert.ThrowsAsync<BusinessException>(() =>
            CreateService(testContext).ConfirmEmailAsync(
                user.Id.ToString(),
                token,
                CancellationToken.None));

        var storedUser = await testContext.ReloadUserAsync(user.Id);
        Assert.False(storedUser.EmailConfirmed);
        Assert.Equal(UserStatus.Unverified, storedUser.Status);
    }

    [Fact]
    public async Task ControllerUsesGenericMessageOnlyResponse()
    {
        var controller = new ConfirmEmailController(
            new NoOpConfirmEmailService(),
            new NoOpAccountKeyRateLimiter());

        var result = await controller.Get(new VerifyEmailRequest { UserId = "bad-id", Token = "bad-token" }, CancellationToken.None);

        var value = Assert.IsType<OkObjectResult>(result).Value;
        var response = Assert.IsType<ApiResponse>(value);
        Assert.True(response.Success);
        Assert.False(string.IsNullOrWhiteSpace(response.Message));
    }

    [Fact]
    public void ConfirmEmailChangeRoute_IsNotExposed()
    {
        var routes = typeof(ConfirmEmailController)
            .GetMethods()
            .SelectMany(method => method
                .GetCustomAttributes(typeof(HttpGetAttribute), inherit: true)
                .Cast<HttpGetAttribute>())
            .Select(attribute => attribute.Template);

        Assert.DoesNotContain("/api/auth/confirm-email-change", routes);
    }

    private static ConfirmEmailService CreateService(PasswordServiceTestContext testContext)
        => new(testContext.UserManager, testContext.DbContext);

    private sealed class NoOpConfirmEmailService : IConfirmEmailService
    {
        public Task ConfirmEmailAsync(string userId, string token, CancellationToken cancellationToken = default)
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
