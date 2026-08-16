using SmartCourt.Common.Domain;
using SmartCourt.Features.Payments.Enums;

namespace SmartCourt.Features.Payments.Entities;

public sealed class WithdrawalRequest
{
    private WithdrawalRequest()
    {
    }

    internal WithdrawalRequest(
        Guid id,
        Guid lawyerUserId,
        decimal amount,
        string idempotencyKey,
        DateTimeOffset requestedAt)
    {
        Id = EntityGuard.NotEmpty(id, nameof(id));
        LawyerUserId = EntityGuard.NotEmpty(
            lawyerUserId,
            nameof(lawyerUserId));
        Amount = EntityGuard.PositiveMoney(amount, nameof(amount));
        Currency = EntityGuard.CurrencyEgp;
        Status = WithdrawalStatus.Processing;
        IdempotencyKey = EntityGuard.Required(
            idempotencyKey,
            nameof(idempotencyKey));
        RequestedAt = EntityGuard.Utc(requestedAt, nameof(requestedAt));
    }

    public Guid Id { get; internal set; }
    public Guid LawyerUserId { get; internal set; }
    public decimal Amount { get; internal set; }
    public string Currency { get; internal set; } = EntityGuard.CurrencyEgp;
    public WithdrawalStatus Status { get; internal set; }
    public string? ProviderTransactionId { get; internal set; }
    public Guid? LawyerPayoutAccountId { get; internal set; }
    public string? ProviderAccountId { get; internal set; }
    public string? ProviderStatus { get; internal set; }
    public long? ProviderAmountMinor { get; internal set; }
    public string? ProviderCurrency { get; internal set; }
    public string? FailureReason { get; internal set; }
    public bool RequiresManualAction { get; internal set; }
    public DateTimeOffset? ManualActionRequiredAt { get; internal set; }
    public DateTimeOffset RequestedAt { get; internal set; }
    public DateTimeOffset? ProcessedAt { get; internal set; }
    public string IdempotencyKey { get; internal set; } = string.Empty;
    public byte[] RowVersion { get; internal set; } = [];
}
