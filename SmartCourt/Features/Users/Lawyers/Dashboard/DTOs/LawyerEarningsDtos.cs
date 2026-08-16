namespace SmartCourt.Features.Users.Lawyers.Dashboard.DTOs;

public sealed record LawyerEarningsQuery(
    string Period = "6months",
    string GroupBy = "monthly"
);

public sealed record EarningsPeriodPointDto(
    string PeriodLabel,
    DateTimeOffset PeriodStartUtc,
    DateTimeOffset PeriodEndUtc,
    decimal ContractAmount,
    decimal ConsultationAmount,
    decimal TotalAmount
);

public sealed record RecentPayoutDto(
    Guid Id,
    decimal Amount,
    string Currency,
    string Status,
    DateTimeOffset RequestedAtUtc,
    DateTimeOffset? ProcessedAtUtc
);

public sealed record LawyerEarningsSummaryDto(
    decimal TotalEarnedInPeriod,
    decimal ContractEarningsInPeriod,
    decimal ConsultationEarningsInPeriod,
    decimal PlatformFeesPaidInPeriod,
    decimal CurrentAvailableBalance,
    decimal CurrentPendingBalance,
    string Currency,
    IReadOnlyList<EarningsPeriodPointDto> PeriodBreakdown,
    IReadOnlyList<RecentPayoutDto> RecentWithdrawals
);
