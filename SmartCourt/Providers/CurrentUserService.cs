using System;
using Microsoft.AspNetCore.Http;
using SmartCourt.Extensions;
using SmartCourt.Interfaces;

namespace SmartCourt.Providers;

public class CurrentUserService(IHttpContextAccessor httpContextAccessor) : ICurrentUserService
{
    public Guid? UserId
    {
        get
        {
            var httpContext = httpContextAccessor.HttpContext;
            if (httpContext == null)
            {
                return null;
            }

            var user = httpContext.User;
            if (user == null || user.Identity?.IsAuthenticated != true)
            {
                return null;
            }

            try
            {
                return user.GetUserIdAsGuid();
            }
            catch
            {
                return null;
            }
        }
    }

    public bool IsAuthenticated => httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false;
}
