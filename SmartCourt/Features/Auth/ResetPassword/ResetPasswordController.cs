using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using SmartCourt.Common.Models;
using SmartCourt.Features.Auth.ResetPassword.DTOs;

namespace SmartCourt.Features.Auth.ResetPassword;

[ApiController]
[Route("api/auth/reset-password")]
[AllowAnonymous]
public class ResetPasswordController(IResetPasswordService resetPasswordService) : ControllerBase
{
    [HttpPost]
    [EnableRateLimiting("ResetPassword")]
    public async Task<IActionResult> HandleAsync([FromBody] ResetPasswordRequest request, CancellationToken cancellationToken)
    {
        await resetPasswordService.ResetPasswordAsync(request.Email, request.Token, request.NewPassword, cancellationToken);
        return Ok(ApiResponse.Ok("تم إعادة تعيين كلمة المرور بنجاح"));
    }
}
