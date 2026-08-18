namespace SmartCourt.Common.Configuration;

public sealed class QuotaOptions
{
    public const string SectionName = "Quota";

    public int DailyFreeTokens { get; set; } = 100000;
    public string Timezone { get; set; } = "Egypt Standard Time";
}
