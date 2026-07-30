using SmartCourt.Features.Proposals.Entities;

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
    DateTime CreatedAt,
    DateTime? RespondedAt);

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
    DateTime CreatedAt,
    DateTime? RespondedAt,
    DateTime UpdatedAt,
    Guid? ConversationId = null);

public sealed record ProposalPageDto(
    IReadOnlyList<ProposalListItemDto> Items,
    int Page,
    int PageSize,
    int TotalCount,
    bool HasNextPage);

internal static class ProposalMappings
{
    public static ProposalDetailDto ToDetailDto(
        Proposal proposal,
        string caseTitle,
        string clientName,
        string lawyerName,
        Guid? conversationId = null)
    {
        return new ProposalDetailDto(
            proposal.Id,
            proposal.LegalCaseId,
            caseTitle,
            proposal.ClientUserId,
            clientName,
            proposal.LawyerUserId,
            lawyerName,
            proposal.Message,
            proposal.Status.ToString(),
            proposal.DecisionReason,
            proposal.CreatedAt,
            proposal.RespondedAt,
            proposal.UpdatedAt,
            conversationId);
    }
}
