using Microsoft.AspNetCore.WebUtilities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using SmartCourt.Common.Models;
using SmartCourt.Common.RateLimiting;
using SmartCourt.Features.Auth.Enums;
using SmartCourt.Features.Auth.ForgotPassword;
using SmartCourt.Features.Auth.ForgotPassword.DTOs;
using SmartCourt.Features.Auth.Shared;
using SmartCourt.Interfaces.Providers;
using System.Net;
using Xunit;

namespace SmartCourt.Tests.Features.Auth;

public sealed class ForgotPasswordServiceTests
{
    [Theory]
    [InlineData(UserStatus.Active, true, true)]
    [InlineData(UserStatus.PendingReview, true, true)]
    [InlineData(UserStatus.Active, false, false)]
    [InlineData(UserStatus.Unverified, true, false)]
    [InlineData(UserStatus.Suspended, true, false)]
    [InlineData(UserStatus.Rejected, true, false)]
    [InlineData(UserStatus.Deleted, true, false)]
    public async Task OnlyConfirmedEligibleUsersReceiveResetEmail(
        UserStatus status,
        bool emailConfirmed,
        bool shouldSend)
    {
        await using var testContext = await PasswordServiceTestContext.CreateAsync();
        var user = await testContext.CreateUserAsync(status, emailConfirmed);
        var emailProvider = new CapturingEmailProvider();
        var service = CreateService(testContext, emailProvider);

        await service.ForgotPasswordAsync(user.Email!, CancellationToken.None);

        Assert.Equal(shouldSend ? 1 : 0, emailProvider.Messages.Count);
    }

    [Fact]
    public async Task LookupUsesIdentityNormalization()
    {
        await using var testContext = await PasswordServiceTestContext.CreateAsync();
        var user = await testContext.CreateUserAsync();
        var emailProvider = new CapturingEmailProvider();
        var service = CreateService(testContext, emailProvider);

        await service.ForgotPasswordAsync(user.Email!.ToUpperInvariant(), CancellationToken.None);

        Assert.Single(emailProvider.Messages);
    }

    [Fact]
    public async Task ResetLinkEncodesPlusAddressAndHtmlEncodesName()
    {
        await using var testContext = await PasswordServiceTestContext.CreateAsync();
        var user = await testContext.CreateUserAsync(
            email: "user+tag@example.com",
            fullName: "<script>alert(1)</script>");
        var emailProvider = new CapturingEmailProvider();
        var service = CreateService(testContext, emailProvider);

        await service.ForgotPasswordAsync(user.Email!, CancellationToken.None);

        var message = Assert.Single(emailProvider.Messages);
        Assert.Contains("&lt;script&gt;alert(1)&lt;/script&gt;", message.Body);
        Assert.DoesNotContain("<script>alert(1)</script>", message.Body);

        var hrefStart = message.Body.IndexOf("href='", StringComparison.Ordinal) + "href='".Length;
        var hrefEnd = message.Body.IndexOf("'", hrefStart, StringComparison.Ordinal);
        var url = WebUtility.HtmlDecode(message.Body[hrefStart..hrefEnd]);
        var query = QueryHelpers.ParseQuery(new Uri(url).Query);

        Assert.Equal(user.Email, query["email"].ToString());
        Assert.False(string.IsNullOrWhiteSpace(query["token"].ToString()));
    }

    [Fact]
    public async Task MissingEmailDoesNotQueueOrThrow()
    {
        await using var testContext = await PasswordServiceTestContext.CreateAsync();
        var emailProvider = new CapturingEmailProvider();
        var service = CreateService(testContext, emailProvider);

        var exception = await Record.ExceptionAsync(() =>
            service.ForgotPasswordAsync("missing@example.com", CancellationToken.None));

        Assert.Null(exception);
        Assert.Empty(emailProvider.Messages);
    }

    [Fact]
    public async Task ControllerReturnsGenericMessageOnlyResponse()
    {
        var controller = new ForgotPasswordController(
            new NoOpForgotPasswordService(),
            new NoOpAccountKeyRateLimiter());

        var result = await controller.HandleAsync(
            new ForgotPasswordRequest("user@example.com"),
            CancellationToken.None);

        var value = Assert.IsType<OkObjectResult>(result).Value;
        var response = Assert.IsType<ApiResponse>(value);
        Assert.True(response.Success);
        Assert.False(string.IsNullOrWhiteSpace(response.Message));
        Assert.IsNotType<ApiResponse<string>>(value);
    }

    private static ForgotPasswordService CreateService(
        PasswordServiceTestContext testContext,
        CapturingEmailProvider emailProvider)
    {
        return new ForgotPasswordService(
            testContext.UserManager,
            emailProvider,
            Options.Create(new AuthEmailOptions { PublicBaseUrl = "https://app.example.com" }),
            new TestWebHostEnvironment());
    }

    private sealed class CapturingEmailProvider : IEmailProvider
    {
        public List<EmailMessage> Messages { get; } = [];

        public Task<bool> SendEmailAsync(
            string to,
            string subject,
            string body,
            bool isHtml = false,
            CancellationToken cancellationToken = default)
        {
            Messages.Add(new EmailMessage(to, subject, body, isHtml));
            return Task.FromResult(true);
        }
    }

    private sealed record EmailMessage(string To, string Subject, string Body, bool IsHtml);

    private sealed class NoOpForgotPasswordService : IForgotPasswordService
    {
        public Task ForgotPasswordAsync(string email, CancellationToken cancellationToken = default)
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
