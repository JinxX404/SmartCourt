using SmartCourt.Features.Payments.Enums;

namespace SmartCourt.Features.Consultations.Domain.Entities;

public sealed class ConsultationEscrowHold
{
    public Guid Id { get; set; }
    public Guid BookingId { get; set; }
    public Guid DepositTransactionId { get; set; }
    public decimal GrossAmount { get; set; }
    public decimal PlatformFeeAmount { get; set; }
    public decimal NetAmount { get; set; }
    public string Currency { get; set; } = "EGP";
    public EscrowHoldStatus Status { get; set; }
    public DateTimeOffset FundedAtUtc { get; set; }
    public DateTimeOffset? HoldStartsAtUtc { get; set; }
    public DateTimeOffset? HoldExpiresAtUtc { get; set; }
    public DateTimeOffset? FrozenAtUtc { get; set; }
    public DateTimeOffset? SettledAtUtc { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public byte[] RowVersion { get; set; } = [];
}
