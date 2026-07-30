using Microsoft.EntityFrameworkCore;
using SmartCourt.Common.Exceptions;
using SmartCourt.Features.Chat.DTOs;
using SmartCourt.Features.Chat.Entities;
using SmartCourt.Features.Chat.Realtime;
using SmartCourt.Features.Chat.Shared;
using SmartCourt.Persistence;

namespace SmartCourt.Features.Chat.Integration;

public sealed class ContractConversationService(
    ApplicationDbContext context,
    IChatConversationService conversationService,
    IChatRealtimeNotifier realtimeNotifier)
    : IContractConversationService
{
    public async Task AppendSystemMessageAsync(
        ContractConversationSystemMessage message,
        CancellationToken cancellationToken)
    {
        Validate(message);
        if (await context.ChatMessages.AnyAsync(
                item => item.Id == message.EventId,
                cancellationToken))
        {
            return;
        }

        var conversationId = await conversationService
            .EnsureForAcceptedProposalAsync(
                message.ProposalId,
                cancellationToken);
        var createdAt = message.OccurredAt.UtcDateTime;
        var chatMessage = ChatMessage.CreateSystemMessage(
            message.EventId,
            conversationId,
            message.Type,
            message.RelatedEntityId,
            createdAt);

        var conversation = await context.ChatConversations
            .SingleAsync(
                item => item.Id == conversationId,
                cancellationToken);
        context.ChatMessages.Add(chatMessage);
        conversation.MarkMessageAdded(createdAt);
        await context.SaveChangesAsync(cancellationToken);

        var dto = new ChatMessageDto(
            chatMessage.Id,
            chatMessage.ConversationId,
            chatMessage.SenderUserId,
            SenderName: null,
            chatMessage.Type.ToString(),
            chatMessage.Content,
            chatMessage.SystemCode,
            chatMessage.RelatedEntityId,
            chatMessage.CreatedAt,
            IsMine: false);
        await realtimeNotifier.MessageCreatedAsync(
            dto,
            cancellationToken);
    }

    private static void Validate(ContractConversationSystemMessage message)
    {
        if (message.EventId == Guid.Empty
            || message.ProposalId == Guid.Empty
            || message.RelatedEntityId == Guid.Empty)
        {
            throw new BusinessException(
                "Contract conversation system message identifiers are required.");
        }

        if (message.OccurredAt == default)
        {
            throw new BusinessException(
                "Contract conversation system message time is required.");
        }
    }
}
