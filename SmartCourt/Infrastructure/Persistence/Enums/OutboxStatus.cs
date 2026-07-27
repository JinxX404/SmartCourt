namespace SmartCourt.Infrastructure.Persistence.Enums;

public enum OutboxStatus : int
{
    Pending = 0,
    Processing = 1,
    Processed = 2,
    Failed = 3
}
