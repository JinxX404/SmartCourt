using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using SmartCourt.Common;
using SmartCourt.Features.Auth.Enums;
using SmartCourt.Interfaces;
using SmartCourt.Interfaces.Providers;
using System.Text;

namespace SmartCourt.Features.Auth;

public class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IEmailProvider _emailProvider;
    private readonly IWebHostEnvironment _env;
    private readonly string _appUrl;

    public AuthService(
        UserManager<ApplicationUser> userManager,
        IEmailProvider emailProvider,
        IConfiguration configuration,
        IWebHostEnvironment env)
    {
        _userManager = userManager;
        _emailProvider = emailProvider;
        _env = env;
        _appUrl = configuration["AppUrl"]?.TrimEnd('/') ?? "http://localhost:5000";
    }

    public async Task ForgotPasswordAsync(string email)
    {
        var user = await _userManager.FindByEmailAsync(email);

        if (user == null)
        {
            return;
        }

        if (!user.EmailConfirmed)
        {
            throw new BusinessException("البريد الإلكتروني غير مؤكد");
        }

        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));
        var resetUrl = $"{_appUrl}/api/auth/reset-password?email={email}&token={encodedToken}";

        var subject = "إعادة تعيين كلمة المرور - المحكمة الذكية";
        var templatePath = Path.Combine(_env.ContentRootPath, "Features", "Auth", "Shared", "Templates", "ResetPasswordEmail.html");
        var template = await File.ReadAllTextAsync(templatePath);
        var body = template.Replace("{{FullName}}", user.FullName)
                           .Replace("{{ResetUrl}}", resetUrl)
                           .Replace("{{Year}}", DateTime.UtcNow.Year.ToString());

        await _emailProvider.SendEmailAsync(user.Email!, subject, body, true);
    }

    public async Task ResetPasswordAsync(string email, string token, string newPassword)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user == null)
        {
            throw new BusinessException("بيانات غير صالحة");
        }

        var decodedToken = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(token));
        var result = await _userManager.ResetPasswordAsync(user, decodedToken, newPassword);
        if (!result.Succeeded)
        {
            throw new ValidationException("Password", string.Join(" ", result.Errors.Select(e => e.Description)));
        }
    }

    public async Task ChangePasswordAsync(string userId, string currentPassword, string newPassword)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user is null)
        {
            throw new AuthenticationException("المستخدم غير معروف");
        }

        var result = await _userManager.ChangePasswordAsync(user, currentPassword, newPassword);
        if (!result.Succeeded)
        {
            throw new ValidationException("Password", string.Join(" ", result.Errors.Select(e => e.Description)));
        }
    }

    public async Task ResendVerificationEmailAsync(string email)
    {
        var user = await _userManager.FindByEmailAsync(email);

        if (user == null)
        {
            return;
        }

        if (user.EmailConfirmed)
        {
            return;
        }

        var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
        var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));
        var confirmationUrl = $"{_appUrl}/api/auth/confirm-email?email={email}&token={encodedToken}";

        var subject = "تأكيد البريد الإلكتروني - المحكمة الذكية";
        var templatePath = Path.Combine(_env.ContentRootPath, "Features", "Auth", "Shared", "Templates", "ResendVerificationEmail.html");
        var template = await File.ReadAllTextAsync(templatePath);
        var body = template.Replace("{{FullName}}", user.FullName)
                           .Replace("{{ConfirmationUrl}}", confirmationUrl)
                           .Replace("{{Year}}", DateTime.UtcNow.Year.ToString());

        await _emailProvider.SendEmailAsync(user.Email!, subject, body, true);
    }
}
