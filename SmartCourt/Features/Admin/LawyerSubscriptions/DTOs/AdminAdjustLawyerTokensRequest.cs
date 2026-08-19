namespace SmartCourt.Features.Admin.LawyerSubscriptions.DTOs;

public sealed record AdminAdjustLawyerTokensRequest(
    decimal CreditAmount, // Positive to add, negative to subtract
    string Reason
);
