using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using SmartCourt.Common.Entities;
using SmartCourt.Common.Exceptions;
using System.Text;

namespace SmartCourt.Features.Auth.ResetPassword;

public class ResetPasswordService(UserManager<ApplicationUser> userManager) : IResetPasswordService
{
    public async Task ResetPasswordAsync(string email, string token, string newPassword)
    {
        var user = await userManager.FindByEmailAsync(email);
        if (user == null)
        {
            throw new BusinessException("بيانات غير صالحة");
        }

        var decodedToken = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(token));
        var result = await userManager.ResetPasswordAsync(user, decodedToken, newPassword);
        if (!result.Succeeded)
        {
            throw new ValidationException("Password", string.Join(" ", result.Errors.Select(e => e.Description)));
        }
    }
}
