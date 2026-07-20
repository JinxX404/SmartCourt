using SmartCourt.Common.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using SmartCourt.Common.RateLimiting;

namespace SmartCourt.Features.Auth.ConfirmEmail;

[ApiController]
[Route("api/auth/confirm-email")]
public class ConfirmEmailController(
    IConfirmEmailService confirmEmailService,
    IAccountKeyRateLimiter accountKeyRateLimiter) : ControllerBase
{
    [HttpGet]
    [AllowAnonymous]
    [SecurityRateLimit(RateLimitPolicyNames.ConfirmEmail)]
    public async Task<IActionResult> Get(
        [FromQuery] string? userId,
        [FromQuery] string? token,
        CancellationToken cancellationToken)
    {
        accountKeyRateLimiter.CheckConfirmEmail(userId ?? string.Empty);
        await confirmEmailService.ConfirmEmailAsync(userId, token, cancellationToken);
        return Ok(ApiResponse.Ok("تم تأكيد البريد الإلكتروني بنجاح."));
    }

    [HttpGet("/api/auth/confirm-email-change")]
    [AllowAnonymous]
    public async Task<IActionResult> ConfirmChangeAsync([FromQuery] string userId, [FromQuery] string newEmail, [FromQuery] string token, CancellationToken cancellationToken)
    {
        await confirmEmailService.ConfirmEmailChangeAsync(userId, newEmail, token, cancellationToken);
        return Ok(ApiResponse<bool>.Ok(true, "تم تأكيد وتغيير البريد الإلكتروني بنجاح."));
    }
}
