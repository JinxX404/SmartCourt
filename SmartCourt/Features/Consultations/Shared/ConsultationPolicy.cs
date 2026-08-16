namespace SmartCourt.Features.Consultations.Shared;

public static class ConsultationPolicy
{
    public const int DefaultPageSize = 5;
    public const int MaximumPageSize = 50;
    public const int PaymentReservationMinutes = 10;
    public const int ClientReviewHours = 24;
    public const int ReleaseHoldDays = 14;
    public const decimal PlatformFeeRate = 0.05m;

    public static (decimal Fee, decimal Net) CalculateSettlement(decimal gross)
    {
        var fee = decimal.Round(
            gross * PlatformFeeRate,
            2,
            MidpointRounding.AwayFromZero);
        return (fee, gross - fee);
    }
}
