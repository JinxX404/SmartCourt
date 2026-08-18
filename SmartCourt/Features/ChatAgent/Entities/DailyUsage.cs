using SmartCourt.Common.Domain;

namespace SmartCourt.Features.ChatAgent.Entities;

public sealed class DailyUsage
{
    private DailyUsage()
    {
    }

    internal DailyUsage(Guid clientId, DateTimeOffset usageDate)
    {
        ClientId = EntityGuard.NotEmpty(clientId, nameof(clientId));
        UsageDate = usageDate;
        ConsumedTokens = 0;
    }

    public Guid ClientId { get; internal set; }
    public DateTimeOffset UsageDate { get; internal set; }
    public int ConsumedTokens { get; internal set; }
    public byte[] RowVersion { get; internal set; } = [];

    public static DailyUsage Create(Guid clientId, DateTimeOffset usageDate)
    {
        return new DailyUsage(clientId, usageDate);
    }

    public void IncrementConsumption()
    {
        ConsumedTokens++;
    }
}
