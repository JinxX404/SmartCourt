using MediatR;
using SmartCourt.Common.Models;
using SmartCourt.Features.Chat.DTOs;

namespace SmartCourt.Features.Chat.Attachments;

public sealed record SendChatAttachmentsCommand(
    Guid ConversationId,
    string? Caption,
    IReadOnlyList<IFormFile> Files)
    : IRequest<ApiResponse<ChatMessageDto>>;
