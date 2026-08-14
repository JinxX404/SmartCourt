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
    DateTime? DueDate,
    MilestoneStatus Status,
    MilestoneFundingStatus FundingStatus,
    Guid? EscrowHoldId,
    DateTime? FundedAt,
    DateTime? SubmittedAt,
    DateTime? AutoAcceptEligibleAt,
    DateTime? HoldExpiresAt,
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
        DateTime? dueDate,
        MilestoneStatus status,
        MilestoneFundingStatus fundingStatus,
        Guid? escrowHoldId,
        DateTime? fundedAt,
        DateTime? submittedAt,
        DateTime? autoAcceptEligibleAt,
        DateTime? holdExpiresAt,
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
    DateTime? ProposedDueDate,
    string Reason,
    ChangeRequestStatus Status,
    Guid? DecidedByUserId,
    DateTime? DecidedAt,
    DateTime CreatedAt)
{
    public string? DecisionReason { get; init; }
}

public sealed record MilestoneActionResultDto(
    Guid EntityId,
    string Status,
    DateTime OccurredAt);
