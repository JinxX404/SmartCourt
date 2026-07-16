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
        /*
         * ALGORITHM:
         * 1. Validate incoming request.
         * 2. Call IAuthService.ResendVerificationEmailAsync(request.Email)
         * 3. Return ApiResponse<string>.Ok() with message: "تم إرسال رابط التحقق مرة أخرى"
         */
        throw new NotImplementedException();
    }
}
