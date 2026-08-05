using SmartCourt.Common.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartCourt.Features.Users.Clients.DTOs;
using SmartCourt.Common.RateLimiting;
using SmartCourt.Features.Users.Shared.DTOs;

namespace SmartCourt.Features.Users.Clients;

[ApiController]
[Route("api/clients/profile")]
[Authorize(Roles = "Client")]
public class ClientsController(IClientService clientService) : ControllerBase
{
    [HttpGet]
    [SecurityRateLimit(RateLimitPolicyNames.PrivateProfileGet)]
    public async Task<IActionResult> GetAsync(CancellationToken cancellationToken)
    {
        var result = await clientService.GetProfileAsync(cancellationToken);
        return Ok(ApiResponse<ClientProfileResponse>.Ok(result));
    }

    [HttpPost("complete")]
    [SecurityRateLimit(RateLimitPolicyNames.PrivateProfileUpdate)] // Using same rate limit as update for now
    public async Task<IActionResult> CompleteAsync([FromBody] CompleteClientProfileRequest request, CancellationToken cancellationToken)
    {
        await clientService.CompleteProfileAsync(request, cancellationToken);
        return Ok(ApiResponse.Ok("تم استكمال الملف الشخصي بنجاح."));
    }

    [HttpPut]
    [SecurityRateLimit(RateLimitPolicyNames.PrivateProfileUpdate)]
    public async Task<IActionResult> UpdateAsync([FromBody] UpdateClientProfileRequest request, CancellationToken cancellationToken)
    {
        await clientService.UpdateProfileAsync(request, cancellationToken);
        return Ok(ApiResponse.Ok("تم تحديث الملف الشخصي بنجاح."));
    }

    [HttpDelete]
    [SecurityRateLimit(RateLimitPolicyNames.PrivateProfileDelete)]
    public async Task<IActionResult> DeleteAsync(
        [FromBody] DeleteAccountRequest request,
        CancellationToken cancellationToken)
    {
        await clientService.DeleteProfileAsync(request, cancellationToken);
        return Ok(ApiResponse.Ok("تم حذف الملف الشخصي بنجاح."));
    }
}
