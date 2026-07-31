using SmartCourt.Common.Exceptions;
using SmartCourt.Interfaces;

namespace SmartCourt.Common.Domain;

public static class CurrentUserServiceExtensions
{
    public static Guid RequireUserId(
        this ICurrentUserService currentUserService,
        string errorMessage)
    {
        if (!currentUserService.IsAuthenticated
            || !currentUserService.UserId.HasValue
            || currentUserService.UserId.Value == Guid.Empty)
        {
            throw new AuthenticationException(errorMessage);
        }

        return currentUserService.UserId.Value;
    }
}
