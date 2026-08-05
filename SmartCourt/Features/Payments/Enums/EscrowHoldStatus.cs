namespace SmartCourt.Features.Payments.Enums;

public enum EscrowHoldStatus : int
{
    Funded = 0,
    Frozen = 1,
    Released = 2,
    Refunded = 3
}
