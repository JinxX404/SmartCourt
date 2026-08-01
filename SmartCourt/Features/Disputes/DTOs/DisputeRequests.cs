using SmartCourt.Features.Disputes.Enums;

namespace SmartCourt.Features.Disputes.DTOs;

public sealed record CreateDisputeRequest(
    Guid MilestoneId,
    DisputeCategory Category,
    string Title,
    string Description,
    DisputeRequestedOutcome RequestedOutcome,
    IReadOnlyList<Guid> StoredFileIds);

public sealed record AddDisputeEvidenceRequest(
    string? Content,
    IReadOnlyList<Guid> StoredFileIds);

public sealed record AssignDisputeRequest(Guid ModeratorUserId);

public sealed record ResolveDisputeRequest(
    DisputeResolutionType ResolutionType,
    decimal ClientRefundAmount,
    decimal LawyerReleaseAmount,
    string Summary,
    PenaltyType? PenaltyType = null,
    string? PenaltyReason = null);

public sealed record DisputeListQuery(
    DisputeStatus? Status = null,
    Guid? AssignedModeratorUserId = null,
    int Page = 1,
    int PageSize = 20);
