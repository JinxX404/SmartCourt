using SmartCourt.Common.Domain;

namespace SmartCourt.Features.Payments.Entities;

public sealed class WalletAdjustment
{
    private WalletAdjustment()
    {
    }

    internal WalletAdjustment(
        Guid id,
        Guid lawyerWalletId,
        Guid contractId,
        Guid escrowAccountId,
        Guid ledgerEntryId,
        decimal pendingBalanceDelta,
        decimal availableBalanceDelta,
        decimal pendingBalanceBefore,
        decimal pendingBalanceAfter,
        decimal availableBalanceBefore,
        decimal availableBalanceAfter,
        string reason,
        Guid createdByUserId,
        Guid correlationId,
        DateTime createdAt)
    {
        Id = EntityGuard.NotEmpty(id, nameof(id));
        LawyerWalletId = EntityGuard.NotEmpty(
            lawyerWalletId,
            nameof(lawyerWalletId));
        ContractId = EntityGuard.NotEmpty(contractId, nameof(contractId));
        EscrowAccountId = EntityGuard.NotEmpty(
            escrowAccountId,
            nameof(escrowAccountId));
        LedgerEntryId = EntityGuard.NotEmpty(
            ledgerEntryId,
            nameof(ledgerEntryId));
        PendingBalanceDelta = pendingBalanceDelta;
        AvailableBalanceDelta = availableBalanceDelta;
        PendingBalanceBefore = EntityGuard.NonNegativeMoney(
            pendingBalanceBefore,
            nameof(pendingBalanceBefore));
        PendingBalanceAfter = EntityGuard.NonNegativeMoney(
            pendingBalanceAfter,
            nameof(pendingBalanceAfter));
        AvailableBalanceBefore = EntityGuard.NonNegativeMoney(
            availableBalanceBefore,
            nameof(availableBalanceBefore));
        AvailableBalanceAfter = EntityGuard.NonNegativeMoney(
            availableBalanceAfter,
            nameof(availableBalanceAfter));
        Reason = EntityGuard.Required(reason, nameof(reason));
        CreatedByUserId = EntityGuard.NotEmpty(
            createdByUserId,
            nameof(createdByUserId));
        CorrelationId = EntityGuard.NotEmpty(
            correlationId,
            nameof(correlationId));
        CreatedAt = EntityGuard.Utc(createdAt, nameof(createdAt));
    }

    public Guid Id { get; private set; }
    public Guid LawyerWalletId { get; private set; }
    public Guid ContractId { get; private set; }
    public Guid EscrowAccountId { get; private set; }
    public Guid LedgerEntryId { get; private set; }
    public decimal PendingBalanceDelta { get; private set; }
    public decimal AvailableBalanceDelta { get; private set; }
    public decimal PendingBalanceBefore { get; private set; }
    public decimal PendingBalanceAfter { get; private set; }
    public decimal AvailableBalanceBefore { get; private set; }
    public decimal AvailableBalanceAfter { get; private set; }
    public string Reason { get; private set; } = string.Empty;
    public Guid CreatedByUserId { get; private set; }
    public Guid CorrelationId { get; private set; }
    public DateTime CreatedAt { get; private set; }
}
