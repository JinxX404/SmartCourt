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
        if (!Guid.TryParse(userId, out var parsedUserId))
        {
            throw new ValidationException("userId", "معرّف المستخدم غير صالح.");
        }

        var user = await _userManager.FindByIdAsync(parsedUserId.ToString());
        if (user is null)
        {
            throw new NotFoundException("المستخدم غير موجود.");
        }

        var decodedToken = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(token));
        var result = await _userManager.ConfirmEmailAsync(user, decodedToken);
        if (!result.Succeeded)
        {
            throw new BusinessException("رمز تأكيد البريد الإلكتروني غير صالح أو منتهي الصلاحية.");
        }
    }
}
