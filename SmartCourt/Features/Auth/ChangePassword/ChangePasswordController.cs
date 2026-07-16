using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartCourt.Common;
using SmartCourt.Interfaces;
using System.Security.Claims;
using SmartCourt.Extensions;

namespace SmartCourt.Features.Auth.ChangePassword;

[ApiController]
[Route("api/auth/change-password")]
[Authorize]
public class ChangePasswordController(IAuthService authService) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> HandleAsync([FromBody] ChangePasswordRequest request)
    {

        var userId = User.GetUserId();

        await authService.ChangePasswordAsync(userId, request.CurrentPassword, request.NewPassword);

        return Ok(ApiResponse<string>.Ok("تم تغيير كلمة المرور بنجاح"));
    }
}
