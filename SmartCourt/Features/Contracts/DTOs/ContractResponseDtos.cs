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
    DateTime? ActivatedAt,
    DateTime? CompletedAt);

public sealed record ContractMilestoneDto(
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
    decimal? NetLawyerAmount);

public sealed record ContractPaymentDto(
    Guid Id,
    Guid MilestoneId,
    decimal GrossAmount,
    decimal PlatformFee,
    decimal NetAmount,
    string Currency,
    EscrowHoldStatus Status,
    DateTime? HoldExpiresAt,
    DateTime? SettledAt);

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
    DateTime? AcceptedByClientAt,
    DateTime? AcceptedByLawyerAt,
    DateTime? ActivatedAt,
    DateTime? CompletedAt,
    DateTime? TerminatedAt,
    decimal CurrentMilestoneTotal,
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
    DateTime CreatedAt);

public sealed record ContractActionResultDto(
    Guid EntityId,
    string Status,
    DateTime OccurredAt);

public sealed record ContractSettlementSummaryDto(
    Guid ContractId,
    string Currency,
    decimal GrossAmount,
    decimal ClientRefundAmount,
    decimal LawyerReleaseAmount,
    decimal PlatformFeeAmount);
