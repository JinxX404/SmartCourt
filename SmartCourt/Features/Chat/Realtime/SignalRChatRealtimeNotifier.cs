using Microsoft.AspNetCore.SignalR;
using SmartCourt.Features.Chat.DTOs;
using SmartCourt.Features.Chat.Hubs;
using SmartCourt.Features.Chat.Shared;

namespace SmartCourt.Features.Chat.Realtime;

public sealed class SignalRChatRealtimeNotifier(
    IHubContext<ChatHub, IChatClient> hubContext) : IChatRealtimeNotifier
{
    public async Task MessageCreatedAsync(
        ChatMessageDto message,
        CancellationToken cancellationToken)
    {
        await hubContext.Clients
            .Group(ChatGroups.Conversation(message.ConversationId))
            .ReceiveMessage(message);
    }
}
