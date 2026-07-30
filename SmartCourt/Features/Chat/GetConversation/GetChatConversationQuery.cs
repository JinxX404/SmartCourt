using MediatR;
using SmartCourt.Common.Models;
using SmartCourt.Features.Chat.DTOs;

namespace SmartCourt.Features.Chat.GetConversation;

public sealed record GetChatConversationQuery(Guid ConversationId)
    : IRequest<ApiResponse<ChatConversationDetailDto>>;
