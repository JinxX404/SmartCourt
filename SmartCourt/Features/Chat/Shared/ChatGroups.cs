namespace SmartCourt.Features.Chat.Shared;

internal static class ChatGroups
{
    public static string Conversation(Guid conversationId)
    {
        return $"chat:conversation:{conversationId:N}";
    }
}
