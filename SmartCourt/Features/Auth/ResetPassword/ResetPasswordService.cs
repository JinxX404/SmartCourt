using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using SmartCourt.Common.Entities;
using SmartCourt.Common.Exceptions;
using SmartCourt.Features.Auth.Shared;
using System.Text;

namespace SmartCourt.Features.Auth.ResetPassword;

public class ResetPasswordService(UserManager<ApplicationUser> userManager, IAuthHelperService authHelperService) : IResetPasswordService
{
    public async Task ResetPasswordAsync(string email, string token, string newPassword, CancellationToken cancellationToken)
    {
        var user = await userManager.Users
            .Include(u => u.RefreshTokens)
            .SingleOrDefaultAsync(u => u.Email == email, cancellationToken);

        if (user == null)
        {
            throw new BusinessException("بيانات غير صالحة");
        }

        string decodedToken;
        try
        {
            decodedToken = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(token));
        }
        catch (FormatException)
        {
            throw new ValidationException("Token", "رمز غير صالح أو تالف");
        }

        var result = await userManager.ResetPasswordAsync(user, decodedToken, newPassword);
        if (!result.Succeeded)
        {
            var errors = result.Errors.Select(e => e.Description).ToArray();
            var failures = new List<KeyValuePair<string, string[]>>
            {
                new("NewPassword", errors)
            };
            throw new ValidationException(failures);
        }

        authHelperService.RevokeAllActiveRefreshTokens(user);
        await userManager.UpdateAsync(user);
    }
}
