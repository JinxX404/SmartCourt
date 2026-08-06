using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartCourt.Common.Enums;
using SmartCourt.Common.Exceptions;
using SmartCourt.Common.Models;
using SmartCourt.Features.Auth.Enums;
using SmartCourt.Persistence;
using SmartCourt.Features.Notifications.Services;

namespace SmartCourt.Features.Admin.Verifications.ApproveUserAccount;

public sealed class ApproveUserAccountHandler(
    ApplicationDbContext context,
    INotificationsService notificationsService)
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

        user.Status = UserStatus.Active;
        await context.SaveChangesAsync(cancellationToken);

        await notificationsService.SendNotificationAsync(
            user.Id,
            "تم توثيق الحساب بالكامل",
            "تهانينا! تم اعتماد بيانات حسابك وتوثيقه بنجاح من إدارة المنصة.",
            cancellationToken);

        return ApiResponse<object>.Ok(new { message = "تم اعتماد بيانات الحساب بنجاح" });
    }
}
