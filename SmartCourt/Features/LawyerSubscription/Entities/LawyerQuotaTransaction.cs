using System;

namespace SmartCourt.Features.LawyerSubscription.Entities;

public sealed class LawyerQuotaTransaction
{
    public Guid Id { get; set; }
    public Guid LawyerId { get; set; }
    public int Amount { get; set; }
    public string Reason { get; set; } = string.Empty;
    public string? ReferenceId { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}
