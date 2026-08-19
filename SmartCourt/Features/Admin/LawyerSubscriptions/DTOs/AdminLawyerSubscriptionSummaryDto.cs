using System;

namespace SmartCourt.Features.Admin.LawyerSubscriptions.DTOs;

public sealed record AdminLawyerSubscriptionSummaryDto(
    Guid LawyerId,
    string FirstName,
    string LastName,
    string Email,
    string PlanName,
    decimal DailyCreditLimit,
    decimal PurchasedCreditBalance,
    DateTimeOffset CreatedAt
);
