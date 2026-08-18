using SmartCourt.Common.Domain;

namespace SmartCourt.Features.ChatAgent.Entities;

public sealed class QuotaProfile
{
    public static readonly Guid GlobalProfileId = new("99999999-9999-9999-9999-999999999999");

    private QuotaProfile()
    {
    }

    internal QuotaProfile(Guid clientId, int dailyTokenLimit)
    {
        ClientId = EntityGuard.NotEmpty(clientId, nameof(clientId));
        DailyTokenLimit = EntityGuard.NonNegative(dailyTokenLimit, nameof(dailyTokenLimit));
    }

    public Guid ClientId { get; internal set; }
    public int DailyTokenLimit { get; internal set; }

    public static QuotaProfile Create(Guid clientId, int dailyTokenLimit)
    {
        return new QuotaProfile(clientId, dailyTokenLimit);
    }

    public void UpdateLimit(int newLimit)
    {
        DailyTokenLimit = EntityGuard.NonNegative(newLimit, nameof(newLimit));
    }
}
