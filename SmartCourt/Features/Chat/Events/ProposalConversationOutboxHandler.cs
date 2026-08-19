using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using SmartCourt.Features.Chat.DTOs;
using SmartCourt.Features.Chat.Entities;
using SmartCourt.Features.Chat.Realtime;
using SmartCourt.Infrastructure.Persistence.Entities;
using SmartCourt.Infrastructure.Providers.Events;
using SmartCourt.Persistence;

namespace SmartCourt.Features.Chat.Events;

public sealed class ProposalConversationOutboxHandler(
    ApplicationDbContext context,
    IChatRealtimeNotifier realtimeNotifier) : IOutboxEventHandler
{
    private static readonly JsonSerializerOptions SerializerOptions = new(
        JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true
    };

    public IReadOnlyCollection<string> EventTypes =>
    [
        ContractPaymentEventTypes.ProposalTerminated,
        ContractPaymentEventTypes.ProposalSuperseded
    ];

    public async Task HandleAsync(
        OutboxMessage message,
        CancellationToken cancellationToken)
    {
        if (await context.ChatMessages.AnyAsync(
                item => item.Id == message.Id,
                cancellationToken))
        {
            return;
        }

        var payload = JsonSerializer.Deserialize<ProposalEventPayload>(
            message.Payload,
            SerializerOptions)
            ?? throw new InvalidOperationException(
                "Proposal conversation payload is invalid.");
        var conversation = await context.ChatConversations.SingleOrDefaultAsync(
            item => item.ProposalId == payload.ProposalId,
            cancellationToken);
        if (conversation is null)
        {
            return;
        }

        var occurredAt = message.CreatedAt;
        var content = message.EventType == ContractPaymentEventTypes.ProposalSuperseded
            ? "تم إغلاق هذه المحادثة التفاوضية لتفعيل عقد آخر لنفس القضية."
            : string.IsNullOrWhiteSpace(payload.Reason)
                ? "تم إنهاء التفاوض على هذا العرض."
                : $"تم إنهاء التفاوض على هذا العرض. السبب: {payload.Reason.Trim()}";
        var chatMessage = ChatMessage.CreateSystemMessage(
            message.Id,
            conversation.Id,
            message.EventType,
            content,
            payload.ProposalId,
            occurredAt);
        context.ChatMessages.Add(chatMessage);
        conversation.MarkMessageAdded(occurredAt);
        conversation.Close(occurredAt);
        await context.SaveChangesAsync(cancellationToken);

        await realtimeNotifier.MessageCreatedAsync(
            new ChatMessageDto(
                chatMessage.Id,
                chatMessage.ConversationId,
                SenderUserId: null,
                SenderName: null,
                chatMessage.Type.ToString(),
                chatMessage.Content,
                chatMessage.SystemCode,
                chatMessage.RelatedEntityId,
                chatMessage.CreatedAt,
                IsMine: false,
                Attachments: []),
            cancellationToken);
    }
}
