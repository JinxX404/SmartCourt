using System;

namespace SmartCourt.Features.Case.GetUserCases.DTOs;

public sealed record GetUserCaseSummaryDto(
    Guid Id,
    string Title,
    string Status,
    string? Governorate,
    string? City,
    DateTime? SubmittedAt,
    DateTime CreatedAt,
    int DocumentCount);
