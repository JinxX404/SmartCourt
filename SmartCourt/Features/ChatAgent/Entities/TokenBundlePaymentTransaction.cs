using SmartCourt.Features.Payments.Enums;

namespace SmartCourt.Features.ChatAgent.Entities;

public sealed class TokenBundlePaymentTransaction
{
    public Guid Id { get; set; }
    public Guid ClientId { get; set; }
    public string BundleId { get; set; } = string.Empty;
    public int TokenAmount { get; set; }
    public decimal PriceEgp { get; set; }
    public PaymentOperationType OperationType { get; set; }
    public PaymentTransactionStatus Status { get; set; }
    public string ProviderName { get; set; } = string.Empty;
    public string IdempotencyKey { get; set; } = string.Empty;
    public string? ProviderTransactionId { get; set; }
    public string? RelatedProviderTransactionId { get; set; }
    public string? ProviderStatus { get; set; }
    public string? FailureReason { get; set; }
    public DateTimeOffset? ProcessedAtUtc { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public byte[] RowVersion { get; set; } = [];
}
