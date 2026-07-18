using SmartCourt.Features.Auth.ChangePassword.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartCourt.Common.Models;
using SmartCourt.Features.Auth.ChangePassword;
using System.Security.Claims;
using SmartCourt.Extensions;

namespace SmartCourt.Features.Auth.ChangePassword;

[ApiController]
[Route("api/auth/change-password")]
[Authorize]
public class ChangePasswordController(IChangePasswordService changePasswordService) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> HandleAsync([FromBody] ChangePasswordRequest request, CancellationToken cancellationToken)
    {
        await changePasswordService.ChangePasswordAsync(request.CurrentPassword, request.NewPassword, cancellationToken);

        return Ok(ApiResponse.Ok("تم تغيير كلمة المرور بنجاح"));
    }
}
