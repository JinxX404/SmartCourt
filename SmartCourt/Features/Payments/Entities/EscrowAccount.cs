using SmartCourt.Common.Domain;
using SmartCourt.Features.Payments.Enums;

namespace SmartCourt.Features.Payments.Entities;

public sealed class EscrowAccount
{
    private EscrowAccount()
    {
    }

    internal EscrowAccount(
        Guid id,
        Guid contractId,
        DateTimeOffset createdAt)
    {
        Id = EntityGuard.NotEmpty(id, nameof(id));
        ContractId = EntityGuard.NotEmpty(contractId, nameof(contractId));
        Currency = EntityGuard.CurrencyEgp;
        Status = EscrowAccountStatus.Active;
        CreatedAt = EntityGuard.Utc(createdAt, nameof(createdAt));
        UpdatedAt = CreatedAt;
    }

    public Guid Id { get; internal set; }
    public Guid ContractId { get; internal set; }
    public string Currency { get; internal set; } = EntityGuard.CurrencyEgp;
    public decimal TotalDeposited { get; internal set; }
    public decimal TotalReleased { get; internal set; }
    public decimal TotalRefunded { get; internal set; }
    public decimal TotalFees { get; internal set; }
    public EscrowAccountStatus Status { get; internal set; }
    public byte[] RowVersion { get; internal set; } = [];
    public DateTimeOffset CreatedAt { get; internal set; }
    public DateTimeOffset UpdatedAt { get; internal set; }
}
