using MediatR;
using SmartCourt.Common.Models;
using SmartCourt.Features.Chat.DTOs;

namespace SmartCourt.Features.Chat.SendMessage;

public sealed record SendChatMessageCommand(
    Guid ConversationId,
    string Content)
    : IRequest<ApiResponse<ChatMessageDto>>;
