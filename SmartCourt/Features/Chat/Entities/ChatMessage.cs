using SmartCourt.Common.Domain;
using SmartCourt.Common.Exceptions;
using SmartCourt.Features.Chat.Enums;
using SmartCourt.Features.Chat.Integration;

namespace SmartCourt.Features.Chat.Entities;

public sealed class ChatMessage
{
    public const int MaximumContentLength = 2_000;

    private ChatMessage()
    {
    }

    private ChatMessage(
        Guid id,
        Guid conversationId,
        Guid? senderUserId,
        ChatMessageType type,
        string content,
        string? systemCode,
        Guid? relatedEntityId,
        DateTime createdAt)
    {
        Id = EntityGuard.NotEmpty(id, nameof(id));
        ConversationId = EntityGuard.NotEmpty(
            conversationId,
            nameof(conversationId));
        SenderUserId = EntityGuard.OptionalGuid(
            senderUserId,
            nameof(senderUserId));
        Type = type;
        Content = EntityGuard.Required(content, nameof(content)).Trim();
        if (Content.Length > MaximumContentLength)
        {
            throw new BusinessException(
                $"Chat message cannot exceed {MaximumContentLength} characters.");
        }

        SystemCode = string.IsNullOrWhiteSpace(systemCode)
            ? null
            : systemCode.Trim();
        RelatedEntityId = EntityGuard.OptionalGuid(
            relatedEntityId,
            nameof(relatedEntityId));
        CreatedAt = EntityGuard.Utc(createdAt, nameof(createdAt));
    }

    public Guid Id { get; internal set; }
    public Guid ConversationId { get; internal set; }
    public Guid? SenderUserId { get; internal set; }
    public ChatMessageType Type { get; internal set; }
    public string Content { get; internal set; } = string.Empty;
    public string? SystemCode { get; internal set; }
    public Guid? RelatedEntityId { get; internal set; }
    public DateTime CreatedAt { get; internal set; }
    public ChatConversation Conversation { get; internal set; } = null!;
    public ICollection<ChatMessageAttachment> Attachments { get; internal set; } = [];

    internal static ChatMessage CreateUserMessage(
        Guid id,
        Guid conversationId,
        Guid senderUserId,
        string content,
        DateTime createdAt)
    {
        return new ChatMessage(
            id,
            conversationId,
            senderUserId,
            ChatMessageType.User,
            content,
            systemCode: null,
            relatedEntityId: null,
            createdAt);
    }

    internal static ChatMessage CreateUserAttachmentMessage(
        Guid id,
        Guid conversationId,
        Guid senderUserId,
        string? caption,
        int attachmentCount,
        DateTime createdAt)
    {
        if (attachmentCount <= 0)
        {
            throw new BusinessException(
                "An attachment message requires at least one file.");
        }

        var content = string.IsNullOrWhiteSpace(caption)
            ? attachmentCount == 1
                ? "Shared an attachment."
                : $"Shared {attachmentCount} attachments."
            : caption.Trim();

        return CreateUserMessage(
            id,
            conversationId,
            senderUserId,
            content,
            createdAt);
    }

    internal static ChatMessage CreateSystemMessage(
        Guid id,
        Guid conversationId,
        ContractConversationMessageType messageType,
        Guid relatedEntityId,
        DateTime createdAt)
    {
        return CreateSystemMessage(
            id,
            conversationId,
            messageType.ToString(),
            ChatSystemMessageText.For(messageType),
            relatedEntityId,
            createdAt);
    }

    internal static ChatMessage CreateSystemMessage(
        Guid id,
        Guid conversationId,
        string systemCode,
        string content,
        Guid relatedEntityId,
        DateTime createdAt)
    {
        return new ChatMessage(
            id,
            conversationId,
            senderUserId: null,
            ChatMessageType.System,
            content,
            systemCode,
            relatedEntityId,
            createdAt);
    }
}
