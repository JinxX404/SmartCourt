using SmartCourt.Features.Chat.DTOs;

namespace SmartCourt.Features.Chat.Hubs;

public interface IChatClient
{
    Task ReceiveMessage(ChatMessageDto message);
}
