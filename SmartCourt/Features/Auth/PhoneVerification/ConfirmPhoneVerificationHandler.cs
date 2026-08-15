using MediatR;
using Microsoft.AspNetCore.Identity;
using SmartCourt.Common.Exceptions;
using SmartCourt.Common.Models;
using SmartCourt.Entities;
using Microsoft.EntityFrameworkCore;
using SmartCourt.Interfaces;

namespace SmartCourt.Features.Auth.PhoneVerification;

public class ConfirmPhoneVerificationHandler(
    ICurrentUserService currentUserService,
    UserManager<ApplicationUser> userManager) : IRequestHandler<ConfirmPhoneVerificationCommand, ApiResponse<object>>
{
    public async Task<ApiResponse<object>> Handle(ConfirmPhoneVerificationCommand request, CancellationToken cancellationToken)
    {
        var userId = currentUserService.UserId.ToString();
        var user = await userManager.FindByIdAsync(userId);

        if (user == null)
            throw new NotFoundException("المستخدم غير موجود");

        var result = await userManager.ChangePhoneNumberAsync(user, request.Request.PhoneNumber, request.Request.Token);

        if (!result.Succeeded)
            throw new BusinessException("كود التوثيق غير صحيح أو منتهي الصلاحية.");

        // Reload user with documents to evaluate status
        user = await userManager.Users
            .Include(u => u.VerificationDocuments)
            .SingleAsync(u => u.Id == currentUserService.UserId, cancellationToken);

        var isLawyer = await userManager.IsInRoleAsync(user, "Lawyer");
        var today = DateOnly.FromDateTime(DateTimeOffset.UtcNow.UtcDateTime);

        user.Status = SmartCourt.Features.Admin.Verifications.Shared.VerificationStatusEvaluator.ResolveAccountStatus(
            user.VerificationDocuments,
            today,
            isLawyer,
            user.PhoneNumberConfirmed,
            user.Status);

        await userManager.UpdateAsync(user);

        return ApiResponse<object>.Ok(new { message = "تم توثيق رقم الهاتف بنجاح" });
    }
}
