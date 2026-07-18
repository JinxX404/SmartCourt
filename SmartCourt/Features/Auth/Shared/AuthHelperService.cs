using SmartCourt.Common.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using SmartCourt.Interfaces.Providers;
using System.Security.Cryptography;
using System.Text;

namespace SmartCourt.Features.Auth.Shared;

public class AuthHelperService : IAuthHelperService
{
    private readonly RoleManager<IdentityRole<Guid>> _roleManager;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IEmailProvider _emailProvider;
    private readonly string _appUrl;
    private readonly IWebHostEnvironment _env;

    public AuthHelperService(
        RoleManager<IdentityRole<Guid>> roleManager,
        UserManager<ApplicationUser> userManager,
        IEmailProvider emailProvider,
        IConfiguration configuration,
        IWebHostEnvironment env)
    {
        _roleManager = roleManager;
        _userManager = userManager;
        _emailProvider = emailProvider;
        _appUrl = configuration["AppUrl"]?.TrimEnd('/') ?? "http://localhost:5000";
        _env = env;
    }

    public async Task EnsureRoleExistsAsync(string roleName)
    {
        if (!await _roleManager.RoleExistsAsync(roleName))
        {
            await _roleManager.CreateAsync(new IdentityRole<Guid>(roleName));
        }
    }

    public string GenerateRefreshToken()
    {
        return Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
    }

    public void RevokeAllActiveRefreshTokens(ApplicationUser applicationUser)
    {
        var activeTokens = applicationUser.RefreshTokens.Where(rt => rt.IsActive).ToList();
        foreach (var token in activeTokens) {
            token.RevokedOn = DateTime.UtcNow;
        }

    }

    public async Task SendConfirmationEmailAsync(ApplicationUser user, CancellationToken cancellationToken = default)
    {
        var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
        var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));
        var confirmationUrl = $"{_appUrl}/api/auth/confirm-email?userId={user.Id}&token={encodedToken}";

        var subject = "تأكيد البريد الإلكتروني - المحكمة الذكية";
        var templatePath = Path.Combine(_env.ContentRootPath, "Features", "Auth", "Shared", "Templates", "ConfirmationEmail.html");
        var template = await File.ReadAllTextAsync(templatePath, cancellationToken);
        var body = template.Replace("{{FullName}}", user.FullName)
                           .Replace("{{ConfirmationUrl}}", confirmationUrl)
                           .Replace("{{Year}}", DateTime.UtcNow.Year.ToString());

        await _emailProvider.SendEmailAsync(user.Email!, subject, body, true, cancellationToken);
    }
}
