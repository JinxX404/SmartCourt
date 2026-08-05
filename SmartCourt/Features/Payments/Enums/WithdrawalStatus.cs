namespace SmartCourt.Features.Payments.Enums;

public enum WithdrawalStatus : int
{
    // Unknown provider outcomes remain Processing with funds reserved.
    Processing = 0,
    Completed = 1,
    Failed = 2
}
