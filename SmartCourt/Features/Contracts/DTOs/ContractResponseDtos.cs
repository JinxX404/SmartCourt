using System.Text.Json.Serialization;
using SmartCourt.Features.Contracts.Enums;
using SmartCourt.Features.Milestones.Enums;
using SmartCourt.Features.Payments.Enums;

namespace SmartCourt.Features.Contracts.DTOs;

public sealed record ContractSummaryDto(
    Guid Id,
    Guid LegalCaseId,
    Guid ClientUserId,
    Guid LawyerUserId,
    string Title,
    string Currency,
    ContractStatus Status,
    DateTimeOffset? ActivatedAt,
    DateTimeOffset? CompletedAt);

public sealed record ContractMilestoneDto(
    Guid Id,
    int OrderNumber,
    string Title,
    string? Description,
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
    MilestoneType Type = MilestoneType.Standard,
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    IReadOnlyList<string>? Deliverables = null);

public sealed record ContractPaymentDto(
    Guid Id,
    Guid MilestoneId,
    decimal GrossAmount,
    decimal PlatformFee,
    decimal NetAmount,
    string Currency,
    EscrowHoldStatus Status,
    DateTimeOffset? HoldExpiresAt,
    DateTimeOffset? SettledAt);

public sealed record ContractDetailDto(
    Guid Id,
    Guid ProposalId,
    Guid LegalCaseId,
    Guid ClientUserId,
    Guid LawyerUserId,
    string Title,
    string TermsAndConditions,
    string Currency,
    ContractStatus Status,
    DateTimeOffset? AcceptedByClientAt,
    DateTimeOffset? AcceptedByLawyerAt,
    DateTimeOffset? ActivatedAt,
    DateTimeOffset? CompletedAt,
    DateTimeOffset? TerminatedAt,
    decimal CurrentMilestoneTotal,
    string Version,
    IReadOnlyList<ContractMilestoneDto> Milestones,
    IReadOnlyList<ContractPaymentDto> Payments,
    IReadOnlyList<string> PermittedActions);

public sealed record ContractStateHistoryDto(
    Guid Id,
    ContractStatus? PreviousStatus,
    ContractStatus NewStatus,
    string Trigger,
    Guid? ActorUserId,
    string? Reason,
    DateTimeOffset CreatedAt);

public sealed record ContractActionResultDto(
    Guid EntityId,
    string Status,
    DateTimeOffset OccurredAt);

public sealed record ContractSettlementSummaryDto(
    Guid ContractId,
    string Currency,
    decimal GrossAmount,
    decimal ClientRefundAmount,
    decimal LawyerReleaseAmount,
    decimal PlatformFeeAmount);
