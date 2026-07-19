using SmartCourt.Features.Auth.Enums;

namespace SmartCourt.Features.Users.Lawyers;

public static class LawyerQueryExtensions
{
    public static IQueryable<ApplicationUser> WherePublicLawyer(
        this IQueryable<ApplicationUser> users,
        Guid lawyerId)
    {
        return users.Where(user => user.Id == lawyerId
            && user.LawyerProfile != null
            && user.EmailConfirmed
            && user.Status == UserStatus.Active);
    }
}
