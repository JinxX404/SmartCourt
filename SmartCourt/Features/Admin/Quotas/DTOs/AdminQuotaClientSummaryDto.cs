using System;

namespace SmartCourt.Features.Admin.Quotas.DTOs;

public sealed record AdminQuotaClientSummaryDto(
    Guid ClientId,
    string FullName,
    string Email,
    decimal DailyCreditLimit,
    decimal ConsumedDailyCredits,
    decimal AvailableAdditionalCredits,
    decimal TotalRemainingCredits
);

public sealed record AdminQuotaClientSummaryListDto(
    System.Collections.Generic.List<AdminQuotaClientSummaryDto> Clients,
    int TotalCount
);
