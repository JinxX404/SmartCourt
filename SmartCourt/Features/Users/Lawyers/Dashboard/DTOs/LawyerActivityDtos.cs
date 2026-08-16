namespace SmartCourt.Features.Users.Lawyers.Dashboard.DTOs;

public enum LawyerActivityType
{
    ContractStateChanged = 1,
    MilestoneStateChanged = 2,
    ProposalStateChanged = 3,
    ConsultationBooked = 4,
    ConsultationCompleted = 5,
    RatingReceived = 6,
    DisputeRaised = 7
}

public sealed record LawyerActivityItemDto(
    Guid Id,
    LawyerActivityType ActivityType,
    string Title,
    string Description,
    Guid? ReferenceId,
    string? ReferenceType,
    DateTimeOffset OccurredAtUtc
);

public sealed record LawyerActivityQuery(int Page = 1, int PageSize = 15);
