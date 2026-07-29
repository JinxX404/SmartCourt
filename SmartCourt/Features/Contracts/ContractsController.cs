using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SmartCourt.Common.Exceptions;
using SmartCourt.Common.Models;
using SmartCourt.Features.Contracts.DTOs;

namespace SmartCourt.Features.Contracts;

[ApiController]
[Route("api/contracts")]
[Authorize]
[Produces("application/json")]
public sealed class ContractsController(
    IContractService contractService,
    IValidator<IfMatchRequest> ifMatchValidator) : ControllerBase
{
    [HttpPost]
    [Authorize(Roles = "Lawyer")]
    [ProducesResponseType(
        typeof(ApiResponse<ContractDetailDto>),
        StatusCodes.Status201Created)]
    public async Task<ActionResult<ApiResponse<ContractDetailDto>>> CreateAsync(
        [FromBody] CreateContractRequest request,
        CancellationToken cancellationToken)
    {
        var contract = await contractService.CreateAsync(
            request,
            cancellationToken);
        return CreatedAtAction(
            nameof(GetAsync),
            new { contractId = contract.Id },
            ApiResponse<ContractDetailDto>.Created(contract));
    }

    [HttpGet]
    [Authorize(Roles = "Client,Lawyer")]
    [ProducesResponseType(
        typeof(ApiResponse<PagedResult<ContractSummaryDto>>),
        StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<PagedResult<ContractSummaryDto>>>>
        ListAsync(
            [FromQuery] ContractListQuery query,
            CancellationToken cancellationToken)
    {
        var contracts = await contractService.ListAsync(
            query,
            cancellationToken);
        return Ok(
            ApiResponse<PagedResult<ContractSummaryDto>>.Ok(contracts));
    }

    [HttpGet("{contractId:guid}")]
    [Authorize(
        Roles = "Client,Lawyer,Moderator,SuperAdministrator")]
    [ProducesResponseType(
        typeof(ApiResponse<ContractDetailDto>),
        StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<ContractDetailDto>>> GetAsync(
        Guid contractId,
        CancellationToken cancellationToken)
    {
        var contract = await contractService.GetAsync(
            contractId,
            cancellationToken);
        return Ok(ApiResponse<ContractDetailDto>.Ok(contract));
    }

    [HttpPut("{contractId:guid}")]
    [Authorize(Roles = "Lawyer")]
    [ProducesResponseType(
        typeof(ApiResponse<ContractDetailDto>),
        StatusCodes.Status200OK)]
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
    [Authorize(Roles = "Client,Lawyer")]
    [ProducesResponseType(
        typeof(ApiResponse<ContractActionResultDto>),
        StatusCodes.Status200OK)]
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
    [Authorize(Roles = "Client,Lawyer")]
    [ProducesResponseType(
        typeof(ApiResponse<ContractDetailDto>),
        StatusCodes.Status200OK)]
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
    [Authorize(
        Roles = "Client,Lawyer,Moderator,SuperAdministrator")]
    [ProducesResponseType(
        typeof(ApiResponse<PagedResult<ContractStateHistoryDto>>),
        StatusCodes.Status200OK)]
    public async Task<
        ActionResult<ApiResponse<PagedResult<ContractStateHistoryDto>>>>
        GetStateHistoryAsync(
            Guid contractId,
            [FromQuery] ContractStateHistoryQuery query,
            CancellationToken cancellationToken)
    {
        var history = await contractService.GetStateHistoryAsync(
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
