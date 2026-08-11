using Microsoft.EntityFrameworkCore;
using SmartCourt.Features.Auth.Integration;
using SmartCourt.Persistence;

namespace SmartCourt.Features.Auth.Events;

internal sealed class AuthNotificationContextReader(
    ApplicationDbContext dbContext) : IAuthNotificationContextReader
{
    public async Task<AuthAccountNotificationContext> GetAccountAsync(
        Guid userId,
        CancellationToken cancellationToken)
    {
        return await dbContext.Users
            .AsNoTracking()
            .Where(user => user.Id == userId)
            .Select(user => new AuthAccountNotificationContext(user.Id))
            .SingleOrDefaultAsync(cancellationToken)
            ?? throw new InvalidOperationException(
                "تعذر إنشاء إشعار أمان الحساب لأن الحساب المرتبط بالحدث غير موجود.");
    }
}
