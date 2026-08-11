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
}
