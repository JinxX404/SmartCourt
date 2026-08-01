using SmartCourt.Features.Contracts.DTOs;
using SmartCourt.Features.Disputes.Enums;

namespace SmartCourt.Features.Disputes.DTOs;

public sealed record DisputeEvidenceDto(
    Guid Id,
    Guid UploadedByUserId,
    Guid? StoredFileId,
    string? Content,
    DateTime CreatedAt);

public sealed record DisputePermittedActionsDto(
    bool CanAddEvidence,
    bool CanAssign,
    bool CanStartReview,
    bool CanResolve,
    bool CanClose);

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
    DateTime? ResolvedAt,
    DateTime? ClosedAt,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    IReadOnlyList<DisputeEvidenceDto> Evidence,
    DisputeSettlementDto? Settlement,
    DisputePermittedActionsDto PermittedActions);

public sealed record DisputeActionResultDto(
    Guid DisputeId,
    string Status,
    DateTime OccurredAt);

public sealed record DisputeListResult(PagedResult<DisputeDto> Page);
