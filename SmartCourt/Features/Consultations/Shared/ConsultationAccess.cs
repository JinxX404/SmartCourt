using Microsoft.EntityFrameworkCore;
using SmartCourt.Common.Domain;
using SmartCourt.Interfaces;
using SmartCourt.Persistence;

namespace SmartCourt.Features.Consultations.Shared;

internal static class ConsultationAccess
{
    internal static Guid RequireUserId(ICurrentUserService currentUserService) =>
        currentUserService.RequireUserId("Authentication is required for this consultation operation.");

    internal static Task<bool> HasRoleAsync(
        ApplicationDbContext dbContext,
        Guid userId,
        string role,
        CancellationToken cancellationToken) =>
        dbContext.UserRoles.AnyAsync(
            userRole => userRole.UserId == userId
                && dbContext.Roles.Any(item => item.Id == userRole.RoleId && item.Name == role),
            cancellationToken);
}
