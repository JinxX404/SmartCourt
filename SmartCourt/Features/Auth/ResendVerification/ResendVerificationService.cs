using Microsoft.AspNetCore.Identity;
using SmartCourt.Common.Entities;
using SmartCourt.Features.Auth.Enums;
using SmartCourt.Features.Auth.Shared;

namespace SmartCourt.Features.Auth.ResendVerification;

public class ResendVerificationService(
    UserManager<ApplicationUser> userManager,
    IAuthHelperService authHelperService
) : IResendVerificationService
{
    public async Task ResendVerificationEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        var user = await userManager.FindByEmailAsync(email);

        if (user is null || user.EmailConfirmed || user.Status != UserStatus.Unverified)
        {
            return;
        }

        await authHelperService.SendConfirmationEmailAsync(user, cancellationToken);
    }
}
