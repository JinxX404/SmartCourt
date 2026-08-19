using System.Collections.Generic;

namespace SmartCourt.Features.LawyerSubscription.DTOs;

public sealed record LawyerQuotaHistoryResponse(
    List<LawyerDailyQuotaUsageDto> Usages
);

public sealed record LawyerDailyQuotaUsageDto(
    string UsageDate,
    decimal ConsumedCredits
);
