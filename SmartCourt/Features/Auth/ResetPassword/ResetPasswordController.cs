using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartCourt.Common;
using SmartCourt.Interfaces;

namespace SmartCourt.Features.Auth.ResetPassword;

[ApiController]
[Route("api/auth/reset-password")]
[AllowAnonymous]
public class ResetPasswordController(IAuthService authService) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> HandleAsync([FromBody] ResetPasswordRequest request)
    {
        await authService.ResetPasswordAsync(request.Email, request.Token, request.NewPassword);
        return Ok(ApiResponse<string>.Ok("تم إعادة تعيين كلمة المرور بنجاح"));
    }
}
