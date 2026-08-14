using System.Text.Json.Serialization;
using SmartCourt.Features.Payments.Enums;

namespace SmartCourt.Features.Payments.DTOs;

public sealed record FundMilestoneRequest(
    string PaymentMethodReference,
    [property: JsonIgnore] string ConfirmationTokenReference = "",
    [property: JsonIgnore] string CustomerReference = "");

public sealed record CreateMilestonePaymentSessionRequest(
    string ConfirmationTokenReference);

public sealed record RetryPaymentRequest(
    string PaymentMethodReference,
    string IdempotencyKey = "");

public sealed record RetryPaymentSessionRequest(
    string ConfirmationTokenReference,
    string IdempotencyKey = "");

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
    string DestinationReference = "");

public sealed record LinkLawyerPayoutAccountRequest(
    Guid LawyerUserId,
    string ProviderAccountId);
