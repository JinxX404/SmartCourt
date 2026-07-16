using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartCourt.Common;
using SmartCourt.Common.Attributes;
using SmartCourt.Features.Users.Clients.DTOs;

namespace SmartCourt.Features.Users.Clients;

[ApiController]
[Route("api/v1/clients")]
[Authorize]
public class ClientsController(IClientService clientService) : ControllerBase
{
    [HttpGet("{id:guid}")]
    [AuthorizeOwner]
    public async Task<IActionResult> GetAsync(Guid id)
    {
        var result = await clientService.GetProfileAsync(id);
        return Ok(ApiResponse<ClientProfileResponse>.Ok(result));
    }

    [HttpPut("{id:guid}")]
    [AuthorizeOwner]
    public async Task<IActionResult> UpdateAsync(Guid id, [FromBody] UpdateClientProfileRequest request)
    {
        await clientService.UpdateProfileAsync(id, request);
        return Ok(ApiResponse<string>.Ok("تم تحديث الملف الشخصي بنجاح."));
    }

    [HttpDelete("{id:guid}")]
    [AuthorizeOwner]
    public async Task<IActionResult> DeleteAsync(Guid id)
    {
        await clientService.DeleteProfileAsync(id);
        return Ok(ApiResponse<string>.Ok("تم حذف الملف الشخصي بنجاح."));
    }
}
