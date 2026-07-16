using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SmartCourt.Common;
using SmartCourt.Interfaces;

namespace SmartCourt.Features.Auth.ForgotPassword;

[ApiController]
[Route("api/auth/forgot-password")]
[AllowAnonymous]
public class ForgotPasswordController(IAuthService authService) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> HandleAsync([FromBody] ForgotPasswordRequest request)
    {

        await authService.ForgotPasswordAsync(request.Email);

        return Ok(ApiResponse<string>.Ok("إذا كان البريد الإلكتروني مسجلاً، سيتم إرسال رابط إعادة تعيين كلمة المرور"));
    }
}
