namespace SmartCourt.Common.Payments;

public static class PlatformFeePolicy
{
    public const decimal Rate = 0.15m;

    public static decimal Calculate(decimal lawyerGrossAllocation)
        => decimal.Round(
            lawyerGrossAllocation * Rate,
            2,
            MidpointRounding.AwayFromZero);
}
