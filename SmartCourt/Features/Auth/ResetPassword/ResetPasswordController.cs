using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartCourt.Common.Models;
using SmartCourt.Common.RateLimiting;
using SmartCourt.Features.Auth.ResetPassword.DTOs;

namespace SmartCourt.Features.Auth.ResetPassword;

[ApiController]
[Route("api/auth/reset-password")]
[AllowAnonymous]
public class ResetPasswordController(
    IResetPasswordService resetPasswordService,
    IAccountKeyRateLimiter accountKeyRateLimiter) : ControllerBase
{
    [HttpPost]
    [SecurityRateLimit(RateLimitPolicyNames.ResetPassword)]
    public async Task<IActionResult> HandleAsync([FromBody] ResetPasswordRequest request, CancellationToken cancellationToken)
    {
        accountKeyRateLimiter.CheckResetPassword(request.Email, request.Token ?? string.Empty);
        await resetPasswordService.ResetPasswordAsync(request.Email, request.Token, request.NewPassword, cancellationToken);
        return Ok(ApiResponse.Ok("تم إعادة تعيين كلمة المرور بنجاح"));
    }
}
