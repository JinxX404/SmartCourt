namespace SmartCourt.Infrastructure.Persistence.Enums;

public enum IdempotencyStatus : int
{
    Processing = 0,
    Completed = 1,
    Failed = 2
}
