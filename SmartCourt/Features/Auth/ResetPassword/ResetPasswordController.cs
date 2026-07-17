using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartCourt.Common.Models;
using SmartCourt.Features.Auth.ResetPassword.DTOs;

namespace SmartCourt.Features.Auth.ResetPassword;

[ApiController]
[Route("api/auth/reset-password")]
[AllowAnonymous]
public class ResetPasswordController(IResetPasswordService resetPasswordService) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> HandleAsync([FromBody] ResetPasswordRequest request)
    {
        await resetPasswordService.ResetPasswordAsync(request.Email, request.Token, request.NewPassword);
        return Ok(ApiResponse<string>.Ok("تم إعادة تعيين كلمة المرور بنجاح"));
    }
}
