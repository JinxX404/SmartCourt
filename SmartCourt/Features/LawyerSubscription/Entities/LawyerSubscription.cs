using System;
using SmartCourt.Features.LawyerSubscription.Enums;

namespace SmartCourt.Features.LawyerSubscription.Entities;

public sealed class LawyerSubscription
{
    public Guid LawyerId { get; set; }
    public LawyerPlanType PlanType { get; set; }
    public int DailyTokenLimit { get; set; }
    public DateTimeOffset StartedAt { get; set; }
    public DateTimeOffset? ExpiresAt { get; set; }
    public bool IsActive { get; set; }
    public byte[] RowVersion { get; set; } = [];
}
