using SmartCourt.Features.Payments.Enums;

namespace SmartCourt.Features.Payments.DTOs;

public sealed record PaymentDto(
    Guid Id,
    Guid MilestoneId,
    decimal GrossAmount,
    decimal PlatformFee,
    decimal NetAmount,
    string Currency,
    EscrowHoldStatus Status,
    DateTime? HoldExpiresAt,
    DateTime? SettledAt);

public sealed record PaymentAttemptDto(
    Guid Id,
    Guid? MilestoneId,
    PaymentOperationType OperationType,
    PaymentTransactionStatus Status,
    decimal Amount,
    string Currency,
    string ProviderName,
    int ProviderAttemptCount,
    DateTime? NextRetryAt,
    bool RequiresManualAction,
    DateTime CreatedAt,
    DateTime? ProcessedAt);

public sealed record EscrowLedgerEntryDto(
    Guid Id,
    Guid? EscrowHoldId,
    LedgerTransactionType TransactionType,
    decimal Amount,
    decimal RunningBalance,
    string Currency,
    string Description,
    DateTime CreatedAt);

public sealed record PaymentHistoryDto(
    IReadOnlyList<PaymentDto> Payments,
    IReadOnlyList<PaymentAttemptDto> Attempts,
    IReadOnlyList<EscrowLedgerEntryDto> LedgerEntries);

public sealed record WalletDto(
    Guid LawyerUserId,
    string Currency,
    decimal PendingBalance,
    decimal AvailableBalance,
    decimal TotalReleased);

public sealed record PaymentActionResultDto(
    Guid EntityId,
    string Status,
    DateTime OccurredAt);
