using Microsoft.AspNetCore.Identity;
using SmartCourt.Common.Entities;
using SmartCourt.Common.Exceptions;

namespace SmartCourt.Features.Auth.ChangePassword;

public class ChangePasswordService(UserManager<ApplicationUser> userManager) : IChangePasswordService
{
    public async Task ChangePasswordAsync(string userId, string currentPassword, string newPassword)
    {
        var user = await userManager.FindByIdAsync(userId);
        if (user is null)
        {
            throw new AuthenticationException("المستخدم غير معروف");
        }

        var result = await userManager.ChangePasswordAsync(user, currentPassword, newPassword);
        if (!result.Succeeded)
        {
            throw new ValidationException("Password", string.Join(" ", result.Errors.Select(e => e.Description)));
        }
    }
}
