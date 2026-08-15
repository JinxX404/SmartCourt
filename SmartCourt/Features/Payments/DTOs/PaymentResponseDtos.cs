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
    DateTimeOffset? HoldExpiresAt,
    DateTimeOffset? SettledAt);

public sealed record PaymentAttemptDto(
    Guid Id,
    Guid? MilestoneId,
    PaymentOperationType OperationType,
    PaymentTransactionStatus Status,
    decimal Amount,
    string Currency,
    string ProviderName,
    int ProviderAttemptCount,
    DateTimeOffset? NextRetryAt,
    bool RequiresManualAction,
    DateTimeOffset? ManualActionRequiredAt,
    DateTimeOffset CreatedAt,
    DateTimeOffset? ProcessedAt);

public sealed record EscrowLedgerEntryDto(
    Guid Id,
    Guid? EscrowHoldId,
    LedgerTransactionType TransactionType,
    decimal Amount,
    decimal RunningBalance,
    string Currency,
    string Description,
    DateTimeOffset CreatedAt);

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

public sealed record WithdrawalDto(
    Guid Id,
    decimal Amount,
    string Currency,
    WithdrawalStatus Status,
    string? ProviderStatus,
    string? FailureReason,
    bool RequiresManualAction,
    DateTimeOffset RequestedAt,
    DateTimeOffset? ProcessedAt);

public sealed record PaymentActionResultDto(
    Guid EntityId,
    string Status,
    DateTimeOffset OccurredAt);

public sealed record FundingOperationDto(
    Guid PaymentTransactionId,
    Guid MilestoneId,
    string Status,
    string? ClientActionType,
    string? ClientSecret,
    string? RedirectUrl,
    PaymentDto? Payment,
    DateTimeOffset OccurredAt);

public sealed record LawyerPayoutAccountDto(
    Guid Id,
    string ProviderCode,
    string Status,
    bool DetailsSubmitted,
    bool TransfersEnabled,
    bool PayoutsEnabled,
    string Country,
    string DefaultCurrency,
    string? MaskedDestination,
    DateTimeOffset? LastSynchronizedAt);

public sealed record PayoutAccountLinkDto(
    string Url,
    DateTimeOffset? ExpiresAt);

public sealed record PaymentProviderConfigDto(
    string ProviderCode,
    string PublishableKey,
    string Currency,
    bool SandboxOnly,
    bool ConfirmationTokensEnabled,
    bool SavedPaymentMethodsEnabled);

public sealed record SetupPaymentMethodSessionDto(
    string SetupIntentId,
    string ClientSecret,
    string Status);

public sealed record SavedPaymentMethodDto(
    string PaymentMethodReference,
    string Type,
    string? Brand,
    string? Last4,
    long? ExpiryMonth,
    long? ExpiryYear,
    string? HolderName,
    bool IsDefault);
