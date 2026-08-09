using SmartCourt.Features.Payments.Enums;

namespace SmartCourt.Features.Payments.Integration;

public interface IPaymentNotificationContextReader
{
    Task<WithdrawalNotificationContext> GetWithdrawalAsync(
        Guid withdrawalId,
        CancellationToken cancellationToken);

    Task<WalletAdjustmentNotificationContext> GetWalletAdjustmentAsync(
        Guid walletAdjustmentId,
        CancellationToken cancellationToken);
}

public sealed record WithdrawalNotificationContext(
    Guid WithdrawalId,
    Guid LawyerUserId,
    WithdrawalStatus Status,
    bool RequiresManualAction);

public sealed record WalletAdjustmentNotificationContext(
    Guid WalletAdjustmentId,
    Guid LawyerUserId,
    Guid ContractId);
