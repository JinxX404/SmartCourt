using SmartCourt.Features.Payments.DTOs;

namespace SmartCourt.Features.Payments;

public interface IClientPaymentMethodService
{
    Task<SetupPaymentMethodSessionDto> CreateSetupSessionAsync(
        string? idempotencyKey,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<SavedPaymentMethodDto>> ListAsync(
        CancellationToken cancellationToken);

    Task SetDefaultAsync(
        string paymentMethodReference,
        CancellationToken cancellationToken);

    Task RemoveAsync(
        string paymentMethodReference,
        CancellationToken cancellationToken);
}
