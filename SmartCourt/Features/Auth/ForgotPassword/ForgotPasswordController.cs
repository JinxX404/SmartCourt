using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SmartCourt.Common.Models;
using SmartCourt.Common.RateLimiting;
using SmartCourt.Features.Auth.ForgotPassword.DTOs;

namespace SmartCourt.Features.Auth.ForgotPassword;

[ApiController]
[Route("api/auth/forgot-password")]
[AllowAnonymous]
public class ForgotPasswordController(
    IForgotPasswordService forgotPasswordService,
    IAccountKeyRateLimiter accountKeyRateLimiter) : ControllerBase
{
    [HttpPost]
    [SecurityRateLimit(RateLimitPolicyNames.ForgotPassword)]
    public async Task<IActionResult> HandleAsync([FromBody] ForgotPasswordRequest request, CancellationToken cancellationToken)
    {
        accountKeyRateLimiter.CheckForgotPassword(request.Email);
        await forgotPasswordService.ForgotPasswordAsync(request.Email, cancellationToken);

        return Ok(ApiResponse.Ok("إذا كان البريد الإلكتروني مسجلاً، سيتم إرسال رابط إعادة تعيين كلمة المرور"));
    }
}
