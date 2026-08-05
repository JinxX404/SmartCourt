using SmartCourt.Features.Users.Lawyers.DTOs;
using SmartCourt.Common.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using SmartCourt.Common.Exceptions;
using SmartCourt.Common.RateLimiting;
using SmartCourt.Features.Users.Shared.DTOs;

namespace SmartCourt.Features.Users.Lawyers;

[Route("api/lawyers")]
[ApiController]
public class LawyersController : ControllerBase
{
    private readonly ILawyerService _lawyerService;

    public LawyersController(ILawyerService lawyerService)
    {
        _lawyerService = lawyerService;
    }

    [HttpGet("profile")]
    [Authorize(Roles = "Lawyer")]
    [SecurityRateLimit(RateLimitPolicyNames.PrivateProfileGet)]
    public async Task<IActionResult> GetProfile(CancellationToken cancellationToken)
    {
        var result = await _lawyerService.GetProfileAsync(cancellationToken);
        return Ok(ApiResponse<LawyerProfileResponse>.Ok(result));
    }

    [HttpPost("profile/complete")]
    [Authorize(Roles = "Lawyer")]
    [SecurityRateLimit(RateLimitPolicyNames.PrivateProfileUpdate)] // Using same rate limit as update for now
    public async Task<IActionResult> CompleteProfile([FromBody] CompleteLawyerProfileRequest request, CancellationToken cancellationToken)
    {
        await _lawyerService.CompleteProfileAsync(request, cancellationToken);
        return Ok(ApiResponse.Ok("تم استكمال البيانات بنجاح"));
    }

    [HttpPut("profile")]
    [Authorize(Roles = "Lawyer")]
    [SecurityRateLimit(RateLimitPolicyNames.PrivateProfileUpdate)]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateLawyerProfileRequest request, CancellationToken cancellationToken)
    {
        await _lawyerService.UpdateProfileAsync(request, cancellationToken);
        return Ok(ApiResponse.Ok("تم تحديث البيانات بنجاح"));
    }

    [HttpDelete("profile")]
    [Authorize(Roles = "Lawyer")]
    [SecurityRateLimit(RateLimitPolicyNames.PrivateProfileDelete)]
    public async Task<IActionResult> DeleteProfile(
        [FromBody] DeleteAccountRequest request,
        CancellationToken cancellationToken)
    {
        await _lawyerService.DeleteProfileAsync(request, cancellationToken);
        return Ok(ApiResponse.Ok("تم حذف الحساب بنجاح"));
    }

    [HttpGet("search")]
    [Authorize]
    [SecurityRateLimit(RateLimitPolicyNames.PublicLawyerGet)]
    public async Task<IActionResult> SearchLawyers(
        [FromQuery] SearchLawyersRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _lawyerService.SearchLawyersAsync(request, cancellationToken);
        return Ok(result);
    }

    [HttpGet("public/{id:guid}")]
    [AllowAnonymous]
    [SecurityRateLimit(RateLimitPolicyNames.PublicLawyerGet)]
    public async Task<IActionResult> GetPublicProfile(Guid id, CancellationToken cancellationToken)
    {
        var result = await _lawyerService.GetPublicProfileAsync(id, cancellationToken);
        return Ok(ApiResponse<PublicLawyerProfileResponse>.Ok(result));
    }
}
