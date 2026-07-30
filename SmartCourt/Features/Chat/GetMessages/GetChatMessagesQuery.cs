using MediatR;
using SmartCourt.Common.Models;
using SmartCourt.Features.Chat.DTOs;

namespace SmartCourt.Features.Chat.GetMessages;

public sealed record GetChatMessagesQuery(
    Guid ConversationId,
    int Page = 1,
    int PageSize = 50)
    : IRequest<ApiResponse<ChatMessagePageDto>>;
