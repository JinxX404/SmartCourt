using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using SmartCourt.Common.Entities;
using SmartCourt.Common.Exceptions;
using SmartCourt.Interfaces.Providers;
using System.Text;

namespace SmartCourt.Features.Auth.ForgotPassword;

public class ForgotPasswordService(
    UserManager<ApplicationUser> userManager,
    IEmailProvider emailProvider,
    IConfiguration configuration,
    IWebHostEnvironment env) : IForgotPasswordService
{
    private readonly string _appUrl = configuration["AppUrl"]?.TrimEnd('/') ?? "http://localhost:5000";

    public async Task ForgotPasswordAsync(string email, CancellationToken cancellationToken = default)
    {
        var user = await userManager.FindByEmailAsync(email);

        if (user == null)
        {
            return;
        }

        if (!user.EmailConfirmed)
        {
            return;
        }

        var token = await userManager.GeneratePasswordResetTokenAsync(user);
        var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));
        var resetUrl = $"{_appUrl}/auth/reset-password?email={email}&token={encodedToken}";

        var subject = "إعادة تعيين كلمة المرور - المحكمة الذكية";
        var templatePath = Path.Combine(env.ContentRootPath, "Features", "Auth", "Shared", "Templates", "ResetPasswordEmail.html");
        var template = await File.ReadAllTextAsync(templatePath, cancellationToken);
        var body = template.Replace("{{FullName}}", user.FullName)
                           .Replace("{{ResetUrl}}", resetUrl)
                           .Replace("{{Year}}", DateTime.UtcNow.Year.ToString());

        await emailProvider.SendEmailAsync(user.Email!, subject, body, true);
    }
}
