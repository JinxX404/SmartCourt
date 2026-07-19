using System.Security.Claims;
using SmartCourt.Features.Auth.Enums;

namespace SmartCourt.Common;

public static class AuthSecurity
{
    public const string SecurityStampClaimType = "security_stamp";

    public static bool IsAccessEligible(ApplicationUser user)
    {
        return user.EmailConfirmed
            && user.Status is UserStatus.Active or UserStatus.PendingReview;
    }

    public static bool HasValidSecurityStamp(ApplicationUser user, ClaimsPrincipal principal)
    {
        var tokenSecurityStamp = principal.FindFirstValue(SecurityStampClaimType);

        return !string.IsNullOrWhiteSpace(user.SecurityStamp)
            && !string.IsNullOrWhiteSpace(tokenSecurityStamp)
            && string.Equals(user.SecurityStamp, tokenSecurityStamp, StringComparison.Ordinal);
    }
}
