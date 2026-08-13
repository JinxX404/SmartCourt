namespace SmartCourt.Features.Milestones.DTOs;

public sealed record AddMilestoneRequest(
    string Title,
    string? Description,
    IReadOnlyList<string>? Deliverables,
    int OrderNumber,
    decimal Amount,
    int? DurationDays,
    DateTime? DueDate);

public sealed record UpdateMilestoneRequest(
    string Title,
    string? Description,
    IReadOnlyList<string>? Deliverables,
    int? DurationDays,
    DateTime? DueDate);

public sealed record SubmitMilestoneRequest(
    string Notes,
    IReadOnlyList<Guid> StoredFileIds);

public sealed record RequestMilestoneChangesRequest(string Reason);

public sealed record CreateMilestoneChangeRequest(
    string? ProposedDescription,
    int? ProposedDurationDays,
    DateTime? ProposedDueDate,
    string Reason);

public sealed record RejectChangeRequest(string Reason);
