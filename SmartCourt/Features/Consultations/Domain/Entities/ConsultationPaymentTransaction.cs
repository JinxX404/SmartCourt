using SmartCourt.Features.Payments.Enums;

namespace SmartCourt.Features.Consultations.Domain.Entities;

public sealed class ConsultationPaymentTransaction
{
    public Guid Id { get; set; }
    public Guid BookingId { get; set; }
    public PaymentOperationType OperationType { get; set; }
    public PaymentTransactionStatus Status { get; set; }
    public string ProviderName { get; set; } = string.Empty;
    public string IdempotencyKey { get; set; } = string.Empty;
    public string? ProviderTransactionId { get; set; }
    public string? RelatedProviderTransactionId { get; set; }
    public string? ProviderStatus { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "EGP";
    public string? FailureReason { get; set; }
    public bool RequiresManualAction { get; set; }
    public DateTime? ProcessedAtUtc { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public byte[] RowVersion { get; set; } = [];
}
