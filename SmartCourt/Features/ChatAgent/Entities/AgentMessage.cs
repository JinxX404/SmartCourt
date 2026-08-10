using SmartCourt.Common.Domain;
using SmartCourt.Features.ChatAgent.Enums;

namespace SmartCourt.Features.ChatAgent.Entities;

public sealed class AgentMessage
{
    private AgentMessage()
    {
    }

    private AgentMessage(
        Guid id,
        Guid conversationId,
        AgentMessageRole role,
        string content,
        DateTime createdAt)
    {
        Id = EntityGuard.NotEmpty(id, nameof(id));
        ConversationId = EntityGuard.NotEmpty(conversationId, nameof(conversationId));
        Role = role;
        Content = EntityGuard.Required(content, nameof(content)).Trim();
        CreatedAt = EntityGuard.Utc(createdAt, nameof(createdAt));
    }

    public Guid Id { get; internal set; }
    public Guid ConversationId { get; internal set; }
    public AgentMessageRole Role { get; internal set; }
    public string Content { get; internal set; } = string.Empty;
    public DateTime CreatedAt { get; internal set; }

    public AgentConversation Conversation { get; internal set; } = null!;

    internal static AgentMessage CreateUserMessage(
        Guid id,
        Guid conversationId,
        string content,
        DateTime createdAt)
    {
        return new AgentMessage(
            id,
            conversationId,
            AgentMessageRole.User,
            content,
            createdAt);
    }

    internal static AgentMessage CreateAssistantMessage(
        Guid id,
        Guid conversationId,
        string content,
        DateTime createdAt)
    {
        return new AgentMessage(
            id,
            conversationId,
            AgentMessageRole.Assistant,
            content,
            createdAt);
    }
}
