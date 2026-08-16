namespace SmartCourt.Features.Users.Lawyers.Dashboard.DTOs;

public sealed record LawyerDashboardStatsDto(
    int ActiveContractsCount,
    int PendingProposalsCount,
    int RecommendedCasesCount,
    int UpcomingConsultationsCount,
    int PendingConsultationRequestsCount,
    decimal AvailableBalance,
    decimal PendingBalance,
    decimal LifetimeEarnings,
    string Currency,
    decimal AverageRating,
    int TotalReviewsCount,
    int UnreadNotificationsCount,
    int ActiveDisputesCount
);
