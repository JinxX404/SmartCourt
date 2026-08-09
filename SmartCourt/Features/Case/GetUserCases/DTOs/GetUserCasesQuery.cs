using SmartCourt.Common.Enums;

namespace SmartCourt.Features.Case.GetUserCases.DTOs;

public sealed record GetUserCasesQuery(
    CaseStatus? Status = null,
    string? SearchTerm = null,
    int Page = 1,
    int PageSize = 10);
