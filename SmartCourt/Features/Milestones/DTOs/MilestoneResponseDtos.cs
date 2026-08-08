using SmartCourt.Features.Milestones.Enums;

namespace SmartCourt.Features.Milestones.DTOs;

public sealed record MilestoneDto(
    Guid Id,
    int OrderNumber,
    string Title,
    string? Description,
    decimal Amount,
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
    string Version)
{
    public IReadOnlyList<string> PermittedActions { get; init; } = [];
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
