using SmartCourt.Common.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using SmartCourt.Common.RateLimiting;
using SmartCourt.Features.Auth.ConfirmEmail.DTOs;

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
        [FromQuery] VerifyEmailRequest request,
        CancellationToken cancellationToken)
    {
        accountKeyRateLimiter.CheckConfirmEmail(request.UserId);
        await confirmEmailService.ConfirmEmailAsync(request.UserId, request.Token, cancellationToken);
        return Ok(ApiResponse.Ok("تم تأكيد البريد الإلكتروني بنجاح."));
    }

}
