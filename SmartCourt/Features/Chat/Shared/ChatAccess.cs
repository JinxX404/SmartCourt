using Microsoft.EntityFrameworkCore;
using SmartCourt.Common.Exceptions;
using SmartCourt.Features.Contracts.Enums;
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
            join contract in context.Contracts
                on conversation.ProposalId equals contract.ProposalId into contractJoin
            from contract in contractJoin.DefaultIfEmpty()
            where conversation.Id == conversationId
                && (conversation.ClientUserId == userId
                    || (conversation.LawyerUserId == userId
                        && proposal.Status != ProposalStatus.Superseded
                        && proposal.Status != ProposalStatus.Terminated
                        && (contract == null
                            || (contract.Status != ContractStatus.Completed
                                && contract.Status != ContractStatus.Terminated))))
            select conversation.Id)
            .AnyAsync(cancellationToken);
    }

    public static bool IsHiddenFromLawyer(
        ProposalStatus proposalStatus,
        ContractStatus? contractStatus,
        Guid lawyerUserId,
        Guid actorUserId)
    {
        return actorUserId == lawyerUserId
            && (proposalStatus is ProposalStatus.Superseded or ProposalStatus.Terminated
                || contractStatus is ContractStatus.Completed or ContractStatus.Terminated);
    }
}
