using SmartCourt.Features.Consultations.DTOs;

namespace SmartCourt.Features.Consultations.Payments;

public interface IConsultationPaymentService
{
    Task<ConsultationPaymentDto> FundAsync(
        Guid bookingId,
        string confirmationTokenReference,
        string? idempotencyKey,
        CancellationToken cancellationToken);

    Task RefundAsync(
        Guid bookingId,
        decimal amount,
        string reason,
        CancellationToken cancellationToken);

    Task StartCompletionHoldAsync(
        Guid bookingId,
        CancellationToken cancellationToken);

    Task ReleaseAsync(Guid bookingId, CancellationToken cancellationToken);

    Task SettleDisputeAsync(
        Guid bookingId,
        decimal clientRefundAmount,
        string reason,
        CancellationToken cancellationToken);

    Task ReconcileProviderObjectAsync(
        string providerObjectId,
        CancellationToken cancellationToken);
}
