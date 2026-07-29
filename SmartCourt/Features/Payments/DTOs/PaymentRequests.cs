using SmartCourt.Features.Payments.Enums;

namespace SmartCourt.Features.Payments.DTOs;

public sealed record FundMilestoneRequest(
    string PaymentMethodReference);

public sealed record PaymentWebhookRequest(
    string EventId,
    Guid PaymentTransactionId,
    string ProviderTransactionId,
    PaymentTransactionStatus Status,
    decimal Amount,
    string Currency,
    DateTime? ProcessedAt,
    string? FailureReason);

public sealed record CreateWithdrawalRequest(
    decimal Amount,
    string DestinationReference);
