using SmartCourt.Common.Exceptions;
using SmartCourt.Common.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using System.Text;

namespace SmartCourt.Features.Auth.ConfirmEmail;

public class ConfirmEmailService : IConfirmEmailService
{
    private readonly UserManager<ApplicationUser> _userManager;

    public ConfirmEmailService(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task ConfirmEmailAsync(string userId, string token, CancellationToken cancellationToken = default)
    {
        var user = await GetUserAndValidateAsync(userId);
        
        var decodedToken = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(token));
        var result = await _userManager.ConfirmEmailAsync(user, decodedToken);
        if (!result.Succeeded)
        {
            throw new BusinessException("رمز تأكيد البريد الإلكتروني غير صالح أو منتهي الصلاحية.");
        }
    }

    public async Task ConfirmEmailChangeAsync(string userId, string newEmail, string token, CancellationToken cancellationToken = default)
    {
        var user = await GetUserAndValidateAsync(userId);

        var decodedToken = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(token));
        var result = await _userManager.ChangeEmailAsync(user, newEmail, decodedToken);
        
        if (!result.Succeeded)
        {
            throw new BusinessException("رمز تغيير البريد الإلكتروني غير صالح أو منتهي الصلاحية.");
        }

        var setUserNameResult = await _userManager.SetUserNameAsync(user, newEmail);
        if (!setUserNameResult.Succeeded)
        {
            throw new BusinessException(string.Join(" ", setUserNameResult.Errors.Select(e => e.Description)));
        }
    }

    private async Task<ApplicationUser> GetUserAndValidateAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user is null)
        {
            throw new NotFoundException("المستخدم غير موجود.");
        }

        return user;
    }
}
