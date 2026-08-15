using System.Text.Json.Serialization;
using SmartCourt.Features.Milestones.Enums;

namespace SmartCourt.Features.Milestones.DTOs;

[method: JsonConstructor]
public sealed record AddMilestoneRequest(
    string Title,
    string? Description,
    IReadOnlyList<string>? Deliverables,
    int OrderNumber,
    decimal Amount,
    int? DurationDays,
    DateTimeOffset? DueDate,
    MilestoneType Type = MilestoneType.Standard)
{
    public AddMilestoneRequest(
        string title,
        string? description,
        int orderNumber,
        decimal amount,
        int? durationDays,
        DateTimeOffset? dueDate)
        : this(
            title,
            description,
            null,
            orderNumber,
            amount,
            durationDays,
            dueDate)
    {
    }
}

[method: JsonConstructor]
public sealed record UpdateMilestoneRequest(
    string Title,
    string? Description,
    IReadOnlyList<string>? Deliverables,
    int? DurationDays,
    DateTimeOffset? DueDate,
    MilestoneType? Type = null)
{
    public UpdateMilestoneRequest(
        string title,
        string? description,
        int? durationDays,
        DateTimeOffset? dueDate)
        : this(
            title,
            description,
            null,
            durationDays,
            dueDate)
    {
    }
}

public sealed record SubmitMilestoneRequest(
    string Notes,
    IReadOnlyList<Guid> StoredFileIds);

public sealed record RequestMilestoneChangesRequest(string Reason);

public sealed record ExpenseMilestoneDecisionRequest(string Reason);

public sealed record CreateMilestoneChangeRequest(
    string? ProposedDescription,
    int? ProposedDurationDays,
    DateTimeOffset? ProposedDueDate,
    string Reason);

public sealed record RejectChangeRequest(string Reason);
