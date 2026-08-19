using System;

namespace SmartCourt.Features.LawyerSubscription.Entities;

public sealed class LawyerDailyUsage
{
    public Guid LawyerId { get; set; }
    public DateTimeOffset UsageDate { get; set; }
    public int ConsumedTokens { get; set; }
    public byte[] RowVersion { get; set; } = [];
}
