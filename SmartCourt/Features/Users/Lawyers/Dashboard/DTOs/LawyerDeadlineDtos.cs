namespace SmartCourt.Features.Users.Lawyers.Dashboard.DTOs;

public enum DeadlineUrgency
{
    Overdue = 1,
    Critical = 2,
    Approaching = 3,
    Normal = 4
}

public sealed record UpcomingDeadlineItemDto(
    Guid ContractId,
    string ContractTitle,
    Guid MilestoneId,
    string MilestoneTitle,
    int MilestoneOrder,
    decimal Amount,
    string Currency,
    string MilestoneStatus,
    DateTimeOffset? DueDateUtc,
    int? DaysRemaining,
    DeadlineUrgency Urgency,
    Guid ClientId,
    string ClientName
);

public sealed record LawyerDeadlinesQuery(int DaysAhead = 30);
