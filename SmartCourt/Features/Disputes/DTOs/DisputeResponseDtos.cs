using SmartCourt.Common.Models;
using SmartCourt.Features.Contracts.DTOs;
using SmartCourt.Features.Disputes.Enums;

namespace SmartCourt.Features.Disputes.DTOs;

public sealed record DisputeEvidenceDto(
    Guid Id,
    Guid UploadedByUserId,
    Guid? StoredFileId,
    string? Content,
    DateTimeOffset CreatedAt);

public sealed record EvidenceDownloadUrlDto(
    Guid EvidenceId,
    Guid StoredFileId,
    string DownloadUrl,
    DateTimeOffset ExpiresAt);

public sealed record DisputePermittedActionsDto(
    bool CanAddEvidence,
    bool CanAssign,
    bool CanReassign,
    bool CanStartReview,
    bool CanResolve,
    bool CanClose,
    bool CanWithdraw);

public sealed record DisputeSettlementDto(
    string Status,
    decimal GrossHoldAmount,
    decimal ClientRefundAmount,
    decimal LawyerReleaseAmount,
    decimal PlatformFeeAmount);

public sealed record DisputeDto(
    Guid Id,
    Guid ContractId,
    Guid MilestoneId,
    Guid RaisedByUserId,
    Guid? AssignedModeratorUserId,
    DisputeCategory Category,
    string Title,
    string Description,
    DisputeStatus Status,
    DisputeRequestedOutcome RequestedOutcome,
    DisputeResolutionType? ResolutionType,
    string? ResolutionSummary,
    DateTimeOffset? ResolvedAt,
    DateTimeOffset? ClosedAt,
    DateTimeOffset? CancelledAt,
    string? CancellationReason,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    IReadOnlyList<DisputeEvidenceDto> Evidence,
    DisputeSettlementDto? Settlement,
    DisputePermittedActionsDto PermittedActions);

public sealed record DisputeActionResultDto(
    Guid DisputeId,
    string Status,
    DateTimeOffset OccurredAt);

public sealed record DisputeStatsDto(
    int TotalOpen,
    int TotalAssigned,
    int TotalUnderReview,
    int TotalResolved,
    int TotalClosed,
    int TotalCancelled,
    int UnassignedCount);

public sealed record LawyerPenaltyDto(
    Guid Id,
    Guid LawyerUserId,
    string LawyerName,
    Guid DisputeId,
    PenaltyType PenaltyType,
    string Reason,
    bool IsActive,
    DateTimeOffset StartsAt,
    DateTimeOffset? EndsAt,
    bool IsRevoked,
    DateTimeOffset? RevokedAt,
    Guid? RevokedByUserId,
    string? RevocationReason,
    DateTimeOffset CreatedAt);

public sealed record DisputeListResult(PagedResult<DisputeDto> Page);

