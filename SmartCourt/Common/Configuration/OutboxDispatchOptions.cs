namespace SmartCourt.Common.Configuration;

public sealed class OutboxDispatchOptions
{
    public const string SectionName = "OutboxDispatch";

    public bool Enabled { get; set; } = true;
    public int BatchSize { get; set; } = 100;
    public int IdleDelayMilliseconds { get; set; } = 1_000;
    public int ErrorDelayMilliseconds { get; set; } = 5_000;
}
