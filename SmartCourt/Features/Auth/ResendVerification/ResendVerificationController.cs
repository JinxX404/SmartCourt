using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartCourt.Common;
using SmartCourt.Interfaces;

namespace SmartCourt.Features.Auth.ResendVerification;

[ApiController]
[Route("api/auth/resend-verification")]
[AllowAnonymous]
public class ResendVerificationController(IAuthService authService) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> HandleAsync([FromBody] ResendVerificationRequest request)
    {
        await authService.ResendVerificationEmailAsync(request.Email);
        return Ok(ApiResponse<string>.Ok("تم إرسال رابط التحقق مرة أخرى"));
    }
}
