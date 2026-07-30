using MediatR;
using SmartCourt.Common.Models;
using SmartCourt.Features.Chat.DTOs;
using SmartCourt.Features.Chat.Shared;
using SmartCourt.Interfaces;
using SmartCourt.Persistence;

namespace SmartCourt.Features.Chat.GetConversation;

public sealed class GetChatConversationHandler(
    ApplicationDbContext context,
    ICurrentUserService currentUserService)
    : IRequestHandler<GetChatConversationQuery, ApiResponse<ChatConversationDetailDto>>
{
    public async Task<ApiResponse<ChatConversationDetailDto>> Handle(
        GetChatConversationQuery request,
        CancellationToken cancellationToken)
    {
        if (request.ConversationId == Guid.Empty)
        {
            return ApiResponse<ChatConversationDetailDto>.Fail(
                "Conversation ID is required.");
        }

        var actorUserId = ChatAccess.GetRequiredUserId(currentUserService);
        var detail = await ChatReadModel.FindConversationAsync(
            context,
            request.ConversationId,
            actorUserId,
            cancellationToken);
        return detail is null
            ? ApiResponse<ChatConversationDetailDto>.Fail(
                "Conversation was not found.",
                404)
            : ApiResponse<ChatConversationDetailDto>.Ok(detail);
    }
}
