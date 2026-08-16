using SmartCourt.Common.Payments;

namespace SmartCourt.Features.Consultations.Shared;

public static class ConsultationPolicy
{
    public const int DefaultPageSize = 5;
    public const int MaximumPageSize = 50;
    public const int PaymentReservationMinutes = 10;
    public const int ClientReviewHours = 24;
    public const int ReleaseHoldDays = 14;
    public static (decimal Fee, decimal Net) CalculateSettlement(decimal gross)
    {
        var fee = PlatformFeePolicy.Calculate(gross);
        return (fee, gross - fee);
    }
}
