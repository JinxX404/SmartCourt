using SmartCourt.Common.Domain;
using SmartCourt.Common.Exceptions;
using SmartCourt.Features.Payments.Enums;

namespace SmartCourt.Features.Payments.Entities;

public sealed class PaymentTransaction
{
    private PaymentTransaction()
    {
    }

    internal PaymentTransaction(
        Guid id,
        Guid contractId,
        Guid? milestoneId,
        PaymentOperationType operationType,
        string providerName,
        string idempotencyKey,
        decimal amount,
        DateTime createdAt)
    {
        Id = EntityGuard.NotEmpty(id, nameof(id));
        ContractId = EntityGuard.NotEmpty(contractId, nameof(contractId));
        MilestoneId = EntityGuard.OptionalGuid(milestoneId, nameof(milestoneId));
        if (operationType != PaymentOperationType.Withdrawal
            && !MilestoneId.HasValue)
        {
            throw new BusinessException(
                "تتطلب عملية دفع المرحلة تحديد معرّف مرحلة صالح.");
        }

        OperationType = operationType;
        ProviderName = EntityGuard.Required(providerName, nameof(providerName));
        IdempotencyKey = EntityGuard.Required(
            idempotencyKey,
            nameof(idempotencyKey));
        Amount = EntityGuard.PositiveMoney(amount, nameof(amount));
        Currency = EntityGuard.CurrencyEgp;
        Status = PaymentTransactionStatus.Processing;
        CreatedAt = EntityGuard.Utc(createdAt, nameof(createdAt));
        UpdatedAt = CreatedAt;
    }

    public Guid Id { get; internal set; }
    public Guid ContractId { get; internal set; }
    public Guid? MilestoneId { get; internal set; }
    public Guid? EscrowHoldId { get; internal set; }
    public PaymentOperationType OperationType { get; internal set; }
    public string ProviderName { get; internal set; } = string.Empty;
    public string? ProviderTransactionId { get; internal set; }
    public string IdempotencyKey { get; internal set; } = string.Empty;
    public decimal Amount { get; internal set; }
    public string Currency { get; internal set; } = EntityGuard.CurrencyEgp;
    public PaymentTransactionStatus Status { get; internal set; }
    public string? FailureReason { get; internal set; }
    public int ProviderAttemptCount { get; internal set; }
    public DateTime? NextRetryAt { get; internal set; }
    public bool RequiresManualAction { get; internal set; }
    public DateTime? ManualActionRequiredAt { get; internal set; }
    public DateTime? ProcessedAt { get; internal set; }
    public byte[] RowVersion { get; internal set; } = [];
    public DateTime CreatedAt { get; internal set; }
    public DateTime UpdatedAt { get; internal set; }
}
