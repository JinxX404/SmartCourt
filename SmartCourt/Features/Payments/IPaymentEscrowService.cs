using SmartCourt.Features.Payments.DTOs;

namespace SmartCourt.Features.Payments;

public interface IPaymentEscrowService
{
    Task<PaymentDto> FundAsync(
        Guid milestoneId,
        FundMilestoneRequest request,
        string? idempotencyKey,
        CancellationToken cancellationToken);
}
