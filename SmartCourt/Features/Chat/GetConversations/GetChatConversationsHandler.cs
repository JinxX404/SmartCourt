using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartCourt.Common.Models;
using SmartCourt.Features.Chat.DTOs;
using SmartCourt.Features.Chat.Shared;
using SmartCourt.Features.Proposals.Enums;
using SmartCourt.Interfaces;
using SmartCourt.Persistence;

namespace SmartCourt.Features.Chat.GetConversations;

public sealed class GetChatConversationsHandler(
    ApplicationDbContext context,
    ICurrentUserService currentUserService,
    IValidator<GetChatConversationsQuery> validator)
    : IRequestHandler<GetChatConversationsQuery, ApiResponse<ChatConversationPageDto>>
{
    public async Task<ApiResponse<ChatConversationPageDto>> Handle(
        GetChatConversationsQuery request,
        CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(
            request,
            cancellationToken);
        if (!validationResult.IsValid)
        {
            return ApiResponse<ChatConversationPageDto>.Fail(
                validationResult.Errors.Select(error => error.ErrorMessage).ToList());
        }

        var actorUserId = ChatAccess.GetRequiredUserId(currentUserService);
        var query =
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
            where conversation.ClientUserId == actorUserId
                || (conversation.LawyerUserId == actorUserId
                    && proposal.Status != ProposalStatus.Superseded
                    && proposal.Status != ProposalStatus.Terminated
                    && (contract == null
                        || (contract.Status != SmartCourt.Features.Contracts.Enums.ContractStatus.Completed
                            && contract.Status != SmartCourt.Features.Contracts.Enums.ContractStatus.Terminated)))
            select new
            {
                conversation,
                legalCase,
                client,
                lawyer,
                proposal,
                contract
            };

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim();
            query = query.Where(item =>
                item.legalCase.Title.Contains(search)
                || item.client.FullName.Contains(search)
                || item.lawyer.FullName.Contains(search));
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var rows = await query
            .OrderByDescending(item =>
                item.conversation.LastMessageAt
                    ?? item.conversation.UpdatedAt)
            .ThenBy(item => item.conversation.Id)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(item => new
            {
                item.conversation.Id,
                item.conversation.ProposalId,
                item.conversation.LegalCaseId,
                CaseTitle = item.legalCase.Title,
                item.conversation.ClientUserId,
                ClientName = item.client.FullName,
                item.conversation.LawyerUserId,
                LawyerName = item.lawyer.FullName,
                item.conversation.IsClosed,
                ProposalStatus = item.proposal.Status,
                ContractStatus = item.contract == null
                    ? null
                    : (SmartCourt.Features.Contracts.Enums.ContractStatus?)item.contract.Status,
                item.conversation.CreatedAt,
                item.conversation.UpdatedAt,
                item.conversation.LastMessageAt
            })
            .ToListAsync(cancellationToken);

        var conversationIds = rows.Select(row => row.Id).ToArray();
        var messageRows = await (
                from message in context.ChatMessages.AsNoTracking()
                join sender in context.Users.AsNoTracking()
                    on message.SenderUserId equals sender.Id into senderJoin
                from sender in senderJoin.DefaultIfEmpty()
                where conversationIds.Contains(message.ConversationId)
                select new
                {
                    Message = message,
                    SenderName = sender == null ? null : sender.FullName
                })
            .ToListAsync(cancellationToken);
        var lastMessageRows = messageRows
            .GroupBy(item => item.Message.ConversationId)
            .Select(group => group
                .OrderByDescending(item => item.Message.CreatedAt)
                .ThenByDescending(item => item.Message.Id)
                .First())
            .ToList();
        var attachmentsByMessageId = await ChatAttachmentReadModel
            .FindForMessagesAsync(
                context,
                lastMessageRows.Select(item => item.Message.Id).ToArray(),
                cancellationToken);
        var lastMessageByConversationId = lastMessageRows.ToDictionary(
            item => item.Message.ConversationId,
            item =>
            {
                attachmentsByMessageId.TryGetValue(
                    item.Message.Id,
                    out var attachments);
                return new ChatMessageDto(
                    item.Message.Id,
                    item.Message.ConversationId,
                    item.Message.SenderUserId,
                    item.SenderName,
                    item.Message.Type.ToString(),
                    item.Message.Content,
                    item.Message.SystemCode,
                    item.Message.RelatedEntityId,
                    item.Message.CreatedAt,
                    item.Message.SenderUserId == actorUserId,
                    attachments ?? []);
            });

        var items = rows.Select(row =>
        {
            lastMessageByConversationId.TryGetValue(
                row.Id,
                out var lastMessage);
            var canWrite = !row.IsClosed
                && row.ProposalStatus == ProposalStatus.Accepted
                && row.ContractStatus is not SmartCourt.Features.Contracts.Enums.ContractStatus.Completed
                    and not SmartCourt.Features.Contracts.Enums.ContractStatus.Terminated;
            return new ChatConversationListItemDto(
                row.Id,
                row.ProposalId,
                row.LegalCaseId,
                row.CaseTitle,
                new ChatParticipantDto(row.ClientUserId, row.ClientName, "Client"),
                new ChatParticipantDto(row.LawyerUserId, row.LawyerName, "Lawyer"),
                row.IsClosed ? "Closed" : "Open",
                row.CreatedAt,
                row.UpdatedAt,
                row.LastMessageAt,
                lastMessage)
            {
                CanSendMessages = canWrite,
                CanUploadAttachments = canWrite
            };
        }).ToList();

        var page = new ChatConversationPageDto(
            items,
            request.Page,
            request.PageSize,
            totalCount,
            request.Page * request.PageSize < totalCount);
        return ApiResponse<ChatConversationPageDto>.Ok(page);
    }
}
