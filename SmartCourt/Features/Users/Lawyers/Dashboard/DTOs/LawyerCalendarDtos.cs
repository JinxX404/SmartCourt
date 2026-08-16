namespace SmartCourt.Features.Users.Lawyers.Dashboard.DTOs;

public enum CalendarEventType
{
    Consultation = 1,
    MilestoneDeadline = 2
}

public sealed record LawyerCalendarEventDto(
    Guid Id,
    CalendarEventType EventType,
    string Title,
    string Description,
    DateTimeOffset StartUtc,
    DateTimeOffset? EndUtc,
    string Status,
    Guid? ReferenceId,
    string? ReferenceType,
    Guid? ClientId,
    string? ClientName,
    string? MeetingUrlOrLocation
);

public sealed record LawyerCalendarScheduleDto(
    DateTimeOffset FromUtc,
    DateTimeOffset ToUtc,
    IReadOnlyList<LawyerCalendarEventDto> Events
);

public sealed record LawyerCalendarQuery(
    DateTimeOffset? FromUtc = null,
    DateTimeOffset? ToUtc = null
);
