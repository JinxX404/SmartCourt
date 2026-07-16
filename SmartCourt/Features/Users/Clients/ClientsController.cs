using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartCourt.Common;
using SmartCourt.Features.Users.Clients.DTOs;

namespace SmartCourt.Features.Users.Clients;

[ApiController]
[Route("api/v1/clients")]
[Authorize]
public class ClientsController(IClientService clientService) : ControllerBase
{
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetAsync(Guid id)
    {
        /*
         * ALGORITHM:
         * 1. Verify that the authenticated user has permission to access this profile.
         * 2. Call IClientService.GetProfileAsync(id).
         * 3. Return ApiResponse<ClientProfileResponse>.Ok(result).
         */
        throw new NotImplementedException();
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateAsync(Guid id, [FromBody] UpdateClientProfileRequest request)
    {
        /*
         * ALGORITHM:
         * 1. Validate the incoming DTO (handled by FluentValidation pipeline).
         * 2. Verify that the authenticated user has permission to update this profile.
         * 3. Call IClientService.UpdateProfileAsync(id, request).
         * 4. Return ApiResponse<string>.Ok("Profile updated successfully.").
         */
        throw new NotImplementedException();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteAsync(Guid id)
    {
        /*
         * ALGORITHM:
         * 1. Verify that the authenticated user has permission to delete this profile.
         * 2. Call IClientService.DeleteProfileAsync(id).
         * 3. Return ApiResponse<string>.Ok("Profile deleted successfully.").
         */
        throw new NotImplementedException();
    }
}
