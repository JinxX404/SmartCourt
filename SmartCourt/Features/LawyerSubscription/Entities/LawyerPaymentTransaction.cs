using System;
using SmartCourt.Features.Payments.Enums;

namespace SmartCourt.Features.LawyerSubscription.Entities;

public sealed class LawyerPaymentTransaction
{
    public Guid Id { get; set; }
    public Guid LawyerId { get; set; }
    
    // Distinguish what they are buying
    public string TargetId { get; set; } = string.Empty; // e.g. 'lawyer_bundle_1m' or 'Professional'
    public string TargetType { get; set; } = string.Empty; // e.g. 'Bundle' or 'Subscription'

    public decimal PriceEgp { get; set; }
    public PaymentOperationType OperationType { get; set; }
    public PaymentTransactionStatus Status { get; set; }

    public string ProviderName { get; set; } = string.Empty;
    public string? ProviderTransactionId { get; set; }
    public string? RelatedProviderTransactionId { get; set; }
    public string? ProviderStatus { get; set; }
    public string? FailureReason { get; set; }
    public string? IdempotencyKey { get; set; }
    public DateTimeOffset? ProcessedAtUtc { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}
