using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using SmartCourt.Common;
using SmartCourt.Interfaces;

namespace SmartCourt.Features.Auth.ConfirmEmail;

[ApiController]
[Route("api/auth/confirm-email")]
public class ConfirmEmailController : ControllerBase
{
    private readonly IAuthService _authService;

    public ConfirmEmailController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> Get([FromQuery] string userId, [FromQuery] string token)
    {
        await _authService.ConfirmEmailAsync(userId, token);
        return Ok(ApiResponse<bool>.Ok(true, "تم تأكيد البريد الإلكتروني بنجاح."));
    }
}
