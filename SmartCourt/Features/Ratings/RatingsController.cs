using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SmartCourt.Common.Models;
using SmartCourt.Common.RateLimiting;
using SmartCourt.Features.Ratings.DTOs;

namespace SmartCourt.Features.Ratings;

[ApiController]
[Route("api")]
[Authorize]
public sealed class RatingsController(IRatingService ratingService) : ControllerBase
{
    [HttpPost("contracts/{contractId:guid}/ratings")]
    [SecurityRateLimit(RateLimitPolicyNames.SensitiveMutation)]
    [Authorize(Roles = "Client,Lawyer")]
    [ProducesResponseType(
        typeof(ApiResponse<ContractRatingDto>),
        StatusCodes.Status201Created)]
    public async Task<ActionResult<ApiResponse<ContractRatingDto>>> SubmitAsync(
        [FromRoute] Guid contractId,
        [FromBody] SubmitRatingRequest request,
        CancellationToken cancellationToken)
    {
        var rating = await ratingService.SubmitAsync(
            contractId,
            request,
            cancellationToken);

        return StatusCode(
            StatusCodes.Status201Created,
            ApiResponse<ContractRatingDto>.Created(rating));
    }

    [HttpPut("contracts/{contractId:guid}/ratings")]
    [SecurityRateLimit(RateLimitPolicyNames.SensitiveMutation)]
    [Authorize(Roles = "Client,Lawyer")]
    [ProducesResponseType(
        typeof(ApiResponse<ContractRatingDto>),
        StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<ContractRatingDto>>> UpdateAsync(
        [FromRoute] Guid contractId,
        [FromBody] UpdateRatingRequest request,
        CancellationToken cancellationToken)
    {
        var rating = await ratingService.UpdateAsync(
            contractId,
            request,
            cancellationToken);

        return Ok(ApiResponse<ContractRatingDto>.Ok(rating));
    }

    [HttpGet("contracts/{contractId:guid}/ratings")]
    [SecurityRateLimit(RateLimitPolicyNames.AuthenticatedQuery)]
    [Authorize(Roles = "Client,Lawyer,Moderator,SuperAdministrator")]
    [ProducesResponseType(
        typeof(ApiResponse<ContractRatingSummaryDto>),
        StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<ContractRatingSummaryDto>>> GetByContractAsync(
        [FromRoute] Guid contractId,
        CancellationToken cancellationToken)
    {
        var summary = await ratingService.GetByContractAsync(
            contractId,
            cancellationToken);

        return Ok(ApiResponse<ContractRatingSummaryDto>.Ok(summary));
    }

    [HttpGet("lawyers/{lawyerUserId:guid}/ratings")]
    [AllowAnonymous]
    [SecurityRateLimit(RateLimitPolicyNames.PublicLawyerGet)]
    [ProducesResponseType(
        typeof(ApiResponse<PagedResult<ContractRatingDto>>),
        StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<PagedResult<ContractRatingDto>>>> GetByLawyerAsync(
        [FromRoute] Guid lawyerUserId,
        [FromQuery] LawyerRatingsQuery query,
        CancellationToken cancellationToken)
    {
        var result = await ratingService.GetByLawyerAsync(
            lawyerUserId,
            query,
            cancellationToken);

        return Ok(ApiResponse<PagedResult<ContractRatingDto>>.Ok(result));
    }
}
