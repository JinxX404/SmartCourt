using Microsoft.EntityFrameworkCore;
using SmartCourt.Common.Exceptions;
using SmartCourt.Features.Proposals.Enums;
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

    public static async Task<bool> CanAccessConversationAsync(
        ApplicationDbContext context,
        Guid conversationId,
        Guid userId,
        CancellationToken cancellationToken)
    {
        return await (
            from conversation in context.ChatConversations
            join proposal in context.Proposals
                on conversation.ProposalId equals proposal.Id
            where conversation.Id == conversationId
                && (conversation.ClientUserId == userId
                    || conversation.LawyerUserId == userId)
                && !(proposal.Status == ProposalStatus.Superseded
                    && conversation.LawyerUserId == userId)
            select conversation.Id)
            .AnyAsync(cancellationToken);
    }
}
