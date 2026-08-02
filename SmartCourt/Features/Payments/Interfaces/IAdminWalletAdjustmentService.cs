using SmartCourt.Features.Payments.DTOs;

namespace SmartCourt.Features.Payments;

public interface IAdminWalletAdjustmentService
{
    Task<AdminWalletAdjustmentDto> AdjustAsync(
        Guid lawyerUserId,
        AdminWalletAdjustmentRequest request,
        string? idempotencyKey,
        CancellationToken cancellationToken);
}
