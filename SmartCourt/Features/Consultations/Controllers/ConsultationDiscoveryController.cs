using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartCourt.Common.Models;
using SmartCourt.Features.Consultations.Availability;
using SmartCourt.Features.Consultations.Discovery;
using SmartCourt.Features.Consultations.DTOs;

namespace SmartCourt.Features.Consultations.Controllers;

[ApiController]
[AllowAnonymous]
[Route("api/consultations")]
public sealed class ConsultationDiscoveryController(IMediator mediator) : ControllerBase
{
    [HttpGet("lawyers")]
    public async Task<ActionResult<ApiResponse<ConsultationPageDto<ConsultationLawyerDto>>>> Search(
        [FromQuery] ConsultationLawyerFilter filter, CancellationToken token)
        => Respond(await mediator.Send(new SearchConsultationLawyersQuery(filter), token));

    [HttpGet("lawyers/{lawyerId:guid}")]
    public async Task<ActionResult<ApiResponse<ConsultationLawyerDto>>> GetLawyer(
        Guid lawyerId, CancellationToken token)
        => Respond(await mediator.Send(new GetConsultationLawyerQuery(lawyerId), token));

    [HttpGet("offerings/{offeringId:guid}/slots")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<ConsultationSlotDto>>>> GetSlots(
        Guid offeringId, [FromQuery] DateTime? fromUtc, [FromQuery] DateTime? toUtc, CancellationToken token)
        => Respond(await mediator.Send(new GetConsultationSlotsQuery(offeringId, fromUtc, toUtc), token));

    private ActionResult<ApiResponse<T>> Respond<T>(ApiResponse<T> response)
        => StatusCode(response.StatusCode, response);
}
