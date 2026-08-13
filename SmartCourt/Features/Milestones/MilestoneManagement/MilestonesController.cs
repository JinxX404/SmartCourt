using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SmartCourt.Common.Models;
using SmartCourt.Common.RateLimiting;
using SmartCourt.Common.Validation;
using SmartCourt.Features.Milestones.DTOs;

namespace SmartCourt.Features.Milestones;

[ApiController]
[Route("api")]
[Authorize]
public sealed class MilestonesController(
    IMilestoneService milestoneService,
    IMilestoneDraftService milestoneDraftService,
    IMilestoneChangeRequestService milestoneChangeRequestService,
    IValidator<IfMatchRequest> ifMatchValidator) : ControllerBase
{
    [HttpPost("contracts/{contractId:guid}/milestones")]
    [SecurityRateLimit(RateLimitPolicyNames.StandardMutation)]
    [Authorize(Roles = "Lawyer")]
    public async Task<ActionResult<ApiResponse<MilestoneDto>>> AddAsync(
        Guid contractId,
        [FromBody] AddMilestoneRequest request,
        CancellationToken cancellationToken)
    {
        var milestone = await milestoneDraftService.AddAsync(
            contractId,
            request,
            cancellationToken);
        return StatusCode(
            StatusCodes.Status201Created,
            ApiResponse<MilestoneDto>.Created(milestone));
    }

    [HttpGet("contracts/{contractId:guid}/milestones")]
    [SecurityRateLimit(RateLimitPolicyNames.AuthenticatedQuery)]
    [Authorize(
        Roles = "Client,Lawyer,Moderator,SuperAdministrator")]
    public async Task<
        ActionResult<ApiResponse<IReadOnlyList<MilestoneDto>>>>
        ListAsync(
            Guid contractId,
            CancellationToken cancellationToken)
    {
        var milestones = await milestoneDraftService.ListAsync(
            contractId,
            cancellationToken);
        return Ok(
            ApiResponse<IReadOnlyList<MilestoneDto>>.Ok(milestones));
    }

    [HttpPut(
        "contracts/{contractId:guid}/milestones/{milestoneId:guid}")]
    [SecurityRateLimit(RateLimitPolicyNames.StandardMutation)]
    [Authorize(Roles = "Lawyer")]
    public async Task<ActionResult<ApiResponse<MilestoneDto>>>
        UpdateAsync(
            Guid contractId,
            Guid milestoneId,
            [FromBody] UpdateMilestoneRequest request,
            [FromHeader(Name = "If-Match")] string? ifMatch,
            CancellationToken cancellationToken)
    {
        var validatedIfMatch = await ValidateIfMatchAsync(
            ifMatch,
            cancellationToken);
        var milestone = await milestoneDraftService.UpdateDraftAsync(
            contractId,
            milestoneId,
            request,
            validatedIfMatch,
            cancellationToken);
        return Ok(ApiResponse<MilestoneDto>.Ok(milestone));
    }

    [HttpPost("milestones/{milestoneId:guid}/approve")]
    [SecurityRateLimit(RateLimitPolicyNames.SensitiveMutation)]
    [Authorize(Roles = "Client,Lawyer")]
    public async Task<
        ActionResult<ApiResponse<MilestoneActionResultDto>>>
        ApproveAsync(
            Guid milestoneId,
            [FromHeader(Name = "If-Match")] string? ifMatch,
            CancellationToken cancellationToken)
    {
        var validatedIfMatch = await ValidateIfMatchAsync(
            ifMatch,
            cancellationToken);
        var result = await milestoneService.ApproveAsync(
            milestoneId,
            validatedIfMatch,
            cancellationToken);
        return Ok(ApiResponse<MilestoneActionResultDto>.Ok(result));
    }

    [HttpPost("milestones/{milestoneId:guid}/ready-for-funding")]
    [SecurityRateLimit(RateLimitPolicyNames.SensitiveMutation)]
    [Authorize(Roles = "Lawyer")]
    public async Task<
        ActionResult<ApiResponse<MilestoneActionResultDto>>>
        MarkReadyForFundingAsync(
            Guid milestoneId,
            [FromHeader(Name = "If-Match")] string? ifMatch,
            CancellationToken cancellationToken)
    {
        var validatedIfMatch = await ValidateIfMatchAsync(
            ifMatch,
            cancellationToken);
        var result = await milestoneService.MarkReadyForFundingAsync(
            milestoneId,
            validatedIfMatch,
            cancellationToken);
        return Ok(ApiResponse<MilestoneActionResultDto>.Ok(result));
    }

    [HttpPost("milestones/{milestoneId:guid}/submit")]
    [SecurityRateLimit(RateLimitPolicyNames.SensitiveMutation)]
    [Authorize(Roles = "Lawyer")]
    public async Task<ActionResult<ApiResponse<MilestoneDto>>> SubmitAsync(
        Guid milestoneId,
        [FromBody] SubmitMilestoneRequest request,
        CancellationToken cancellationToken)
    {
        var milestone = await milestoneService.SubmitAsync(
            milestoneId,
            request,
            cancellationToken);
        return Ok(ApiResponse<MilestoneDto>.Ok(milestone));
    }

    [HttpPost("milestones/{milestoneId:guid}/accept")]
    [SecurityRateLimit(RateLimitPolicyNames.FinancialMutation)]
    [Authorize(Roles = "Client")]
    public async Task<ActionResult<ApiResponse<MilestoneDto>>> AcceptAsync(
        Guid milestoneId,
        CancellationToken cancellationToken)
    {
        var milestone = await milestoneService.AcceptAsync(
            milestoneId,
            cancellationToken);
        return Ok(ApiResponse<MilestoneDto>.Ok(milestone));
    }

    [HttpPost("milestones/{milestoneId:guid}/request-changes")]
    [SecurityRateLimit(RateLimitPolicyNames.StandardMutation)]
    [Authorize(Roles = "Client")]
    public async Task<ActionResult<ApiResponse<MilestoneDto>>>
        RequestChangesAsync(
            Guid milestoneId,
            [FromBody] RequestMilestoneChangesRequest request,
            CancellationToken cancellationToken)
    {
        var milestone = await milestoneService.RequestChangesAsync(
            milestoneId,
            request,
            cancellationToken);
        return Ok(ApiResponse<MilestoneDto>.Ok(milestone));
    }

    // [HttpPost("milestones/{milestoneId:guid}/change-requests")]
    // [SecurityRateLimit(RateLimitPolicyNames.StandardMutation)]
    // [Authorize(Roles = "Client,Lawyer")]
    // public async Task<
    //     ActionResult<ApiResponse<MilestoneActionResultDto>>>
    //     CreateChangeRequestAsync(
    //         Guid milestoneId,
    //         [FromBody] CreateMilestoneChangeRequest request,
    //         [FromHeader(Name = "If-Match")] string? ifMatch,
    //         CancellationToken cancellationToken)
    // {
    //     var validatedIfMatch = await ValidateIfMatchAsync(
    //         ifMatch,
    //         cancellationToken);
    //     var result = await milestoneChangeRequestService.CreateChangeRequestAsync(
    //         milestoneId,
    //         request,
    //         validatedIfMatch,
    //         cancellationToken);
    //     return StatusCode(
    //         StatusCodes.Status201Created,
    //         ApiResponse<MilestoneActionResultDto>.Created(result));
    // }

    // [HttpPost("change-requests/{changeRequestId:guid}/approve")]
    // [SecurityRateLimit(RateLimitPolicyNames.SensitiveMutation)]
    // [Authorize(Roles = "Client,Lawyer")]
    // public async Task<
    //     ActionResult<ApiResponse<MilestoneActionResultDto>>>
    //     ApproveChangeRequestAsync(
    //         Guid changeRequestId,
    //         [FromHeader(Name = "If-Match")] string? ifMatch,
    //         CancellationToken cancellationToken)
    // {
    //     var validatedIfMatch = await ValidateIfMatchAsync(
    //         ifMatch,
    //         cancellationToken);
    //     var result = await milestoneChangeRequestService.ApproveChangeRequestAsync(
    //         changeRequestId,
    //         validatedIfMatch,
    //         cancellationToken);
    //     return Ok(ApiResponse<MilestoneActionResultDto>.Ok(result));
    // }

    // [HttpPost("change-requests/{changeRequestId:guid}/reject")]
    // [SecurityRateLimit(RateLimitPolicyNames.StandardMutation)]
    // [Authorize(Roles = "Client,Lawyer")]
    // public async Task<
    //     ActionResult<ApiResponse<MilestoneActionResultDto>>>
    //     RejectChangeRequestAsync(
    //         Guid changeRequestId,
    //         [FromBody] RejectChangeRequest request,
    //         [FromHeader(Name = "If-Match")] string? ifMatch,
    //         CancellationToken cancellationToken)
    // {
    //     var validatedIfMatch = await ValidateIfMatchAsync(
    //         ifMatch,
    //         cancellationToken);
    //     var result = await milestoneChangeRequestService.RejectChangeRequestAsync(
    //         changeRequestId,
    //         request,
    //         validatedIfMatch,
    //         cancellationToken);
    //     return Ok(ApiResponse<MilestoneActionResultDto>.Ok(result));
    // }

    // [HttpPost("change-requests/{changeRequestId:guid}/cancel")]
    // [SecurityRateLimit(RateLimitPolicyNames.StandardMutation)]
    // [Authorize(Roles = "Client,Lawyer")]
    // public async Task<
    //     ActionResult<ApiResponse<MilestoneActionResultDto>>>
    //     CancelChangeRequestAsync(
    //         Guid changeRequestId,
    //         [FromHeader(Name = "If-Match")] string? ifMatch,
    //         CancellationToken cancellationToken)
    // {
    //     var validatedIfMatch = await ValidateIfMatchAsync(
    //         ifMatch,
    //         cancellationToken);
    //     var result = await milestoneChangeRequestService.CancelChangeRequestAsync(
    //         changeRequestId,
    //         validatedIfMatch,
    //         cancellationToken);
    //     return Ok(ApiResponse<MilestoneActionResultDto>.Ok(result));
    // }

    private async Task<string> ValidateIfMatchAsync(
        string? ifMatch,
        CancellationToken cancellationToken)
    {
        var request = new IfMatchRequest(ifMatch ?? string.Empty);
        var validationResult = await ifMatchValidator.ValidateAsync(
            request,
            cancellationToken);

        if (!validationResult.IsValid)
        {
            var error = validationResult.Errors.FirstOrDefault()?.ErrorMessage ?? "Invalid If-Match header.";
            throw new SmartCourt.Common.Exceptions.PreconditionFailedException(error);
        }

        return request.IfMatch;
    }
}


