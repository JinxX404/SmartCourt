using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartCourt.Common.Models;
using SmartCourt.Common.RateLimiting;
using SmartCourt.Common.Validation;
using SmartCourt.Features.Disputes.DTOs;

namespace SmartCourt.Features.Disputes;

[ApiController]
[Route("api")]
[Authorize]
public sealed class DisputesController(
    IDisputeService disputeService,
    IValidator<CreateDisputeRequest> createValidator,
    IValidator<AddDisputeEvidenceRequest> evidenceValidator,
    IValidator<AssignDisputeRequest> assignValidator,
    IValidator<ResolveDisputeRequest> resolveValidator,
    IValidator<DisputeListQuery> listValidator) : ControllerBase
{
    [HttpPost("disputes")]
    [SecurityRateLimit(RateLimitPolicyNames.SensitiveMutation)]
    [Authorize(Roles = "Client,Lawyer")]
    public async Task<ActionResult<ApiResponse<DisputeDto>>> CreateAsync(
        [FromBody] CreateDisputeRequest request,
        CancellationToken cancellationToken)
    {
        await createValidator.ValidateAndThrowBusinessExceptionAsync(request, cancellationToken);
        var dispute = await disputeService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(
            nameof(GetAsync),
            new { disputeId = dispute.Id },
            ApiResponse<DisputeDto>.Created(dispute));
    }

    [HttpGet("disputes")]
    [SecurityRateLimit(RateLimitPolicyNames.AuthenticatedQuery)]
    [Authorize(Roles = "Client,Lawyer,Moderator,SuperAdministrator")]
    public async Task<ActionResult<ApiResponse<PagedResult<DisputeDto>>>> ListAsync(
        [FromQuery] DisputeListQuery query,
        CancellationToken cancellationToken)
    {
        await listValidator.ValidateAndThrowBusinessExceptionAsync(query, cancellationToken);
        var result = await disputeService.ListAsync(query, cancellationToken);
        return Ok(ApiResponse<PagedResult<DisputeDto>>.Ok(result));
    }

    [HttpGet("disputes/{disputeId:guid}")]
    [SecurityRateLimit(RateLimitPolicyNames.AuthenticatedQuery)]
    [Authorize(Roles = "Client,Lawyer,Moderator,SuperAdministrator")]
    public async Task<ActionResult<ApiResponse<DisputeDto>>> GetAsync(
        Guid disputeId,
        CancellationToken cancellationToken)
    {
        var dispute = await disputeService.GetAsync(disputeId, cancellationToken);
        return Ok(ApiResponse<DisputeDto>.Ok(dispute));
    }

    [HttpPost("disputes/{disputeId:guid}/evidence")]
    [SecurityRateLimit(RateLimitPolicyNames.SensitiveMutation)]
    [Authorize(Roles = "Client,Lawyer,Moderator,SuperAdministrator")]
    public async Task<ActionResult<ApiResponse<DisputeActionResultDto>>>
        AddEvidenceAsync(
            Guid disputeId,
            [FromBody] AddDisputeEvidenceRequest request,
            CancellationToken cancellationToken)
    {
        await evidenceValidator.ValidateAndThrowBusinessExceptionAsync(request, cancellationToken);
        var result = await disputeService.AddEvidenceAsync(
            disputeId,
            request,
            cancellationToken);
        return Ok(ApiResponse<DisputeActionResultDto>.Ok(result));
    }

    [HttpPost("admin/disputes/{disputeId:guid}/assign")]
    [SecurityRateLimit(RateLimitPolicyNames.StandardMutation)]
    [Authorize(Roles = "Moderator,SuperAdministrator")]
    public async Task<ActionResult<ApiResponse<DisputeDto>>> AssignAsync(
        Guid disputeId,
        [FromBody] AssignDisputeRequest request,
        CancellationToken cancellationToken)
    {
        await assignValidator.ValidateAndThrowBusinessExceptionAsync(request, cancellationToken);
        var dispute = await disputeService.AssignAsync(
            disputeId,
            request,
            cancellationToken);
        return Ok(ApiResponse<DisputeDto>.Ok(dispute));
    }

    [HttpPost("admin/disputes/{disputeId:guid}/review")]
    [SecurityRateLimit(RateLimitPolicyNames.StandardMutation)]
    [Authorize(Roles = "Moderator,SuperAdministrator")]
    public async Task<ActionResult<ApiResponse<DisputeDto>>> StartReviewAsync(
        Guid disputeId,
        CancellationToken cancellationToken)
    {
        var dispute = await disputeService.StartReviewAsync(
            disputeId,
            cancellationToken);
        return Ok(ApiResponse<DisputeDto>.Ok(dispute));
    }

    [HttpPost("admin/disputes/{disputeId:guid}/resolve")]
    [SecurityRateLimit(RateLimitPolicyNames.AdminFinancialMutation)]
    [Authorize(Roles = "Moderator,SuperAdministrator")]
    public async Task<ActionResult<ApiResponse<DisputeDto>>> ResolveAsync(
        Guid disputeId,
        [FromBody] ResolveDisputeRequest request,
        [FromHeader(Name = "Idempotency-Key")] string? idempotencyKey,
        CancellationToken cancellationToken)
    {
        await resolveValidator.ValidateAndThrowBusinessExceptionAsync(request, cancellationToken);
        var dispute = await disputeService.ResolveAsync(
            disputeId,
            request,
            idempotencyKey,
            cancellationToken);
        return Ok(ApiResponse<DisputeDto>.Ok(dispute));
    }

    [HttpPost("admin/disputes/{disputeId:guid}/close")]
    [SecurityRateLimit(RateLimitPolicyNames.StandardMutation)]
    [Authorize(Roles = "Moderator,SuperAdministrator")]
    public async Task<ActionResult<ApiResponse<DisputeActionResultDto>>> CloseAsync(
        Guid disputeId,
        CancellationToken cancellationToken)
    {
        var result = await disputeService.CloseAsync(disputeId, cancellationToken);
        return Ok(ApiResponse<DisputeActionResultDto>.Ok(result));
    }
}


