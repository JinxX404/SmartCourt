using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartCourt.Common.Enums;
using SmartCourt.Common.Exceptions;
using SmartCourt.Common.Models;
using SmartCourt.Features.Auth.Enums;
using SmartCourt.Persistence;


namespace SmartCourt.Features.Admin.Verifications.ApproveUserAccount;

public sealed class ApproveUserAccountHandler(
    ApplicationDbContext context)
    : IRequestHandler<ApproveUserAccountCommand, ApiResponse<object>>
{
    public async Task<ApiResponse<object>> Handle(
        ApproveUserAccountCommand request,
        CancellationToken cancellationToken)
    {
        var user = await context.Users
            .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);

        if (user is null)
        {
            throw new NotFoundException("المستخدم غير موجود");
        }


        var hasPendingDocs = await context.UserVerificationDocuments
            .AnyAsync(d => d.UserId == user.Id && d.IsCurrent && d.Status == VerificationDocumentStatus.Pending, cancellationToken);

        if (!hasPendingDocs)
        {
            user.Status = UserStatus.Active;
        }
        user.ModifiedFieldsJson = null;
        await context.SaveChangesAsync(cancellationToken);



        return ApiResponse<object>.Ok(new { message = "تم اعتماد بيانات الحساب بنجاح" });
    }
}
