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
    public DateTime FundedAtUtc { get; set; }
    public DateTime? HoldStartsAtUtc { get; set; }
    public DateTime? HoldExpiresAtUtc { get; set; }
    public DateTime? FrozenAtUtc { get; set; }
    public DateTime? SettledAtUtc { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public byte[] RowVersion { get; set; } = [];
}
