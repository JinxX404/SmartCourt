using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartCourt.Common.Models;
using SmartCourt.Features.Consultations.Bookings;
using SmartCourt.Features.Consultations.DTOs;
using SmartCourt.Features.Consultations.Payments;

namespace SmartCourt.Features.Consultations.Controllers;

[ApiController]
[Authorize]
[Route("api/consultations")]
public sealed class ConsultationBookingsController(IMediator mediator) : ControllerBase
{
    [HttpPost("bookings")]
    [Authorize(Roles = "Client")]
    public async Task<ActionResult<ApiResponse<ConsultationBookingDto>>> Create(
        [FromBody] CreateConsultationBookingRequest request, CancellationToken token)
        => Respond(await mediator.Send(new CreateConsultationBookingCommand(request), token));

    [HttpGet("bookings/{bookingId:guid}")]
    [Authorize(Roles = "Client,Lawyer,FinanceAdministrator,SuperAdministrator")]
    public async Task<ActionResult<ApiResponse<ConsultationBookingDto>>> Get(Guid bookingId, CancellationToken token)
        => Respond(await mediator.Send(new GetConsultationBookingQuery(bookingId), token));

    [HttpGet("client/bookings")]
    [Authorize(Roles = "Client")]
    public async Task<ActionResult<ApiResponse<ConsultationPageDto<ConsultationBookingDto>>>> ClientBookings(
        [FromQuery] ConsultationBookingFilter filter, CancellationToken token)
        => Respond(await mediator.Send(new GetClientConsultationBookingsQuery(filter), token));

    [HttpGet("lawyer/bookings")]
    [Authorize(Roles = "Lawyer")]
    public async Task<ActionResult<ApiResponse<ConsultationPageDto<ConsultationBookingDto>>>> LawyerBookings(
        [FromQuery] ConsultationBookingFilter filter, CancellationToken token)
        => Respond(await mediator.Send(new GetLawyerConsultationBookingsQuery(filter), token));

    [HttpPost("bookings/{bookingId:guid}/payment-session")]
    [Authorize(Roles = "Client")]
    public async Task<ActionResult<ApiResponse<ConsultationPaymentDto>>> Pay(
        Guid bookingId,
        [FromBody] CreateConsultationPaymentSessionRequest request,
        [FromHeader(Name = "Idempotency-Key")] string? idempotencyKey,
        CancellationToken token)
        => Respond(await mediator.Send(new CreateConsultationPaymentSessionCommand(
            bookingId, request.ConfirmationTokenReference, idempotencyKey), token));

    [HttpPost("bookings/{bookingId:guid}/cancel")]
    [Authorize(Roles = "Client,Lawyer")]
    public async Task<ActionResult<ApiResponse<ConsultationBookingDto>>> Cancel(
        Guid bookingId, [FromBody] CancelConsultationBookingRequest request, CancellationToken token)
        => Respond(await mediator.Send(new CancelConsultationBookingCommand(bookingId, request.Reason), token));

    [HttpPost("bookings/{bookingId:guid}/mark-performed")]
    [Authorize(Roles = "Lawyer")]
    public async Task<ActionResult<ApiResponse<ConsultationBookingDto>>> MarkPerformed(
        Guid bookingId, [FromBody] MarkConsultationPerformedRequest request, CancellationToken token)
        => Respond(await mediator.Send(new MarkConsultationPerformedCommand(bookingId, request.MeetingUrl), token));

    [HttpPut("bookings/{bookingId:guid}/delivery-details")]
    [Authorize(Roles = "Lawyer")]
    public async Task<ActionResult<ApiResponse<ConsultationBookingDto>>> SetDeliveryDetails(
        Guid bookingId, [FromBody] SetConsultationDeliveryDetailsRequest request, CancellationToken token)
        => Respond(await mediator.Send(new SetConsultationDeliveryDetailsCommand(
            bookingId, request.MeetingUrl), token));

    [HttpPost("bookings/{bookingId:guid}/confirm-completion")]
    [Authorize(Roles = "Client")]
    public async Task<ActionResult<ApiResponse<ConsultationBookingDto>>> ConfirmCompletion(
        Guid bookingId, CancellationToken token)
        => Respond(await mediator.Send(new ConfirmConsultationCompletionCommand(bookingId), token));

    [HttpPost("bookings/{bookingId:guid}/disputes")]
    [Authorize(Roles = "Client")]
    public async Task<ActionResult<ApiResponse<ConsultationBookingDto>>> Dispute(
        Guid bookingId, [FromBody] OpenConsultationDisputeRequest request, CancellationToken token)
        => Respond(await mediator.Send(new OpenConsultationDisputeCommand(bookingId, request.Reason), token));

    private ActionResult<ApiResponse<T>> Respond<T>(ApiResponse<T> response)
        => StatusCode(response.StatusCode, response);
}
