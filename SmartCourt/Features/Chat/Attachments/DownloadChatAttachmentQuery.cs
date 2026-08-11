using MediatR;
using SmartCourt.Common.Models;

namespace SmartCourt.Features.Chat.Attachments;

public sealed record DownloadChatAttachmentQuery(
    Guid ConversationId,
    Guid AttachmentId)
    : IRequest<ApiResponse<ChatAttachmentDownloadResult>>;

public sealed record ChatAttachmentDownloadResult(
    byte[] Content,
    string ContentType,
    string FileName);
