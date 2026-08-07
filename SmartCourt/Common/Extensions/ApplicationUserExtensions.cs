using System.Security.Claims;
using SmartCourt.Features.Auth.Enums;

namespace SmartCourt.Common.Extensions;

public static class ApplicationUserExtensions
{
    public const string SecurityStampClaimType = "security_stamp";

    public static bool IsAccessEligible(this ApplicationUser user)
    {
        return user.EmailConfirmed
            && user.Status is UserStatus.Active or UserStatus.PendingReview or UserStatus.Unverified or UserStatus.Rejected;
    }

    public static bool HasValidSecurityStamp(this ApplicationUser user, ClaimsPrincipal principal)
    {
        var tokenSecurityStamp = principal.FindFirstValue(SecurityStampClaimType);

        return !string.IsNullOrWhiteSpace(user.SecurityStamp)
            && !string.IsNullOrWhiteSpace(tokenSecurityStamp)
            && string.Equals(user.SecurityStamp, tokenSecurityStamp, StringComparison.Ordinal);
    }
}
