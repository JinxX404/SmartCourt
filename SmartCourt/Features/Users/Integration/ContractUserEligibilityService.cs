using Microsoft.EntityFrameworkCore;
using SmartCourt.Features.Auth.Enums;
using SmartCourt.Persistence;

namespace SmartCourt.Features.Users.Integration;

public sealed class ContractUserEligibilityService(
    ApplicationDbContext dbContext,
    TimeProvider timeProvider)
    : IContractUserEligibilityService
{
    public async Task<ContractUserEligibilityFacts?>
        FindEligibilityAsync(
            Guid userId,
            CancellationToken cancellationToken)
    {
        if (userId == Guid.Empty)
        {
            return null;
        }

        var user = await dbContext.Users
            .AsNoTracking()
            .Where(item => item.Id == userId)
            .Select(item => new
            {
                item.Id,
                item.Status
            })
            .SingleOrDefaultAsync(cancellationToken);
        if (user is null)
        {
            return null;
        }

        var roles = await (
                from userRole in dbContext.UserRoles
                join role in dbContext.Roles
                    on userRole.RoleId equals role.Id
                where userRole.UserId == userId
                select role.Name)
            .Where(roleName => roleName != null)
            .ToListAsync(cancellationToken);

        var now = timeProvider.GetUtcNow();
        var hasActivePenalty = await dbContext.LawyerPenalties
            .AsNoTracking()
            .AnyAsync(
                p => p.LawyerUserId == userId
                    && !p.IsRevoked
                    && (!p.EndsAt.HasValue || p.EndsAt.Value > now),
                cancellationToken);

        var canActAsLawyer = roles.Contains("Lawyer") && !hasActivePenalty;

        return new ContractUserEligibilityFacts(
            user.Id,
            user.Status == UserStatus.Active,
            roles.Contains("Client"),
            canActAsLawyer,
            roles.Contains("Moderator"),
            roles.Contains("FinanceAdministrator"),
            roles.Contains("SuperAdministrator"));
    }
}

