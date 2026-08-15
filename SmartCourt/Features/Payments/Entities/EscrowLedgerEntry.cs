using SmartCourt.Common.Domain;
using SmartCourt.Features.Payments.Enums;

namespace SmartCourt.Features.Payments.Entities;

public sealed class EscrowLedgerEntry
{
    private EscrowLedgerEntry()
    {
    }

    internal EscrowLedgerEntry(
        Guid id,
        Guid escrowAccountId,
        Guid? escrowHoldId,
        LedgerTransactionType transactionType,
        decimal amount,
        decimal runningBalance,
        string referenceType,
        Guid referenceId,
        Guid? paymentTransactionId,
        string description,
        Guid? createdByUserId,
        Guid correlationId,
        DateTimeOffset createdAt)
    {
        Id = EntityGuard.NotEmpty(id, nameof(id));
        EscrowAccountId = EntityGuard.NotEmpty(
            escrowAccountId,
            nameof(escrowAccountId));
        EscrowHoldId = EntityGuard.OptionalGuid(
            escrowHoldId,
            nameof(escrowHoldId));
        TransactionType = transactionType;
        Amount = EntityGuard.PositiveMoney(amount, nameof(amount));
        RunningBalance = EntityGuard.NonNegativeMoney(
            runningBalance,
            nameof(runningBalance));
        Currency = EntityGuard.CurrencyEgp;
        ReferenceType = EntityGuard.Required(referenceType, nameof(referenceType));
        ReferenceId = EntityGuard.NotEmpty(referenceId, nameof(referenceId));
        PaymentTransactionId = EntityGuard.OptionalGuid(
            paymentTransactionId,
            nameof(paymentTransactionId));
        Description = EntityGuard.Required(description, nameof(description));
        CreatedByUserId = EntityGuard.OptionalGuid(
            createdByUserId,
            nameof(createdByUserId));
        CorrelationId = EntityGuard.NotEmpty(correlationId, nameof(correlationId));
        CreatedAt = EntityGuard.Utc(createdAt, nameof(createdAt));
    }

    public Guid Id { get; private set; }
    public Guid EscrowAccountId { get; private set; }
    public Guid? EscrowHoldId { get; private set; }
    public LedgerTransactionType TransactionType { get; private set; }
    public decimal Amount { get; private set; }
    public decimal RunningBalance { get; private set; }
    public string Currency { get; private set; } = EntityGuard.CurrencyEgp;
    public string ReferenceType { get; private set; } = string.Empty;
    public Guid ReferenceId { get; private set; }
    public Guid? PaymentTransactionId { get; private set; }
    public string Description { get; private set; } = string.Empty;
    public Guid? CreatedByUserId { get; private set; }
    public Guid CorrelationId { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
}
