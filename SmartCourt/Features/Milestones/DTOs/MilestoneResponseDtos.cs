using System.Text.Json.Serialization;
using SmartCourt.Features.Milestones.Enums;

namespace SmartCourt.Features.Milestones.DTOs;

[method: JsonConstructor]
public sealed record MilestoneDto(
    Guid Id,
    int OrderNumber,
    string Title,
    string? Description,
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    IReadOnlyList<string>? Deliverables,
    decimal Amount,
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    int? DurationDays,
    DateTimeOffset? DueDate,
    MilestoneStatus Status,
    MilestoneFundingStatus FundingStatus,
    Guid? EscrowHoldId,
    DateTimeOffset? FundedAt,
    DateTimeOffset? SubmittedAt,
    DateTimeOffset? AutoAcceptEligibleAt,
    DateTimeOffset? HoldExpiresAt,
    decimal? NetLawyerAmount,
    string Version,
    MilestoneType Type = MilestoneType.Standard)
{
    public IReadOnlyList<string> PermittedActions { get; init; } = [];

    public MilestoneDto(
        Guid id,
        int orderNumber,
        string title,
        string? description,
        decimal amount,
        int? durationDays,
        DateTimeOffset? dueDate,
        MilestoneStatus status,
        MilestoneFundingStatus fundingStatus,
        Guid? escrowHoldId,
        DateTimeOffset? fundedAt,
        DateTimeOffset? submittedAt,
        DateTimeOffset? autoAcceptEligibleAt,
        DateTimeOffset? holdExpiresAt,
        decimal? netLawyerAmount,
        string version)
        : this(
            id,
            orderNumber,
            title,
            description,
            null,
            amount,
            durationDays,
            dueDate,
            status,
            fundingStatus,
            escrowHoldId,
            fundedAt,
            submittedAt,
            autoAcceptEligibleAt,
            holdExpiresAt,
            netLawyerAmount,
            version)
    {
    }
}

public sealed record MilestoneChangeRequestDto(
    Guid Id,
    Guid MilestoneId,
    Guid RequestedByUserId,
    string? ProposedDescription,
    int? ProposedDurationDays,
    DateTimeOffset? ProposedDueDate,
    string Reason,
    ChangeRequestStatus Status,
    Guid? DecidedByUserId,
    DateTimeOffset? DecidedAt,
    DateTimeOffset CreatedAt)
{
    public string? DecisionReason { get; init; }
}

public sealed record MilestoneActionResultDto(
    Guid EntityId,
    string Status,
    DateTimeOffset OccurredAt);
