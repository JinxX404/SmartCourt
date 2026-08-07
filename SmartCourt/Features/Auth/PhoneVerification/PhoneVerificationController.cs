using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartCourt.Common.Models;
using SmartCourt.Features.Auth.PhoneVerification.DTOs;

namespace SmartCourt.Features.Auth.PhoneVerification;

[ApiController]
[Route("api/auth/phone")]
[Authorize] // Requires user to be logged in
public class PhoneVerificationController(IMediator mediator) : ControllerBase
{
    [HttpPost("send-token")]
    public async Task<ActionResult<ApiResponse<object>>> SendToken([FromBody] SendPhoneVerificationRequest request)
    {
        var response = await mediator.Send(new SendPhoneVerificationTokenCommand(request));
        return Ok(response);
    }

    [HttpPost("confirm")]
    public async Task<ActionResult<ApiResponse<object>>> Confirm([FromBody] ConfirmPhoneVerificationRequest request)
    {
        var response = await mediator.Send(new ConfirmPhoneVerificationCommand(request));
        return Ok(response);
    }
}
