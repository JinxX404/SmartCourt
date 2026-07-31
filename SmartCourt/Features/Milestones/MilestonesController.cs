using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SmartCourt.Common.Exceptions;
using SmartCourt.Common.Models;
using SmartCourt.Features.Contracts.DTOs;
using SmartCourt.Features.Milestones.DTOs;
using SmartCourt.Features.Contracts.Validators;

namespace SmartCourt.Features.Milestones;

[ApiController]
[Route("api")]
[Authorize]
[Produces("application/json")]
public sealed class MilestonesController(
    IMilestoneService milestoneService,
    IMilestoneDraftService milestoneDraftService,
    IMilestoneChangeRequestService milestoneChangeRequestService,
    IValidator<IfMatchRequest> ifMatchValidator) : ControllerBase
{
    [HttpPost("contracts/{contractId:guid}/milestones")]
    [Authorize(Roles = "Client,Lawyer")]
    [ProducesResponseType(
        typeof(ApiResponse<MilestoneDto>),
        StatusCodes.Status201Created)]
    public async Task<ActionResult<ApiResponse<MilestoneDto>>> AddAsync(
        Guid contractId,
        [FromBody] AddMilestoneRequest request,
        CancellationToken cancellationToken)
    {
        var milestone = await milestoneDraftService.AddAsync(
            contractId,
            request,
            cancellationToken);
        return CreatedAtAction(
            nameof(ListAsync),
            new { contractId },
            ApiResponse<MilestoneDto>.Created(milestone));
    }

    [HttpGet("contracts/{contractId:guid}/milestones")]
    [Authorize(
        Roles = "Client,Lawyer,Moderator,SuperAdministrator")]
    [ProducesResponseType(
        typeof(ApiResponse<IReadOnlyList<MilestoneDto>>),
        StatusCodes.Status200OK)]
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
    [Authorize(Roles = "Client,Lawyer")]
    [ProducesResponseType(
        typeof(ApiResponse<MilestoneDto>),
        StatusCodes.Status200OK)]
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
    [Authorize(Roles = "Client,Lawyer")]
    [ProducesResponseType(
        typeof(ApiResponse<MilestoneActionResultDto>),
        StatusCodes.Status200OK)]
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
    [Authorize(Roles = "Lawyer")]
    [ProducesResponseType(
        typeof(ApiResponse<MilestoneActionResultDto>),
        StatusCodes.Status200OK)]
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
    [Authorize(Roles = "Lawyer")]
    [ProducesResponseType(
        typeof(ApiResponse<MilestoneDto>),
        StatusCodes.Status200OK)]
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
    [Authorize(Roles = "Client")]
    [ProducesResponseType(
        typeof(ApiResponse<MilestoneDto>),
        StatusCodes.Status200OK)]
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
    [Authorize(Roles = "Client")]
    [ProducesResponseType(
        typeof(ApiResponse<MilestoneDto>),
        StatusCodes.Status200OK)]
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

    [HttpPost("milestones/{milestoneId:guid}/change-requests")]
    [Authorize(Roles = "Client,Lawyer")]
    [ProducesResponseType(
        typeof(ApiResponse<MilestoneActionResultDto>),
        StatusCodes.Status201Created)]
    public async Task<
        ActionResult<ApiResponse<MilestoneActionResultDto>>>
        CreateChangeRequestAsync(
            Guid milestoneId,
            [FromBody] CreateMilestoneChangeRequest request,
            [FromHeader(Name = "If-Match")] string? ifMatch,
            CancellationToken cancellationToken)
    {
        var validatedIfMatch = await ValidateIfMatchAsync(
            ifMatch,
            cancellationToken);
        var result = await milestoneChangeRequestService.CreateChangeRequestAsync(
            milestoneId,
            request,
            validatedIfMatch,
            cancellationToken);
        return StatusCode(
            StatusCodes.Status201Created,
            ApiResponse<MilestoneActionResultDto>.Created(result));
    }

    [HttpPost("change-requests/{changeRequestId:guid}/approve")]
    [Authorize(Roles = "Client,Lawyer")]
    [ProducesResponseType(
        typeof(ApiResponse<MilestoneActionResultDto>),
        StatusCodes.Status200OK)]
    public async Task<
        ActionResult<ApiResponse<MilestoneActionResultDto>>>
        ApproveChangeRequestAsync(
            Guid changeRequestId,
            [FromHeader(Name = "If-Match")] string? ifMatch,
            CancellationToken cancellationToken)
    {
        var validatedIfMatch = await ValidateIfMatchAsync(
            ifMatch,
            cancellationToken);
        var result = await milestoneChangeRequestService.ApproveChangeRequestAsync(
            changeRequestId,
            validatedIfMatch,
            cancellationToken);
        return Ok(ApiResponse<MilestoneActionResultDto>.Ok(result));
    }

    [HttpPost("change-requests/{changeRequestId:guid}/reject")]
    [Authorize(Roles = "Client,Lawyer")]
    [ProducesResponseType(
        typeof(ApiResponse<MilestoneActionResultDto>),
        StatusCodes.Status200OK)]
    public async Task<
        ActionResult<ApiResponse<MilestoneActionResultDto>>>
        RejectChangeRequestAsync(
            Guid changeRequestId,
            [FromBody] RejectChangeRequest request,
            [FromHeader(Name = "If-Match")] string? ifMatch,
            CancellationToken cancellationToken)
    {
        var validatedIfMatch = await ValidateIfMatchAsync(
            ifMatch,
            cancellationToken);
        var result = await milestoneChangeRequestService.RejectChangeRequestAsync(
            changeRequestId,
            request,
            validatedIfMatch,
            cancellationToken);
        return Ok(ApiResponse<MilestoneActionResultDto>.Ok(result));
    }

    [HttpPost("change-requests/{changeRequestId:guid}/cancel")]
    [Authorize(Roles = "Client,Lawyer")]
    [ProducesResponseType(
        typeof(ApiResponse<MilestoneActionResultDto>),
        StatusCodes.Status200OK)]
    public async Task<
        ActionResult<ApiResponse<MilestoneActionResultDto>>>
        CancelChangeRequestAsync(
            Guid changeRequestId,
            [FromHeader(Name = "If-Match")] string? ifMatch,
            CancellationToken cancellationToken)
    {
        var validatedIfMatch = await ValidateIfMatchAsync(
            ifMatch,
            cancellationToken);
        var result = await milestoneChangeRequestService.CancelChangeRequestAsync(
            changeRequestId,
            validatedIfMatch,
            cancellationToken);
        return Ok(ApiResponse<MilestoneActionResultDto>.Ok(result));
    }

    private async Task<string> ValidateIfMatchAsync(
        string? ifMatch,
        CancellationToken cancellationToken)
    {
        var request = new IfMatchRequest(ifMatch ?? string.Empty);
        var result = await ifMatchValidator.ValidateAsync(
            request,
            cancellationToken);
        if (!result.IsValid)
        {
            throw new BusinessException(
                string.Join(
                    " ",
                    result.Errors
                        .Select(error => error.ErrorMessage)
                        .Distinct(StringComparer.Ordinal)));
        }

        return request.IfMatch;
    }
}
