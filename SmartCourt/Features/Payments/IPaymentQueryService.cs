using SmartCourt.Features.Payments.DTOs;

namespace SmartCourt.Features.Payments;

public interface IPaymentQueryService
{
    Task<PaymentHistoryDto> GetContractPaymentsAsync(
        Guid contractId,
        CancellationToken cancellationToken);

    Task<PaymentDto> GetMilestonePaymentAsync(
        Guid milestoneId,
        CancellationToken cancellationToken);
}
