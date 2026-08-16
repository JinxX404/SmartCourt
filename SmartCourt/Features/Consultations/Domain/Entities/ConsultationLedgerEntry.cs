using SmartCourt.Features.Payments.Enums;

namespace SmartCourt.Features.Consultations.Domain.Entities;

public sealed class ConsultationLedgerEntry
{
    public Guid Id { get; set; }
    public Guid BookingId { get; set; }
    public Guid? PaymentTransactionId { get; set; }
    public LedgerTransactionType TransactionType { get; set; }
    public decimal Amount { get; set; }
    public decimal RunningBalance { get; set; }
    public string Currency { get; set; } = "EGP";
    public string Description { get; set; } = string.Empty;
    public Guid CorrelationId { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}
