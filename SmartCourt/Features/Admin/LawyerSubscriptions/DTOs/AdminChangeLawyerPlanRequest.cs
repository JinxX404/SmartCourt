namespace SmartCourt.Features.Admin.LawyerSubscriptions.DTOs;

public sealed record AdminChangeLawyerPlanRequest(
    string PlanType, // Free, Professional, Business
    string Reason
);
