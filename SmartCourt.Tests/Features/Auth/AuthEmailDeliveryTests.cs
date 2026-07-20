using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using SmartCourt.Features.Auth.Enums;
using SmartCourt.Features.Auth.Shared;
using SmartCourt.Interfaces.Providers;
using SmartCourt.Providers.Email;
using System.Net;
using Xunit;

namespace SmartCourt.Tests.Features.Auth;

public sealed class AuthEmailDeliveryTests
{
    [Fact]
    public async Task ConfirmationEmailUsesEncodedHttpsLinkAndHtmlEncodedName()
    {
        await using var testContext = await PasswordServiceTestContext.CreateAsync();
        var user = await testContext.CreateUserAsync(
            UserStatus.Unverified,
            emailConfirmed: false,
            fullName: "<strong>Test User</strong>");
        var emailProvider = new CapturingEmailProvider();
        var service = new AuthHelperService(
            testContext.RoleManager,
            testContext.UserManager,
            emailProvider,
            Options.Create(new AuthEmailOptions
            {
                PublicBaseUrl = "https://app.example.com"
            }),
            new TestWebHostEnvironment());

        await service.SendConfirmationEmailAsync(user, CancellationToken.None);

        var message = Assert.Single(emailProvider.Messages);
        Assert.Contains("&lt;strong&gt;Test User&lt;/strong&gt;", message.Body);
        Assert.DoesNotContain("<strong>Test User</strong>", message.Body);

        var url = ExtractHtmlLink(message.Body);
        var uri = new Uri(url);
        var query = QueryHelpers.ParseQuery(uri.Query);

        Assert.Equal(Uri.UriSchemeHttps, uri.Scheme);
        Assert.Equal("app.example.com", uri.Host);
        Assert.Equal(user.Id.ToString(), query["userId"].ToString());
        Assert.False(string.IsNullOrWhiteSpace(query["token"].ToString()));
    }

    [Fact]
    public async Task EmailQueueFailureThrows()
    {
        await using var testContext = await PasswordServiceTestContext.CreateAsync();
        var user = await testContext.CreateUserAsync(
            UserStatus.Unverified,
            emailConfirmed: false);
        var service = new AuthHelperService(
            testContext.RoleManager,
            testContext.UserManager,
            new CapturingEmailProvider(succeeds: false),
            Options.Create(new AuthEmailOptions
            {
                PublicBaseUrl = "https://app.example.com"
            }),
            new TestWebHostEnvironment());

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.SendConfirmationEmailAsync(user, CancellationToken.None));
    }

    [Fact]
    public async Task SmtpFailureThrowsForHangfireRetry()
    {
        var sender = new SmtpEmailSender(
            Options.Create(new MailKitOptions
            {
                Server = "127.0.0.1",
                Port = 1,
                SenderName = "Smart Court",
                SenderEmail = "noreply@example.com",
                Username = "noreply@example.com",
                Password = "password",
                UseSsl = false
            }),
            NullLogger<SmtpEmailSender>.Instance);

        await Assert.ThrowsAnyAsync<Exception>(() =>
            sender.SendEmailAsync(
                "user@example.com",
                "Subject",
                "Body",
                isHtml: false));
    }

    private static string ExtractHtmlLink(string body)
    {
        var hrefStart = body.IndexOf("href='", StringComparison.Ordinal) + "href='".Length;
        var hrefEnd = body.IndexOf("'", hrefStart, StringComparison.Ordinal);
        return WebUtility.HtmlDecode(body[hrefStart..hrefEnd]);
    }

    private sealed class CapturingEmailProvider(bool succeeds = true) : IEmailProvider
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
            return Task.FromResult(succeeds);
        }
    }

    private sealed record EmailMessage(string To, string Subject, string Body, bool IsHtml);
}
