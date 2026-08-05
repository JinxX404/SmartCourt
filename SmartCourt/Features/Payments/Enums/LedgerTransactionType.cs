namespace SmartCourt.Features.Payments.Enums;

public enum LedgerTransactionType : int
{
    Deposit = 0,
    Release = 1,
    Refund = 2,
    PlatformFee = 3,
    Adjustment = 4
}
