using MediatR;
using SmartCourt.Common.Models;
using SmartCourt.Features.Chat.DTOs;

namespace SmartCourt.Features.Chat.GetConversations;

public sealed record GetChatConversationsQuery(
    string? Search = null,
    int Page = 1,
    int PageSize = 20)
    : IRequest<ApiResponse<ChatConversationPageDto>>;
