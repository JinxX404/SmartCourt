using Microsoft.EntityFrameworkCore;
using SmartCourt.Features.Chat.DTOs;
using SmartCourt.Features.Proposals.Enums;
using SmartCourt.Persistence;

namespace SmartCourt.Features.Chat.Shared;

internal static class ChatReadModel
{
    public static async Task<ChatConversationDetailDto?> FindConversationAsync(
        ApplicationDbContext context,
        Guid conversationId,
        Guid actorUserId,
        CancellationToken cancellationToken)
    {
        var row = await (
            from conversation in context.ChatConversations.AsNoTracking()
            join legalCase in context.Cases
                on conversation.LegalCaseId equals legalCase.Id
            join client in context.Users
                on conversation.ClientUserId equals client.Id
            join lawyer in context.Users
                on conversation.LawyerUserId equals lawyer.Id
            join proposal in context.Proposals
                on conversation.ProposalId equals proposal.Id
            where conversation.Id == conversationId
                && (conversation.ClientUserId == actorUserId
                    || (conversation.LawyerUserId == actorUserId
                        && proposal.Status != ProposalStatus.Superseded))
            select new
            {
                conversation.Id,
                conversation.ProposalId,
                conversation.LegalCaseId,
                CaseTitle = legalCase.Title,
                conversation.ClientUserId,
                ClientName = client.FullName,
                conversation.LawyerUserId,
                LawyerName = lawyer.FullName,
                conversation.IsClosed,
                conversation.CreatedAt,
                conversation.UpdatedAt,
                conversation.LastMessageAt
            })
            .SingleOrDefaultAsync(cancellationToken);

        return row is null
            ? null
            : new ChatConversationDetailDto(
                row.Id,
                row.ProposalId,
                row.LegalCaseId,
                row.CaseTitle,
                new ChatParticipantDto(
                    row.ClientUserId,
                    row.ClientName,
                    "Client"),
                new ChatParticipantDto(
                    row.LawyerUserId,
                    row.LawyerName,
                    "Lawyer"),
                row.IsClosed ? "Closed" : "Open",
                row.CreatedAt,
                row.UpdatedAt,
                row.LastMessageAt);
    }

    public static async Task<ChatMessageDto?> FindMessageAsync(
        ApplicationDbContext context,
        Guid messageId,
        Guid actorUserId,
        CancellationToken cancellationToken)
    {
        return await (
            from message in context.ChatMessages.AsNoTracking()
            join conversation in context.ChatConversations.AsNoTracking()
                on message.ConversationId equals conversation.Id
            join proposal in context.Proposals.AsNoTracking()
                on conversation.ProposalId equals proposal.Id
            join sender in context.Users.AsNoTracking()
                on message.SenderUserId equals sender.Id into senderJoin
            from sender in senderJoin.DefaultIfEmpty()
            where message.Id == messageId
                && (conversation.ClientUserId == actorUserId
                    || (conversation.LawyerUserId == actorUserId
                        && proposal.Status != ProposalStatus.Superseded))
            select new ChatMessageDto(
                message.Id,
                message.ConversationId,
                message.SenderUserId,
                sender == null ? null : sender.FullName,
                message.Type.ToString(),
                message.Content,
                message.SystemCode,
                message.RelatedEntityId,
                message.CreatedAt,
                message.SenderUserId == actorUserId))
            .SingleOrDefaultAsync(cancellationToken);
    }
}
