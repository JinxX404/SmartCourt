using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;
using SmartCourt.Common.Entities;
using SmartCourt.Common.Extensions;
using SmartCourt.Features.Auth.Shared;
using SmartCourt.Interfaces.Providers;
using System.Text;
using System.Text.Encodings.Web;

namespace SmartCourt.Features.Auth.ForgotPassword;

public class ForgotPasswordService(
    UserManager<ApplicationUser> userManager,
    IEmailProvider emailProvider,
    IOptions<AuthEmailOptions> options,
    IWebHostEnvironment env) : IForgotPasswordService
{
    private readonly string _publicBaseUrl = options.Value.PublicBaseUrl.TrimEnd('/');

    public async Task ForgotPasswordAsync(string email, CancellationToken cancellationToken = default)
    {
        var user = await userManager.FindByEmailAsync(email);

        if (user is null || !user.IsAccessEligible())
        {
            return;
        }

        var token = await userManager.GeneratePasswordResetTokenAsync(user);
        var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));
        var resetUrl = QueryHelpers.AddQueryString(
            $"{_publicBaseUrl}/auth/reset-password",
            new Dictionary<string, string?>
            {
                ["email"] = user.Email,
                ["token"] = encodedToken
            });

        var subject = "إعادة تعيين كلمة المرور - مستشار";
        var templatePath = Path.Combine(env.ContentRootPath, "Features", "Auth", "Shared", "Templates", "ResetPasswordEmail.html");
        var template = await File.ReadAllTextAsync(templatePath, cancellationToken);
        var body = template.Replace("{{FullName}}", HtmlEncoder.Default.Encode(user.FullName))
                           .Replace("{{ResetUrl}}", HtmlEncoder.Default.Encode(resetUrl))
                           .Replace("{{Year}}", DateTimeOffset.UtcNow.Year.ToString());

        if (!await emailProvider.SendEmailAsync(user.Email!, subject, body, true, cancellationToken))
        {
            throw new InvalidOperationException("Password reset email could not be queued.");
        }
    }
}
