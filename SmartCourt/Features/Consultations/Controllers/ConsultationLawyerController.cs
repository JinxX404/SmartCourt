using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartCourt.Common.Models;
using SmartCourt.Features.Consultations.Availability;
using SmartCourt.Features.Consultations.DTOs;
using SmartCourt.Features.Consultations.Offerings;
using SmartCourt.Features.Consultations.Settings;

namespace SmartCourt.Features.Consultations.Controllers;

[ApiController]
[Authorize(Roles = "Lawyer")]
[Route("api/consultations/lawyer")]
public sealed class ConsultationLawyerController(IMediator mediator) : ControllerBase
{
    [HttpGet("settings")]
    public async Task<ActionResult<ApiResponse<ConsultationSettingsDto>>> GetSettings(CancellationToken token)
        => Respond(await mediator.Send(new GetMyConsultationSettingsQuery(), token));

    [HttpPut("settings")]
    public async Task<ActionResult<ApiResponse<ConsultationSettingsDto>>> UpdateSettings(
        [FromBody] UpdateConsultationSettingsRequest request, CancellationToken token)
        => Respond(await mediator.Send(new UpdateConsultationSettingsCommand(request), token));

    [HttpGet("offerings")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<ConsultationOfferingDto>>>> GetOfferings(CancellationToken token)
        => Respond(await mediator.Send(new GetMyConsultationOfferingsQuery(), token));

    [HttpPost("offerings")]
    public async Task<ActionResult<ApiResponse<ConsultationOfferingDto>>> CreateOffering(
        [FromBody] CreateConsultationOfferingRequest request, CancellationToken token)
        => Respond(await mediator.Send(new CreateConsultationOfferingCommand(request), token));

    [HttpPut("offerings/{offeringId:guid}")]
    public async Task<ActionResult<ApiResponse<ConsultationOfferingDto>>> UpdateOffering(
        Guid offeringId, [FromBody] UpdateConsultationOfferingRequest request, CancellationToken token)
        => Respond(await mediator.Send(new UpdateConsultationOfferingCommand(offeringId, request), token));

    [HttpPatch("offerings/{offeringId:guid}/status")]
    public async Task<ActionResult<ApiResponse<ConsultationOfferingDto>>> SetOfferingStatus(
        Guid offeringId, [FromBody] SetConsultationOfferingStatusRequest request, CancellationToken token)
        => Respond(await mediator.Send(new SetConsultationOfferingStatusCommand(offeringId, request.IsActive), token));

    [HttpPost("offerings/{offeringId:guid}/slots")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<ConsultationSlotDto>>>> CreateSlots(
        Guid offeringId, [FromBody] CreateConsultationSlotsRequest request, CancellationToken token)
        => Respond(await mediator.Send(new CreateConsultationSlotsCommand(offeringId, request), token));

    [HttpGet("offerings/{offeringId:guid}/slots")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<ConsultationSlotDto>>>> GetSlots(
        Guid offeringId, [FromQuery] DateTimeOffset? fromUtc, [FromQuery] DateTimeOffset? toUtc, CancellationToken token)
        => Respond(await mediator.Send(new GetConsultationSlotsQuery(offeringId, fromUtc, toUtc, true), token));

    [HttpDelete("slots/{slotId:guid}")]
    public async Task<ActionResult<ApiResponse<ConsultationSlotDto>>> CancelSlot(Guid slotId, CancellationToken token)
        => Respond(await mediator.Send(new CancelConsultationSlotCommand(slotId), token));

    private ActionResult<ApiResponse<T>> Respond<T>(ApiResponse<T> response)
        => StatusCode(response.StatusCode, response);
}
