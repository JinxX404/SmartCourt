using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartCourt.Common.Exceptions;
using SmartCourt.Common.Models;
using SmartCourt.Features.Admin.Verifications.Events;
using SmartCourt.Features.Auth.Enums;
using SmartCourt.Infrastructure.Providers.Events;
using SmartCourt.Persistence;

namespace SmartCourt.Features.Admin.Verifications.RejectUserAccount;

public sealed class RejectUserAccountHandler(
    ApplicationDbContext context,
    IOutboxWriter outboxWriter)
    : IRequestHandler<RejectUserAccountCommand, ApiResponse<object>>
{
    public async Task<ApiResponse<object>> Handle(
        RejectUserAccountCommand request,
        CancellationToken cancellationToken)
    {
        var user = await context.Users
            .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);

        if (user is null)
        {
            throw new NotFoundException("المستخدم غير موجود");
        }

        var wasRejected = user.Status == UserStatus.Rejected;
        user.Status = UserStatus.Rejected;
        user.RejectionReason = request.RejectionReason;
        user.ModifiedFieldsJson = null;

        if (!wasRejected)
        {
            await VerificationOutbox.EnqueueAccountAsync(
                outboxWriter,
                VerificationEventTypes.AccountRejected,
                user,
                Guid.NewGuid(),
                cancellationToken);
        }

        await context.SaveChangesAsync(cancellationToken);

        return ApiResponse<object>.Ok(new { message = "تم رفض بيانات الحساب بنجاح" });
    }
}
