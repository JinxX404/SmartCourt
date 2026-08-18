namespace SmartCourt.Features.ChatAgent.DTOs;

public sealed record QuotaInfoResponse(
    decimal DailyCreditLimit,
    decimal ConsumedDailyCredits,
    decimal RemainingDailyCredits,
    decimal AvailableAdditionalCredits,
    decimal TotalRemainingCredits,
    DateTimeOffset NextResetAt
);
