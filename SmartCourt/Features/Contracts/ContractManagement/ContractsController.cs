using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartCourt.Common.Models;
using SmartCourt.Common.RateLimiting;
using SmartCourt.Common.Validation;
using SmartCourt.Features.Contracts.DTOs;

namespace SmartCourt.Features.Contracts;

[ApiController]
[Route("api/contracts")]
[Authorize]
public sealed class ContractsController(
    IContractService contractService,
    IContractQueryService contractQueryService,
    IValidator<IfMatchRequest> ifMatchValidator) : ControllerBase
{
    [HttpPost]
    [SecurityRateLimit(RateLimitPolicyNames.SensitiveMutation)]
    [Authorize(Roles = "Lawyer")]
    public async Task<ActionResult<ApiResponse<ContractDetailDto>>> CreateAsync(
        [FromBody] CreateContractRequest request,
        CancellationToken cancellationToken)
    {
        var contract = await contractService.CreateAsync(
            request,
            cancellationToken);
        return StatusCode(201, ApiResponse<ContractDetailDto>.Created(contract));
    }

    [HttpGet]
    [SecurityRateLimit(RateLimitPolicyNames.AuthenticatedQuery)]
    [Authorize(Roles = "Client,Lawyer")]
    public async Task<ActionResult<ApiResponse<PagedResult<ContractSummaryDto>>>>
        ListAsync(
            [FromQuery] ContractListQuery query,
            CancellationToken cancellationToken)
    {
        var contracts = await contractQueryService.ListAsync(
            query,
            cancellationToken);
        return Ok(
            ApiResponse<PagedResult<ContractSummaryDto>>.Ok(contracts));
    }

    [HttpGet("{contractId:guid}")]
    [SecurityRateLimit(RateLimitPolicyNames.AuthenticatedQuery)]
    [Authorize(
        Roles = "Client,Lawyer,Moderator,SuperAdministrator")]
    public async Task<ActionResult<ApiResponse<ContractDetailDto>>> GetAsync(
        Guid contractId,
        CancellationToken cancellationToken)
    {
        var contract = await contractQueryService.GetAsync(
            contractId,
            cancellationToken);
        return Ok(ApiResponse<ContractDetailDto>.Ok(contract));
    }

    [HttpPut("{contractId:guid}")]
    [SecurityRateLimit(RateLimitPolicyNames.StandardMutation)]
    [Authorize(Roles = "Lawyer")]
    public async Task<ActionResult<ApiResponse<ContractDetailDto>>>
        UpdateAsync(
            Guid contractId,
            [FromBody] UpdateContractRequest request,
            [FromHeader(Name = "If-Match")] string? ifMatch,
            CancellationToken cancellationToken)
    {
        var validatedIfMatch = await ValidateIfMatchAsync(
            ifMatch,
            cancellationToken);
        var contract = await contractService.UpdateDraftAsync(
            contractId,
            request,
            validatedIfMatch,
            cancellationToken);
        return Ok(ApiResponse<ContractDetailDto>.Ok(contract));
    }

    [HttpPost("{contractId:guid}/accept")]
    [SecurityRateLimit(RateLimitPolicyNames.SensitiveMutation)]
    [Authorize(Roles = "Client,Lawyer")]
    public async Task<ActionResult<ApiResponse<ContractActionResultDto>>>
        AcceptAsync(
            Guid contractId,
            [FromHeader(Name = "If-Match")] string? ifMatch,
            CancellationToken cancellationToken)
    {
        var validatedIfMatch = await ValidateIfMatchAsync(
            ifMatch,
            cancellationToken);
        var result = await contractService.AcceptAsync(
            contractId,
            validatedIfMatch,
            cancellationToken);
        return Ok(ApiResponse<ContractActionResultDto>.Ok(result));
    }

    [HttpPost("{contractId:guid}/terminate")]
    [SecurityRateLimit(RateLimitPolicyNames.SensitiveMutation)]
    [Authorize(Roles = "Client,Lawyer")]
    public async Task<ActionResult<ApiResponse<ContractDetailDto>>>
        TerminateAsync(
            Guid contractId,
            [FromBody] TerminateContractRequest request,
            [FromHeader(Name = "If-Match")] string? ifMatch,
            CancellationToken cancellationToken)
    {
        var validatedIfMatch = await ValidateIfMatchAsync(
            ifMatch,
            cancellationToken);
        var contract = await contractService.TerminateAsync(
            contractId,
            request,
            validatedIfMatch,
            cancellationToken);
        return Ok(ApiResponse<ContractDetailDto>.Ok(contract));
    }

    [HttpGet("{contractId:guid}/state-history")]
    [SecurityRateLimit(RateLimitPolicyNames.AuthenticatedQuery)]
    [Authorize(
        Roles = "Client,Lawyer,Moderator,SuperAdministrator")]
    public async Task<
        ActionResult<ApiResponse<PagedResult<ContractStateHistoryDto>>>>
        GetStateHistoryAsync(
            Guid contractId,
            [FromQuery] ContractStateHistoryQuery query,
            CancellationToken cancellationToken)
    {
        var history = await contractQueryService.GetStateHistoryAsync(
            contractId,
            query,
            cancellationToken);
        return Ok(
            ApiResponse<PagedResult<ContractStateHistoryDto>>.Ok(history));
    }

    private async Task<string> ValidateIfMatchAsync(
        string? ifMatch,
        CancellationToken cancellationToken)
    {
        var request = new IfMatchRequest(ifMatch ?? string.Empty);
        await ifMatchValidator.ValidateAndThrowBusinessExceptionAsync(
            request,
            cancellationToken);
        return request.IfMatch;
    }
}


