using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartCourt.Common.Models;
using SmartCourt.Common.RateLimiting;
using SmartCourt.Common.Validation;
using SmartCourt.Features.Disputes.DTOs;

namespace SmartCourt.Features.Disputes.Penalties;

[ApiController]
[Route("api")]
[Authorize]
public sealed class LawyerPenaltiesController(
    ILawyerPenaltyService penaltyService,
    IValidator<RevokeLawyerPenaltyRequest> revokeValidator,
    IValidator<LawyerPenaltyFilterQuery> filterValidator) : ControllerBase
{
    [HttpGet("admin/lawyer-penalties")]
    [SecurityRateLimit(RateLimitPolicyNames.AuthenticatedQuery)]
    [Authorize(Roles = "Moderator,SuperAdministrator")]
    public async Task<ActionResult<ApiResponse<PagedResult<LawyerPenaltyDto>>>> ListAsync(
        [FromQuery] LawyerPenaltyFilterQuery query,
        CancellationToken cancellationToken)
    {
        await filterValidator.ValidateAndThrowBusinessExceptionAsync(query, cancellationToken);
        var result = await penaltyService.ListAsync(query, cancellationToken);
        return Ok(ApiResponse<PagedResult<LawyerPenaltyDto>>.Ok(result));
    }

    [HttpGet("lawyer-penalties/me")]
    [SecurityRateLimit(RateLimitPolicyNames.AuthenticatedQuery)]
    [Authorize(Roles = "Lawyer")]
    public async Task<ActionResult<ApiResponse<PagedResult<LawyerPenaltyDto>>>> GetMyPenaltiesAsync(
        [FromQuery] LawyerPenaltyFilterQuery query,
        CancellationToken cancellationToken)
    {
        await filterValidator.ValidateAndThrowBusinessExceptionAsync(query, cancellationToken);
        var result = await penaltyService.GetMyPenaltiesAsync(query, cancellationToken);
        return Ok(ApiResponse<PagedResult<LawyerPenaltyDto>>.Ok(result));
    }

    [HttpPost("admin/lawyer-penalties/{penaltyId:guid}/revoke")]
    [SecurityRateLimit(RateLimitPolicyNames.StandardMutation)]
    [Authorize(Roles = "SuperAdministrator")]
    public async Task<ActionResult<ApiResponse<LawyerPenaltyDto>>> RevokeAsync(
        Guid penaltyId,
        [FromBody] RevokeLawyerPenaltyRequest request,
        CancellationToken cancellationToken)
    {
        await revokeValidator.ValidateAndThrowBusinessExceptionAsync(request, cancellationToken);
        var result = await penaltyService.RevokeAsync(
            penaltyId,
            request,
            cancellationToken);
        return Ok(ApiResponse<LawyerPenaltyDto>.Ok(result));
    }
}
