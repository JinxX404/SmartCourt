namespace SmartCourt.Features.Payments.Enums;

public enum PaymentTransactionStatus : int
{
    // Unknown provider outcomes remain Processing until reconciliation.
    Processing = 0,
    Completed = 1,
    Failed = 2
}
