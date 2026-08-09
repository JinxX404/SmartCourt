using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;
using SmartCourt.Interfaces.Providers;
using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;

namespace SmartCourt.Features.Auth.Shared;

public class AuthHelperService : IAuthHelperService
{
    private readonly RoleManager<IdentityRole<Guid>> _roleManager;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IEmailProvider _emailProvider;
    private readonly string _publicBaseUrl;
    private readonly IWebHostEnvironment _env;

    public AuthHelperService(
        RoleManager<IdentityRole<Guid>> roleManager,
        UserManager<ApplicationUser> userManager,
        IEmailProvider emailProvider,
        IOptions<AuthEmailOptions> options,
        IWebHostEnvironment env)
    {
        _roleManager = roleManager;
        _userManager = userManager;
        _emailProvider = emailProvider;
        _publicBaseUrl = options.Value.PublicBaseUrl.TrimEnd('/');
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
        foreach (var token in activeTokens)
        {
            token.RevokedOn = DateTime.UtcNow;
        }

    }

    public async Task SendConfirmationEmailAsync(ApplicationUser user, CancellationToken cancellationToken = default)
    {
        // Rotate the SecurityStamp so any previously issued confirmation tokens
        // are immediately invalidated. Only the link in the most recent email will work.
        await _userManager.UpdateSecurityStampAsync(user);

        var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
        var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));
        var confirmationUrl = QueryHelpers.AddQueryString(
            $"{_publicBaseUrl}/verify-email",
            new Dictionary<string, string?>
            {
                ["userId"] = user.Id.ToString(),
                ["token"] = encodedToken
            });

        var subject = "تأكيد البريد الإلكتروني - المحكمة الذكية";
        var templatePath = Path.Combine(_env.ContentRootPath, "Features", "Auth", "Shared", "Templates", "ConfirmationEmail.html");
        var template = await File.ReadAllTextAsync(templatePath, cancellationToken);
        var body = template.Replace("{{FullName}}", HtmlEncoder.Default.Encode(user.FullName))
                           .Replace("{{ConfirmationUrl}}", HtmlEncoder.Default.Encode(confirmationUrl))
                           .Replace("{{Year}}", DateTime.UtcNow.Year.ToString());

        if (!await _emailProvider.SendEmailAsync(user.Email!, subject, body, true, cancellationToken))
        {
            throw new InvalidOperationException("Confirmation email could not be queued.");
        }
    }

    public string HashRefreshToken(string refreshToken)
    {
        using var sha256 = SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes(refreshToken);
        var hash = sha256.ComputeHash(bytes);
        return Convert.ToBase64String(hash);
    }
}
