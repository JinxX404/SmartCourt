using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using SmartCourt.Common.Entities;
using SmartCourt.Interfaces.Providers;
using System.Text;

namespace SmartCourt.Features.Auth.ResendVerification;

public class ResendVerificationService(
    UserManager<ApplicationUser> userManager,
    IEmailProvider emailProvider,
    IConfiguration configuration,
    IWebHostEnvironment env) : IResendVerificationService
{
    private readonly string _appUrl = configuration["AppUrl"]?.TrimEnd('/') ?? "http://localhost:5000";

    public async Task ResendVerificationEmailAsync(string email)
    {
        var user = await userManager.FindByEmailAsync(email);

        if (user == null)
        {
            return;
        }

        if (user.EmailConfirmed)
        {
            return;
        }

        var token = await userManager.GenerateEmailConfirmationTokenAsync(user);
        var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));
        var confirmationUrl = $"{_appUrl}/api/auth/confirm-email?email={email}&token={encodedToken}";

        var subject = "تأكيد البريد الإلكتروني - المحكمة الذكية";
        var templatePath = Path.Combine(env.ContentRootPath, "Features", "Auth", "Shared", "Templates", "ResendVerificationEmail.html");
        var template = await File.ReadAllTextAsync(templatePath);
        var body = template.Replace("{{FullName}}", user.FullName)
                           .Replace("{{ConfirmationUrl}}", confirmationUrl)
                           .Replace("{{Year}}", DateTime.UtcNow.Year.ToString());

        await emailProvider.SendEmailAsync(user.Email!, subject, body, true);
    }
}
