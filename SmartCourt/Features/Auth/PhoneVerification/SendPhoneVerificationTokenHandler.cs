using MediatR;
using Microsoft.AspNetCore.Identity;
using SmartCourt.Common.Exceptions;
using SmartCourt.Common.Models;
using SmartCourt.Entities;
using SmartCourt.Interfaces;
using SmartCourt.Interfaces.Providers;

namespace SmartCourt.Features.Auth.PhoneVerification;

public class SendPhoneVerificationTokenHandler(
    ICurrentUserService currentUserService,
    UserManager<ApplicationUser> userManager,
    ISmsProvider smsProvider) : IRequestHandler<SendPhoneVerificationTokenCommand, ApiResponse<object>>
{
    public async Task<ApiResponse<object>> Handle(SendPhoneVerificationTokenCommand request, CancellationToken cancellationToken)
    {
        var userId = currentUserService.UserId.ToString();
        var user = await userManager.FindByIdAsync(userId);

        if (user == null)
            throw new NotFoundException("المستخدم غير موجود");

        var token = await userManager.GenerateChangePhoneNumberTokenAsync(user, request.Request.PhoneNumber);
        
        var message = $"كود التوثيق الخاص بك في منصة مستشار هو: {token}";
        var isSent = await smsProvider.SendSmsAsync(request.Request.PhoneNumber, message);

        if (!isSent)
            throw new BusinessException("فشل إرسال رسالة التوثيق. يرجى التأكد من صحة رقم الهاتف والمحاولة لاحقاً.");

        return ApiResponse<object>.Ok(new { message = "تم إرسال كود التوثيق بنجاح" });
    }
}
