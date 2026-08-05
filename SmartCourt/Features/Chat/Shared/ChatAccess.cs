using Microsoft.EntityFrameworkCore;
using SmartCourt.Common.Exceptions;
using SmartCourt.Interfaces;
using SmartCourt.Persistence;

namespace SmartCourt.Features.Chat.Shared;

internal static class ChatAccess
{
    public static Guid GetRequiredUserId(ICurrentUserService currentUserService)
    {
        return currentUserService.UserId
            ?? throw new AuthenticationException("Authentication is required.");
    }

    public static async Task<bool> IsParticipantAsync(
        ApplicationDbContext context,
        Guid conversationId,
        Guid userId,
        CancellationToken cancellationToken)
    {
        return await context.ChatConversations.AnyAsync(
            conversation =>
                conversation.Id == conversationId
                && (conversation.ClientUserId == userId
                    || conversation.LawyerUserId == userId),
            cancellationToken);
    }
}
