using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SmartCourt.Common.Models;
using SmartCourt.Features.Auth.ForgotPassword.DTOs;

namespace SmartCourt.Features.Auth.ForgotPassword;

[ApiController]
[Route("api/auth/forgot-password")]
[AllowAnonymous]
public class ForgotPasswordController(IForgotPasswordService forgotPasswordService) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> HandleAsync([FromBody] ForgotPasswordRequest request)
    {

        await forgotPasswordService.ForgotPasswordAsync(request.Email);

        return Ok(ApiResponse<string>.Ok("إذا كان البريد الإلكتروني مسجلاً، سيتم إرسال رابط إعادة تعيين كلمة المرور"));
    }
}
