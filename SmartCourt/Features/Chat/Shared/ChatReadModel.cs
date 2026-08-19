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
            join contract in context.Contracts
                on conversation.ProposalId equals contract.ProposalId into contractJoin
            from contract in contractJoin.DefaultIfEmpty()
            where conversation.Id == conversationId
                && (conversation.ClientUserId == actorUserId
                    || conversation.LawyerUserId == actorUserId)
                && !ChatAccess.IsHiddenFromLawyer(
                    proposal.Status,
                    contract == null
                        ? null
                        : (SmartCourt.Features.Contracts.Enums.ContractStatus?)contract.Status,
                    conversation.LawyerUserId,
                    actorUserId)
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
                ProposalStatus = proposal.Status,
                ContractStatus = contract == null
                    ? null
                    : (SmartCourt.Features.Contracts.Enums.ContractStatus?)contract.Status,
                conversation.CreatedAt,
                conversation.UpdatedAt,
                conversation.LastMessageAt
            })
            .SingleOrDefaultAsync(cancellationToken);

        var canWrite = row is not null
            && !row.IsClosed
            && row.ProposalStatus == ProposalStatus.Accepted
            && row.ContractStatus is not SmartCourt.Features.Contracts.Enums.ContractStatus.Completed
                and not SmartCourt.Features.Contracts.Enums.ContractStatus.Terminated;
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
                row.LastMessageAt)
            {
                CanSendMessages = canWrite,
                CanUploadAttachments = canWrite
            };
    }

    public static async Task<ChatMessageDto?> FindMessageAsync(
        ApplicationDbContext context,
        Guid messageId,
        Guid actorUserId,
        CancellationToken cancellationToken)
    {
        var row = await (
            from message in context.ChatMessages.AsNoTracking()
            join conversation in context.ChatConversations.AsNoTracking()
                on message.ConversationId equals conversation.Id
            join proposal in context.Proposals.AsNoTracking()
                on conversation.ProposalId equals proposal.Id
            join contract in context.Contracts.AsNoTracking()
                on conversation.ProposalId equals contract.ProposalId into contractJoin
            from contract in contractJoin.DefaultIfEmpty()
            join sender in context.Users.AsNoTracking()
                on message.SenderUserId equals sender.Id into senderJoin
            from sender in senderJoin.DefaultIfEmpty()
            where message.Id == messageId
                && (conversation.ClientUserId == actorUserId
                    || conversation.LawyerUserId == actorUserId)
                && !ChatAccess.IsHiddenFromLawyer(
                    proposal.Status,
                    contract == null
                        ? null
                        : (SmartCourt.Features.Contracts.Enums.ContractStatus?)contract.Status,
                    conversation.LawyerUserId,
                    actorUserId)
            select new
            {
                message.Id,
                message.ConversationId,
                message.SenderUserId,
                SenderName = sender == null ? null : sender.FullName,
                message.Type,
                message.Content,
                message.SystemCode,
                message.RelatedEntityId,
                message.CreatedAt
            })
            .SingleOrDefaultAsync(cancellationToken);

        if (row is null)
        {
            return null;
        }

        var attachmentsByMessageId = await ChatAttachmentReadModel
            .FindForMessagesAsync(
                context,
                [row.Id],
                cancellationToken);
        attachmentsByMessageId.TryGetValue(row.Id, out var attachments);
        return new ChatMessageDto(
            row.Id,
            row.ConversationId,
            row.SenderUserId,
            row.SenderName,
            row.Type.ToString(),
            row.Content,
            row.SystemCode,
            row.RelatedEntityId,
            row.CreatedAt,
            row.SenderUserId == actorUserId,
            attachments ?? []);
    }
}
