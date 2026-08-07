using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartCourt.Common.Exceptions;
using SmartCourt.Common.Models;
using SmartCourt.Features.Auth.Enums;
using SmartCourt.Persistence;

namespace SmartCourt.Features.Admin.Verifications.RejectUserAccount;

public sealed class RejectUserAccountHandler(ApplicationDbContext context)
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

        user.Status = UserStatus.Rejected;
        user.RejectionReason = request.RejectionReason;
        user.ModifiedFieldsJson = null;

        await context.SaveChangesAsync(cancellationToken);

        // TODO: Send an email/notification to the user with `request.RejectionReason`

        return ApiResponse<object>.Ok(new { message = "تم رفض بيانات الحساب بنجاح" });
    }
}
