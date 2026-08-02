namespace SmartCourt.Features.Payments.DTOs;

public sealed record AdminWalletAdjustmentRequest(
    Guid ContractId,
    decimal PendingBalanceDelta,
    decimal AvailableBalanceDelta,
    string Reason);

public sealed record AdminWalletAdjustmentDto(
    Guid Id,
    Guid LawyerUserId,
    Guid ContractId,
    Guid LedgerEntryId,
    decimal PendingBalanceDelta,
    decimal AvailableBalanceDelta,
    decimal PendingBalance,
    decimal AvailableBalance,
    Guid CreatedByUserId,
    DateTime CreatedAt);
