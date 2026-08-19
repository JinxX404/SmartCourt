using System.Collections.Generic;

namespace SmartCourt.Features.Admin.LawyerSubscriptions.DTOs;

public sealed record AdminLawyerSubscriptionListDto(
    List<AdminLawyerSubscriptionSummaryDto> Items,
    int TotalCount,
    int Page,
    int PageSize
);
