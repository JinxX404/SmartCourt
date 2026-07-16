using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using SmartCourt.Common;

namespace SmartCourt.Features.Auth.ConfirmEmail;

[ApiController]
[Route("api/auth/confirm-email")]
public class ConfirmEmailController(IConfirmEmailService confirmEmailService) : ControllerBase
{
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> Get([FromQuery] string userId, [FromQuery] string token, CancellationToken cancellationToken)
    {
        await confirmEmailService.ConfirmEmailAsync(userId, token, cancellationToken);
        return Ok(ApiResponse<bool>.Ok(true, "تم تأكيد البريد الإلكتروني بنجاح."));
    }
}
