using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SmartCourt.Common.Entities;
using SmartCourt.Common.Exceptions;
using SmartCourt.Features.Auth.Shared;
using SmartCourt.Interfaces;

namespace SmartCourt.Features.Auth.ChangePassword;

public class ChangePasswordService(UserManager<ApplicationUser> userManager, IAuthHelperService authHelperService, ICurrentUserService CurrentUserService) : IChangePasswordService
{
    public async Task ChangePasswordAsync(string currentPassword, string newPassword, CancellationToken cancellationToken)
    {
        var userId = CurrentUserService.UserId;
        
        var user = await userManager.Users
            .Include(u => u.RefreshTokens)
            .SingleOrDefaultAsync(u => u.Id == userId, cancellationToken);

        if (user is null)
        {
            throw new AuthenticationException("المستخدم غير معروف");
        }

        var result = await userManager.ChangePasswordAsync(user, currentPassword, newPassword);
        if (!result.Succeeded)
        {
            var failures = new List<KeyValuePair<string, string[]>>();
            
            var passwordMismatchError = result.Errors.FirstOrDefault(e => e.Code == "PasswordMismatch");
            if (passwordMismatchError != null)
            {
                failures.Add(new KeyValuePair<string, string[]>("CurrentPassword", new[] { "كلمة المرور الحالية غير صحيحة." }));
            }

            var otherErrors = result.Errors.Where(e => e.Code != "PasswordMismatch").Select(e => e.Description).ToArray();
            if (otherErrors.Any())
            {
                failures.Add(new KeyValuePair<string, string[]>("NewPassword", otherErrors));
            }

            throw new ValidationException(failures);
        }

        // revoke all active refresh tokens for that user
        authHelperService.RevokeAllActiveRefreshTokens(user);
        await userManager.UpdateAsync(user);
    }
}
