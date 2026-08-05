using SmartCourt.Common.Domain;

namespace SmartCourt.Features.Payments.Entities;

public sealed class LawyerWallet
{
    private LawyerWallet()
    {
    }

    internal LawyerWallet(
        Guid id,
        Guid lawyerUserId,
        DateTime createdAt)
    {
        Id = EntityGuard.NotEmpty(id, nameof(id));
        LawyerUserId = EntityGuard.NotEmpty(
            lawyerUserId,
            nameof(lawyerUserId));
        Currency = EntityGuard.CurrencyEgp;
        CreatedAt = EntityGuard.Utc(createdAt, nameof(createdAt));
        UpdatedAt = CreatedAt;
    }

    public Guid Id { get; internal set; }
    public Guid LawyerUserId { get; internal set; }
    public string Currency { get; internal set; } = EntityGuard.CurrencyEgp;
    public decimal PendingBalance { get; internal set; }
    public decimal AvailableBalance { get; internal set; }
    public byte[] RowVersion { get; internal set; } = [];
    public DateTime CreatedAt { get; internal set; }
    public DateTime UpdatedAt { get; internal set; }
}
