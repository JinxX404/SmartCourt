using System;
using System.Collections.Generic;

namespace SmartCourt.Features.ChatAgent.DTOs;

public sealed record QuotaHistoryResponse(
    List<DailyQuotaUsageDto> Usages
);

public sealed record DailyQuotaUsageDto(
    string UsageDate,
    decimal ConsumedCredits
);
