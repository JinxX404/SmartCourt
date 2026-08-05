using SmartCourt.Common.Exceptions;
using System.Security.Claims;

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

    public static Guid GetUserIdAsGuid(this ClaimsPrincipal principal)
    {
        var userIdString = principal.GetUserId();
        
        if (!Guid.TryParse(userIdString, out var userId))
        {
            throw new AuthenticationException("المستخدم غير معروف");
        }

        return userId;
    }
}
