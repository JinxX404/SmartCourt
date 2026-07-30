using Microsoft.EntityFrameworkCore;
using SmartCourt.Common.Exceptions;
using SmartCourt.Interfaces;
using SmartCourt.Persistence;

namespace SmartCourt.Features.Proposals.Shared;

internal static class ProposalAccess
{
    public static Guid GetRequiredUserId(ICurrentUserService currentUserService)
    {
        return currentUserService.UserId
            ?? throw new AuthenticationException("Authentication is required.");
    }

    public static async Task<bool> HasRoleAsync(
        ApplicationDbContext context,
        Guid userId,
        string roleName,
        CancellationToken cancellationToken)
    {
        return await (
                from userRole in context.UserRoles
                join role in context.Roles on userRole.RoleId equals role.Id
                where userRole.UserId == userId && role.Name == roleName
                select userRole.UserId)
            .AnyAsync(cancellationToken);
    }
}
