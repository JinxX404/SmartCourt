using Microsoft.EntityFrameworkCore;
using SmartCourt.Features.Chat.DTOs;
using SmartCourt.Persistence;

namespace SmartCourt.Features.Chat.Shared;

internal static class ChatAttachmentReadModel
{
    public static async Task<
        IReadOnlyDictionary<Guid, IReadOnlyList<ChatAttachmentDto>>>
        FindForMessagesAsync(
            ApplicationDbContext context,
            IReadOnlyCollection<Guid> messageIds,
            CancellationToken cancellationToken)
    {
        if (messageIds.Count == 0)
        {
            return new Dictionary<Guid, IReadOnlyList<ChatAttachmentDto>>();
        }

        var rows = await (
            from attachment in context.ChatMessageAttachments.AsNoTracking()
            join message in context.ChatMessages.AsNoTracking()
                on attachment.MessageId equals message.Id
            join storedFile in context.StoredFiles.AsNoTracking()
                on attachment.StoredFileId equals storedFile.Id
            where messageIds.Contains(attachment.MessageId)
                && !storedFile.IsDeleted
            orderby attachment.CreatedAt, attachment.Id
            select new
            {
                attachment.MessageId,
                attachment.Id,
                message.ConversationId,
                storedFile.OriginalFileName,
                storedFile.ContentType,
                storedFile.SizeInBytes
            })
            .ToListAsync(cancellationToken);

        return rows
            .GroupBy(row => row.MessageId)
            .ToDictionary(
                group => group.Key,
                group => (IReadOnlyList<ChatAttachmentDto>)group
                    .Select(row => new ChatAttachmentDto(
                        row.Id,
                        row.OriginalFileName,
                        row.ContentType,
                        row.SizeInBytes,
                        $"/api/chat/conversations/{row.ConversationId}/attachments/{row.Id}/download"))
                    .ToList());
    }
}
