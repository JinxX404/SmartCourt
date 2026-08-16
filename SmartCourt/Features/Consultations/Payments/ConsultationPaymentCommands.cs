using MediatR;
using SmartCourt.Common.Models;
using SmartCourt.Features.Consultations.DTOs;

namespace SmartCourt.Features.Consultations.Payments;

public sealed record CreateConsultationPaymentSessionCommand(
    Guid BookingId,
    string ConfirmationTokenReference,
    string? IdempotencyKey)
    : IRequest<ApiResponse<ConsultationPaymentDto>>;

public sealed class CreateConsultationPaymentSessionHandler(
    IConsultationPaymentService paymentService)
    : IRequestHandler<CreateConsultationPaymentSessionCommand, ApiResponse<ConsultationPaymentDto>>
{
    public async Task<ApiResponse<ConsultationPaymentDto>> Handle(
        CreateConsultationPaymentSessionCommand command,
        CancellationToken cancellationToken) =>
        ApiResponse<ConsultationPaymentDto>.Ok(await paymentService.FundAsync(
            command.BookingId, command.ConfirmationTokenReference,
            command.IdempotencyKey, cancellationToken));
}
