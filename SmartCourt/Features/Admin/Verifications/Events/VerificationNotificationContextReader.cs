using Microsoft.EntityFrameworkCore;
using SmartCourt.Features.Admin.Verifications.Integration;
using SmartCourt.Persistence;

namespace SmartCourt.Features.Admin.Verifications.Events;

internal sealed class VerificationNotificationContextReader(
    ApplicationDbContext dbContext) : IVerificationNotificationContextReader
{
    public async Task<VerificationDocumentNotificationContext> GetDocumentAsync(
        Guid documentId,
        CancellationToken cancellationToken)
    {
        return await dbContext.UserVerificationDocuments
            .AsNoTracking()
            .Where(document => document.Id == documentId)
            .Select(document => new VerificationDocumentNotificationContext(
                document.Id,
                document.UserId,
                document.DocumentType,
                document.Status))
            .SingleOrDefaultAsync(cancellationToken)
            ?? throw new InvalidOperationException(
                "تعذر إنشاء الإشعار لأن مستند التحقق المرتبط بالحدث غير موجود.");
    }

    public async Task<VerificationAccountNotificationContext> GetAccountAsync(
        Guid userId,
        CancellationToken cancellationToken)
    {
        return await dbContext.Users
            .AsNoTracking()
            .Where(user => user.Id == userId)
            .Select(user => new VerificationAccountNotificationContext(
                user.Id,
                user.Status))
            .SingleOrDefaultAsync(cancellationToken)
            ?? throw new InvalidOperationException(
                "تعذر إنشاء الإشعار لأن الحساب المرتبط بالحدث غير موجود.");
    }

    public async Task<VerificationReviewRequestedNotificationContext>
        GetReviewRequestedAsync(
            Guid userId,
            CancellationToken cancellationToken)
    {
        var userExists = await dbContext.Users
            .AsNoTracking()
            .AnyAsync(user => user.Id == userId, cancellationToken);
        if (!userExists)
        {
            throw new InvalidOperationException(
                "تعذر إنشاء إشعار مراجعة التحقق لأن الحساب المرتبط بالحدث غير موجود.");
        }

        var administratorUserIds = await (
                from userRole in dbContext.UserRoles.AsNoTracking()
                join role in dbContext.Roles.AsNoTracking()
                    on userRole.RoleId equals role.Id
                where role.Name == "Admin"
                select userRole.UserId)
            .Distinct()
            .ToArrayAsync(cancellationToken);

        return new VerificationReviewRequestedNotificationContext(
            userId,
            administratorUserIds);
    }
}
