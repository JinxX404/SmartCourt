using System;

namespace SmartCourt.Features.LawyerSubscription.DTOs;

public sealed record LawyerSubscriptionInfoDto(
    string PlanType,
    decimal DailyCreditLimit,
    DateTimeOffset StartedAt,
    DateTimeOffset? ExpiresAt,
    bool IsActive
);
