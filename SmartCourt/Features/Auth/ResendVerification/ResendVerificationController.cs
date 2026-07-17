using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartCourt.Common.Models;
using SmartCourt.Features.Auth.ResendVerification.DTOs;

namespace SmartCourt.Features.Auth.ResendVerification;

[ApiController]
[Route("api/auth/resend-verification")]
[AllowAnonymous]
public class ResendVerificationController(IResendVerificationService resendVerificationService) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> HandleAsync([FromBody] ResendVerificationRequest request)
    {
        await resendVerificationService.ResendVerificationEmailAsync(request.Email);
        return Ok(ApiResponse<string>.Ok("تم إرسال رابط التحقق مرة أخرى"));
    }
}
