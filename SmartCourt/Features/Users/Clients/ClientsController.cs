using SmartCourt.Common.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartCourt.Features.Users.Clients.DTOs;

namespace SmartCourt.Features.Users.Clients;

[ApiController]
[Route("api/clients/profile")]
[Authorize(Roles = "Client")]
public class ClientsController(IClientService clientService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAsync(CancellationToken cancellationToken)
    {
        var result = await clientService.GetProfileAsync(cancellationToken);
        return Ok(ApiResponse<ClientProfileResponse>.Ok(result));
    }

    [HttpPut]
    public async Task<IActionResult> UpdateAsync([FromBody] UpdateClientProfileRequest request, CancellationToken cancellationToken)
    {
        await clientService.UpdateProfileAsync(request, cancellationToken);
        return Ok(ApiResponse<string>.Ok("تم تحديث الملف الشخصي بنجاح."));
    }

    [HttpDelete]
    public async Task<IActionResult> DeleteAsync(CancellationToken cancellationToken)
    {
        await clientService.DeleteProfileAsync(cancellationToken);
        return Ok(ApiResponse<string>.Ok("تم حذف الملف الشخصي بنجاح."));
    }
}
