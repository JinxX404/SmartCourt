using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartCourt.Common.Models;
using SmartCourt.Features.Consultations.Bookings;
using SmartCourt.Features.Consultations.DTOs;

namespace SmartCourt.Features.Consultations.Controllers;

[ApiController]
[Authorize(Roles = "FinanceAdministrator,SuperAdministrator")]
[Route("api/admin/consultations")]
public sealed class AdminConsultationsController(IMediator mediator) : ControllerBase
{
    [HttpPost("bookings/{bookingId:guid}/settle-dispute")]
    public async Task<ActionResult<ApiResponse<ConsultationBookingDto>>> Settle(
        Guid bookingId, [FromBody] SettleConsultationDisputeRequest request, CancellationToken token)
    {
        var response = await mediator.Send(new SettleConsultationDisputeCommand(
            bookingId, request.ClientRefundAmount, request.Reason), token);
        return StatusCode(response.StatusCode, response);
    }
}
