using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using SmartCourt.Common.Models;
using SmartCourt.Features.Auth.ResendVerification.DTOs;

namespace SmartCourt.Features.Auth.ResendVerification;

[ApiController]
[Route("api/auth/resend-verification")]
[AllowAnonymous]
public class ResendVerificationController(IResendVerificationService resendVerificationService) : ControllerBase
{
    [HttpPost]
    [EnableRateLimiting("ResendVerification")]
    public async Task<IActionResult> HandleAsync([FromBody] ResendVerificationRequest request, CancellationToken cancellationToken)
    {
        await resendVerificationService.ResendVerificationEmailAsync(request.Email, cancellationToken);
        return Ok(ApiResponse.Ok("تم إرسال رابط التحقق مرة أخرى"));
    }
}
