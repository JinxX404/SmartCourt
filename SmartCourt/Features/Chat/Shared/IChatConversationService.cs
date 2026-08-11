using SmartCourt.Features.Proposals.Entities;

namespace SmartCourt.Features.Chat.Shared;

public interface IChatConversationService
{
    Task<Guid> EnsureForAcceptedProposalAsync(
        Proposal proposal,
        CancellationToken cancellationToken);

    Task<Guid> EnsureForAcceptedProposalAsync(
        Guid proposalId,
        CancellationToken cancellationToken);

    Task<bool> CanAccessConversationAsync(
        Guid conversationId,
        Guid userId,
        CancellationToken cancellationToken);
}
