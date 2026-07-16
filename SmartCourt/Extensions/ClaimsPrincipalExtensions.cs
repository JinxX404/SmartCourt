using System.Security.Claims;
using SmartCourt.Common;

namespace SmartCourt.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static string GetUserId(this ClaimsPrincipal principal)
    {
        var userId = principal.FindFirstValue(ClaimTypes.NameIdentifier);
        
        if (string.IsNullOrEmpty(userId))
        {
            throw new AuthenticationException("المستخدم غير معروف");
        }

        return userId;
    }
}
