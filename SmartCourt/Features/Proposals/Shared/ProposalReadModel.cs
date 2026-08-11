using Microsoft.EntityFrameworkCore;
using SmartCourt.Features.Proposals.DTOs;
using SmartCourt.Features.Proposals.Enums;
using SmartCourt.Persistence;

namespace SmartCourt.Features.Proposals.Shared;

internal static class ProposalReadModel
{
    public static async Task<ProposalDetailDto?> FindDetailAsync(
        ApplicationDbContext context,
        Guid proposalId,
        Guid actorUserId,
        CancellationToken cancellationToken)
    {
        var row = await (
            from proposal in context.Proposals.AsNoTracking()
            join legalCase in context.Cases
                on proposal.LegalCaseId equals legalCase.Id
            join client in context.Users
                on proposal.ClientUserId equals client.Id
            join lawyer in context.Users
                on proposal.LawyerUserId equals lawyer.Id
            join conversation in context.ChatConversations
                on proposal.Id equals conversation.ProposalId into conversationJoin
            from conversation in conversationJoin.DefaultIfEmpty()
            join contract in context.Contracts
                on proposal.Id equals contract.ProposalId into contractJoin
            from contract in contractJoin.DefaultIfEmpty()
            where proposal.Id == proposalId
                && (proposal.ClientUserId == actorUserId
                    || proposal.LawyerUserId == actorUserId)
            select new
            {
                proposal.Id,
                proposal.LegalCaseId,
                CaseTitle = legalCase.Title,
                proposal.ClientUserId,
                ClientName = client.FullName,
                proposal.LawyerUserId,
                LawyerName = lawyer.FullName,
                proposal.Message,
                proposal.Status,
                proposal.DecisionReason,
                CaseStatus = legalCase.Status,
                AssignedLawyerUserId = legalCase.LawyerId,
                ContractId = contract == null
                    ? null
                    : (Guid?)contract.Id,
                ContractStatus = contract == null
                    ? null
                    : (SmartCourt.Features.Contracts.Enums.ContractStatus?)contract.Status,
                proposal.CreatedAt,
                proposal.RespondedAt,
                proposal.ExpiresAt,
                proposal.ClosedAt,
                proposal.ClosedByUserId,
                proposal.UpdatedAt,
                ConversationId = conversation == null
                    ? null
                    : (Guid?)conversation.Id,
                ConversationIsClosed = conversation != null
                    && conversation.IsClosed
            })
            .SingleOrDefaultAsync(cancellationToken);

        if (row is null)
        {
            return null;
        }

        var canChat = row.Status == ProposalStatus.Accepted
            && row.ConversationId.HasValue
            && !row.ConversationIsClosed;
        return new ProposalDetailDto(
                row.Id,
                row.LegalCaseId,
                row.CaseTitle,
                row.ClientUserId,
                row.ClientName,
                row.LawyerUserId,
                row.LawyerName,
                row.Message,
                row.Status.ToString(),
                row.DecisionReason,
                row.CaseStatus.ToString(),
                row.AssignedLawyerUserId,
                row.AssignedLawyerUserId == row.LawyerUserId,
                row.ContractId,
                row.ContractStatus?.ToString(),
                row.ConversationId,
                row.ConversationId.HasValue
                    ? row.ConversationIsClosed ? "Closed" : "Open"
                    : null,
                canChat,
                ProposalPermittedActions.Resolve(
                    actorUserId,
                    row.ClientUserId,
                    row.LawyerUserId,
                    row.Status,
                    row.ContractId,
                    row.ConversationId,
                    row.ConversationIsClosed),
                row.CreatedAt,
                row.RespondedAt,
                row.UpdatedAt,
                row.ExpiresAt,
                row.ClosedAt,
                row.ClosedByUserId);
    }
}
