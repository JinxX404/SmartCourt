using Microsoft.AspNetCore.Authorization;
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
        /*
         * ALGORITHM:
         * 1. Validate the incoming request (automatically handled by FluentValidation pipeline).
         * 2. Call IAuthService.ForgotPasswordAsync(request.Email)
         * 3. Return an ApiResponse<string>.Ok() with message: "إذا كان البريد الإلكتروني مسجلاً، سيتم إرسال رابط إعادة تعيين كلمة المرور"
         * 
         * Note: Always return 200 OK to prevent email enumeration attacks.
         */
        throw new NotImplementedException();
    }
}
