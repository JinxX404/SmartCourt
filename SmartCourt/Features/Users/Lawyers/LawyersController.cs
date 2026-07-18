using SmartCourt.Features.Users.Lawyers.DTOs;
using SmartCourt.Common.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using SmartCourt.Common.Exceptions;

namespace SmartCourt.Features.Users.Lawyers;

[Route("api/lawyers")]
[ApiController]
[Authorize(Roles = "Lawyer")]
public class LawyersController : ControllerBase
{
    private readonly ILawyerService _lawyerService;

    public LawyersController(ILawyerService lawyerService)
    {
        _lawyerService = lawyerService;
    }

    [HttpGet("profile")]
    public async Task<IActionResult> GetProfile(CancellationToken cancellationToken)
    {
        var result = await _lawyerService.GetProfileAsync(cancellationToken);
        return Ok(ApiResponse<LawyerProfileResponse>.Ok(result));
    }

    [HttpPut("profile")]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateLawyerProfileRequest request, CancellationToken cancellationToken)
    {
        await _lawyerService.UpdateProfileAsync(request, cancellationToken);
        return Ok(ApiResponse<string>.Ok("تم تحديث البيانات بنجاح"));
    }

    [HttpDelete("profile")]
    public async Task<IActionResult> DeleteProfile(CancellationToken cancellationToken)
    {
        await _lawyerService.DeleteProfileAsync(cancellationToken);
        return Ok(ApiResponse<string>.Ok("تم حذف الحساب بنجاح"));
    }

    [HttpGet("public/{id:guid}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetPublicProfile(Guid id, CancellationToken cancellationToken)
    {
        var result = await _lawyerService.GetPublicProfileAsync(id, cancellationToken);
        return Ok(ApiResponse<PublicLawyerProfileResponse>.Ok(result));
    }
}
