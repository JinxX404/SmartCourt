using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartCourt.Common.Models;
using SmartCourt.Features.Proposals.AcceptProposal;
using SmartCourt.Features.Proposals.CancelProposal;
using SmartCourt.Features.Proposals.CreateProposal;
using SmartCourt.Features.Proposals.DTOs;
using SmartCourt.Features.Proposals.GetProposal;
using SmartCourt.Features.Proposals.GetProposalAvailability;
using SmartCourt.Features.Proposals.GetProposals;
using SmartCourt.Features.Proposals.RejectProposal;
using SmartCourt.Features.Proposals.TerminateProposal;

namespace SmartCourt.Features.Proposals;

[ApiController]
[Authorize]
[Route("api/proposals")]
[Produces("application/json")]
public sealed class ProposalsController(IMediator mediator) : ControllerBase
{
    [HttpPost]
    [Authorize(Roles = "Client")]
    [ProducesResponseType(
        typeof(ApiResponse<ProposalDetailDto>),
        StatusCodes.Status201Created)]
    public async Task<ActionResult<ApiResponse<ProposalDetailDto>>> CreateAsync(
        [FromBody] CreateProposalRequest request,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new CreateProposalCommand(
                request.LegalCaseId,
                request.LawyerUserId,
                request.Message),
            cancellationToken);

        return StatusCode(result.StatusCode, result);
    }

    [HttpGet("cases/{legalCaseId:guid}/availability")]
    [Authorize(Roles = "Client")]
    [ProducesResponseType(
        typeof(ApiResponse<ProposalSlotAvailabilityDto>),
        StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<ProposalSlotAvailabilityDto>>>
        GetAvailabilityAsync(
            Guid legalCaseId,
            CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new GetProposalAvailabilityQuery(legalCaseId),
            cancellationToken);
        return StatusCode(result.StatusCode, result);
    }

    [HttpGet]
    [Authorize(Roles = "Client,Lawyer")]
    [ProducesResponseType(
        typeof(ApiResponse<ProposalPageDto>),
        StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<ProposalPageDto>>> ListAsync(
        [FromQuery] GetProposalsQuery query,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(query, cancellationToken);
        return StatusCode(result.StatusCode, result);
    }

    [HttpGet("{proposalId:guid}")]
    [Authorize(Roles = "Client,Lawyer")]
    [ProducesResponseType(
        typeof(ApiResponse<ProposalDetailDto>),
        StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<ProposalDetailDto>>> GetAsync(
        Guid proposalId,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new GetProposalQuery(proposalId),
            cancellationToken);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPost("{proposalId:guid}/accept")]
    [Authorize(Roles = "Lawyer")]
    [ProducesResponseType(
        typeof(ApiResponse<ProposalDetailDto>),
        StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<ProposalDetailDto>>> AcceptAsync(
        Guid proposalId,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new AcceptProposalCommand(proposalId),
            cancellationToken);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPost("{proposalId:guid}/reject")]
    [Authorize(Roles = "Lawyer")]
    [ProducesResponseType(
        typeof(ApiResponse<ProposalDetailDto>),
        StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<ProposalDetailDto>>> RejectAsync(
        Guid proposalId,
        [FromBody] RejectProposalRequest request,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new RejectProposalCommand(proposalId, request.Reason),
            cancellationToken);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPost("{proposalId:guid}/cancel")]
    [Authorize(Roles = "Client")]
    [ProducesResponseType(
        typeof(ApiResponse<ProposalDetailDto>),
        StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<ProposalDetailDto>>> CancelAsync(
        Guid proposalId,
        [FromBody] CancelProposalRequest request,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new CancelProposalCommand(proposalId, request.Reason),
            cancellationToken);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPost("{proposalId:guid}/terminate")]
    [Authorize(Roles = "Client,Lawyer")]
    [ProducesResponseType(
        typeof(ApiResponse<ProposalDetailDto>),
        StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<ProposalDetailDto>>> TerminateAsync(
        Guid proposalId,
        [FromBody] TerminateProposalRequest request,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new TerminateProposalCommand(proposalId, request.Reason),
            cancellationToken);
        return StatusCode(result.StatusCode, result);
    }
}
