namespace SmartCourt.Features.Proposals.DTOs;

public sealed record ProposalListItemDto(
    Guid Id,
    Guid LegalCaseId,
    string CaseTitle,
    Guid ClientUserId,
    string ClientName,
    Guid LawyerUserId,
    string LawyerName,
    string Status,
    string CaseStatus,
    Guid? AssignedLawyerUserId,
    bool IsAssignedLawyer,
    Guid? ContractId,
    string? ContractStatus,
    Guid? ConversationId,
    string? ConversationStatus,
    bool CanChat,
    IReadOnlyList<string> PermittedActions,
    DateTime CreatedAt,
    DateTime? RespondedAt,
    DateTime? ExpiresAt = null,
    DateTime? ClosedAt = null,
    Guid? ClosedByUserId = null);

public sealed record ProposalDetailDto(
    Guid Id,
    Guid LegalCaseId,
    string CaseTitle,
    Guid ClientUserId,
    string ClientName,
    Guid LawyerUserId,
    string LawyerName,
    string Message,
    string Status,
    string? DecisionReason,
    string CaseStatus,
    Guid? AssignedLawyerUserId,
    bool IsAssignedLawyer,
    Guid? ContractId,
    string? ContractStatus,
    Guid? ConversationId,
    string? ConversationStatus,
    bool CanChat,
    IReadOnlyList<string> PermittedActions,
    DateTime CreatedAt,
    DateTime? RespondedAt,
    DateTime UpdatedAt,
    DateTime? ExpiresAt = null,
    DateTime? ClosedAt = null,
    Guid? ClosedByUserId = null);

public sealed record ProposalPageDto(
    IReadOnlyList<ProposalListItemDto> Items,
    int Page,
    int PageSize,
    int TotalCount,
    bool HasNextPage);

public sealed record ProposalSlotAvailabilityDto(
    Guid LegalCaseId,
    int ActiveProposalCount,
    int ProposalLimit,
    int AvailableProposalSlots,
    bool CanSendProposal);
