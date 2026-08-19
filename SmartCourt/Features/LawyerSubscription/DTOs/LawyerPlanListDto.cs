using System.Collections.Generic;

namespace SmartCourt.Features.LawyerSubscription.DTOs;

public sealed record LawyerPlanDto(
    string PlanType,
    decimal DailyCreditLimit,
    decimal MonthlyPriceEgp
);

public sealed record LawyerPlanListDto(
    List<LawyerPlanDto> Plans
);
