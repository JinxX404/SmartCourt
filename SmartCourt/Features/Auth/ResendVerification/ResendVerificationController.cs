using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartCourt.Common.Models;
using SmartCourt.Common.RateLimiting;
using SmartCourt.Features.Auth.ResendVerification.DTOs;

namespace SmartCourt.Features.Auth.ResendVerification;

[ApiController]
[Route("api/auth/resend-verification")]
[AllowAnonymous]
public class ResendVerificationController(
    IResendVerificationService resendVerificationService,
    IAccountKeyRateLimiter accountKeyRateLimiter) : ControllerBase
{
    [HttpPost]
    [SecurityRateLimit(RateLimitPolicyNames.ResendVerification)]
    public async Task<IActionResult> HandleAsync([FromBody] ResendVerificationRequest request, CancellationToken cancellationToken)
    {
        accountKeyRateLimiter.CheckResendVerification(request.Email);
        await resendVerificationService.ResendVerificationEmailAsync(request.Email, cancellationToken);
        return Ok(ApiResponse.Ok("تم إرسال رابط التحقق مرة أخرى"));
    }
}
