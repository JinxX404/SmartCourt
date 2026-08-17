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

public sealed record ReassignDisputeRequest(
    Guid ModeratorUserId,
    string? Reason = null);

public sealed record WithdrawDisputeRequest(string Reason);

public sealed record ResolveDisputeRequest(
    DisputeResolutionType ResolutionType,
    decimal ClientRefundAmount,
    decimal LawyerReleaseAmount,
    string Summary,
    PenaltyType? PenaltyType = null,
    string? PenaltyReason = null);

public sealed record RevokeLawyerPenaltyRequest(string Reason);

public sealed record DisputeListQuery(
    Guid? ContractId = null,
    Guid? MilestoneId = null,
    DisputeStatus? Status = null,
    DisputeCategory? Category = null,
    Guid? RaisedByUserId = null,
    Guid? AssignedModeratorUserId = null,
    DateTimeOffset? FromDate = null,
    DateTimeOffset? ToDate = null,
    int Page = 1,
    int PageSize = 20);

public sealed record LawyerPenaltyFilterQuery(
    Guid? LawyerUserId = null,
    PenaltyType? PenaltyType = null,
    bool? IsActiveOnly = null,
    int Page = 1,
    int PageSize = 20);

