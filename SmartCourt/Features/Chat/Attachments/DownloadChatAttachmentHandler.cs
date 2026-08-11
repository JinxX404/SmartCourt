using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartCourt.Common.Models;
using SmartCourt.Features.Chat.Shared;
using SmartCourt.Interfaces;
using SmartCourt.Interfaces.Providers;
using SmartCourt.Persistence;

namespace SmartCourt.Features.Chat.Attachments;

public sealed class DownloadChatAttachmentHandler(
    ApplicationDbContext context,
    ICurrentUserService currentUserService,
    IFileStorageService fileStorageService)
    : IRequestHandler<
        DownloadChatAttachmentQuery,
        ApiResponse<ChatAttachmentDownloadResult>>
{
    public async Task<ApiResponse<ChatAttachmentDownloadResult>> Handle(
        DownloadChatAttachmentQuery request,
        CancellationToken cancellationToken)
    {
        if (request.ConversationId == Guid.Empty
            || request.AttachmentId == Guid.Empty)
        {
            return ApiResponse<ChatAttachmentDownloadResult>.Fail(
                "Conversation and attachment IDs are required.");
        }

        var actorUserId = ChatAccess.GetRequiredUserId(currentUserService);
        if (!await ChatAccess.CanAccessConversationAsync(
                context,
                request.ConversationId,
                actorUserId,
                cancellationToken))
        {
            return ApiResponse<ChatAttachmentDownloadResult>.Fail(
                "Attachment was not found.",
                404);
        }

        var file = await (
            from attachment in context.ChatMessageAttachments.AsNoTracking()
            join message in context.ChatMessages.AsNoTracking()
                on attachment.MessageId equals message.Id
            join storedFile in context.StoredFiles.AsNoTracking()
                on attachment.StoredFileId equals storedFile.Id
            where attachment.Id == request.AttachmentId
                && message.ConversationId == request.ConversationId
                && !storedFile.IsDeleted
            select new
            {
                storedFile.FileUrl,
                storedFile.ContentType,
                storedFile.OriginalFileName
            })
            .SingleOrDefaultAsync(cancellationToken);
        if (file is null)
        {
            return ApiResponse<ChatAttachmentDownloadResult>.Fail(
                "Attachment was not found.",
                404);
        }

        var content = await fileStorageService.DownloadAsync(
            file.FileUrl,
            cancellationToken);
        return ApiResponse<ChatAttachmentDownloadResult>.Ok(
            new ChatAttachmentDownloadResult(
                content,
                file.ContentType,
                file.OriginalFileName));
    }
}
