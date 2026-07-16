using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartCourt.Common;
using SmartCourt.Common.Attributes;
using SmartCourt.Features.Users.Lawyers.DTOs;

namespace SmartCourt.Features.Users.Lawyers;

[ApiController]
[Route("api/v1/lawyers")]
[Authorize]
public class LawyersController(ILawyerService lawyerService) : ControllerBase
{
    [HttpGet("{id:guid}")]
    [AuthorizeOwner]
    public async Task<IActionResult> GetAsync(Guid id)
    {
        var result = await lawyerService.GetProfileAsync(id);
        return Ok(ApiResponse<LawyerProfileResponse>.Ok(result));
    }

    [HttpPut("{id:guid}")]
    [AuthorizeOwner]
    public async Task<IActionResult> UpdateAsync(Guid id, [FromBody] UpdateLawyerProfileRequest request)
    {
        await lawyerService.UpdateProfileAsync(id, request);
        return Ok(ApiResponse<string>.Ok("تم تحديث الملف الشخصي بنجاح."));
    }

    [HttpDelete("{id:guid}")]
    [AuthorizeOwner]
    public async Task<IActionResult> DeleteAsync(Guid id)
    {
        await lawyerService.DeleteProfileAsync(id);
        return Ok(ApiResponse<string>.Ok("تم حذف الملف الشخصي بنجاح."));
    }
}
