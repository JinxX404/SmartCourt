using System;

namespace SmartCourt.Features.LawyerSubscription.DTOs;

public sealed record LawyerQuotaInfoResponse(
    decimal DailyCreditLimit,
    decimal ConsumedDailyCredits,
    decimal RemainingDailyCredits,
    decimal AvailableAdditionalCredits,
    decimal TotalRemainingCredits,
    string PlanName,
    DateTimeOffset NextResetAt
);
