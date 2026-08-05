using Microsoft.EntityFrameworkCore;
using SmartCourt.Common.Exceptions;
using SmartCourt.Features.Chat.Entities;
using SmartCourt.Features.Proposals.Entities;
using SmartCourt.Features.Proposals.Enums;
using SmartCourt.Persistence;

namespace SmartCourt.Features.Chat.Shared;

public sealed class ChatConversationService(
    ApplicationDbContext context,
    TimeProvider timeProvider) : IChatConversationService
{
    public async Task<Guid> EnsureForAcceptedProposalAsync(
        Proposal proposal,
        CancellationToken cancellationToken)
    {
        if (proposal.Status != ProposalStatus.Accepted)
        {
            throw new BusinessException(
                "Chat conversations can only be opened for accepted proposals.");
        }

        var existingId = await context.ChatConversations
            .Where(conversation => conversation.ProposalId == proposal.Id)
            .Select(conversation => (Guid?)conversation.Id)
            .SingleOrDefaultAsync(cancellationToken);
        if (existingId.HasValue)
        {
            return existingId.Value;
        }

        var createdAt = proposal.RespondedAt
            ?? timeProvider.GetUtcNow().UtcDateTime;
        var conversation = new ChatConversation(
            Guid.NewGuid(),
            proposal.Id,
            proposal.LegalCaseId,
            proposal.ClientUserId,
            proposal.LawyerUserId,
            createdAt);
        context.ChatConversations.Add(conversation);
        return conversation.Id;
    }

    public async Task<Guid> EnsureForAcceptedProposalAsync(
        Guid proposalId,
        CancellationToken cancellationToken)
    {
        if (proposalId == Guid.Empty)
        {
            throw new BusinessException("Proposal ID is required.");
        }

        var proposal = await context.Proposals
            .SingleOrDefaultAsync(
                item => item.Id == proposalId,
                cancellationToken)
            ?? throw new NotFoundException("Proposal was not found.");

        return await EnsureForAcceptedProposalAsync(
            proposal,
            cancellationToken);
    }

    public async Task<bool> IsParticipantAsync(
        Guid conversationId,
        Guid userId,
        CancellationToken cancellationToken)
    {
        if (conversationId == Guid.Empty || userId == Guid.Empty)
        {
            return false;
        }

        return await ChatAccess.IsParticipantAsync(
            context,
            conversationId,
            userId,
            cancellationToken);
    }
}
