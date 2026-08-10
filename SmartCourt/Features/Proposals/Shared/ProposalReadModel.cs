using Microsoft.EntityFrameworkCore;
using SmartCourt.Features.Proposals.DTOs;
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
                proposal.CreatedAt,
                proposal.RespondedAt,
                proposal.ExpiresAt,
                proposal.ClosedAt,
                proposal.ClosedByUserId,
                proposal.UpdatedAt,
                ConversationId = conversation == null
                    ? null
                    : (Guid?)conversation.Id
            })
            .SingleOrDefaultAsync(cancellationToken);

        return row is null
            ? null
            : new ProposalDetailDto(
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
                row.CreatedAt,
                row.RespondedAt,
                row.UpdatedAt,
                row.ConversationId,
                row.ExpiresAt,
                row.ClosedAt,
                row.ClosedByUserId);
    }
}
