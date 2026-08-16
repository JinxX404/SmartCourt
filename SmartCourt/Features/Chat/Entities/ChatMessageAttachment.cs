using SmartCourt.Common.Domain;
using SmartCourt.Entities;

namespace SmartCourt.Features.Chat.Entities;

public sealed class ChatMessageAttachment
{
    private ChatMessageAttachment()
    {
    }

    internal ChatMessageAttachment(
        Guid id,
        Guid messageId,
        Guid storedFileId,
        DateTimeOffset createdAt)
    {
        Id = EntityGuard.NotEmpty(id, nameof(id));
        MessageId = EntityGuard.NotEmpty(messageId, nameof(messageId));
        StoredFileId = EntityGuard.NotEmpty(storedFileId, nameof(storedFileId));
        CreatedAt = EntityGuard.Utc(createdAt, nameof(createdAt));
    }

    public Guid Id { get; internal set; }
    public Guid MessageId { get; internal set; }
    public Guid StoredFileId { get; internal set; }
    public DateTimeOffset CreatedAt { get; internal set; }
    public ChatMessage Message { get; internal set; } = null!;
    public StoredFile StoredFile { get; internal set; } = null!;
}
