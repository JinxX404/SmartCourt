namespace SmartCourt.Common.Exceptions;

public class InsufficientQuotaException : BusinessException
{
    public decimal DailyLimitCredits { get; }
    public decimal ConsumedCredits { get; }
    public decimal RemainingCredits { get; }
    public decimal RequestedCredits { get; }
    public DateTimeOffset NextResetAt { get; }

    public InsufficientQuotaException(
        int dailyLimitTokens,
        int consumedTokens,
        int requestedTokens,
        DateTimeOffset nextResetAt,
        string message = "لقد استنفدت رصيد الاستخدام المتاح لك.") 
        : base(message)
    {
        DailyLimitCredits = SmartCourt.Common.Domain.CreditConverter.ToCredits(dailyLimitTokens);
        ConsumedCredits = SmartCourt.Common.Domain.CreditConverter.ToCredits(consumedTokens);
        RemainingCredits = SmartCourt.Common.Domain.CreditConverter.ToCredits(Math.Max(0, dailyLimitTokens - consumedTokens));
        RequestedCredits = SmartCourt.Common.Domain.CreditConverter.ToCredits(requestedTokens);
        NextResetAt = nextResetAt;
    }
}
