using SmartCourt.Features.Chat.DTOs;

namespace SmartCourt.Features.Chat.Realtime;

public interface IChatRealtimeNotifier
{
    Task MessageCreatedAsync(
        ChatMessageDto message,
        CancellationToken cancellationToken);
}
