using Microsoft.EntityFrameworkCore;
using SmartCourt.Common.Enums;
using SmartCourt.Common.Exceptions;
using SmartCourt.Features.Chat.Entities;
using SmartCourt.Features.Chat.Shared;
using SmartCourt.Features.Contracts.Integration;
using SmartCourt.Features.Proposals.Entities;
using SmartCourt.Features.Proposals.Enums;
using SmartCourt.Features.Proposals.Shared;
using SmartCourt.Infrastructure.Providers.Events;
using SmartCourt.Persistence;

namespace SmartCourt.Features.Case.Integration;

public sealed class ContractCaseAssignmentService(
    ApplicationDbContext context,
    IChatConversationService chatConversationService,
    IOutboxWriter outboxWriter) : IContractCaseAssignmentService
{
    public async Task AssignAsync(
        ContractCaseAssignment assignment,
        CancellationToken cancellationToken)
    {
        Validate(assignment);
        var legalCase = await context.Cases.SingleOrDefaultAsync(
            item => item.Id == assignment.LegalCaseId
                && item.ClientId == assignment.ClientUserId,
            cancellationToken)
            ?? throw new BusinessException(
                "The contract case could not be assigned.");
        if (legalCase.Status == CaseStatus.Assigned)
        {
            throw new ConflictException(
                "Another contract is already active for this case.");
        }

        if (legalCase.Status != CaseStatus.Matched)
        {
            throw new BusinessException(
                "Only a matched case can be assigned to a lawyer.");
        }

        var selectedProposal = await context.Proposals.SingleOrDefaultAsync(
            proposal => proposal.Id == assignment.ProposalId
                && proposal.LegalCaseId == assignment.LegalCaseId
                && proposal.ClientUserId == assignment.ClientUserId
                && proposal.LawyerUserId == assignment.LawyerUserId
                && proposal.Status == ProposalStatus.Accepted,
            cancellationToken)
            ?? throw new BusinessException(
                "The active contract must belong to an accepted proposal.");

        var winningConversationId = await ResolveWinningConversationIdAsync(
            selectedProposal,
            assignment,
            cancellationToken);
        var occurredAt = assignment.OccurredAt.UtcDateTime;
        legalCase.LawyerId = assignment.LawyerUserId;
        legalCase.ChatId = winningConversationId;
        legalCase.Status = CaseStatus.Assigned;
        legalCase.UpdatedAt = occurredAt;

        var competingProposals = await context.Proposals
            .Where(proposal =>
                proposal.LegalCaseId == assignment.LegalCaseId
                && proposal.Id != selectedProposal.Id
                && (proposal.Status == ProposalStatus.Pending
                    || proposal.Status == ProposalStatus.Accepted))
            .ToListAsync(cancellationToken);
        foreach (var proposal in competingProposals)
        {
            proposal.Supersede(occurredAt);
            await ProposalOutbox.EnqueueAsync(
                outboxWriter,
                ContractPaymentEventTypes.ProposalSuperseded,
                proposal,
                assignment.ClientUserId,
                proposal.DecisionReason,
                cancellationToken);
        }
    }

    private async Task<Guid> ResolveWinningConversationIdAsync(
        Proposal selectedProposal,
        ContractCaseAssignment assignment,
        CancellationToken cancellationToken)
    {
        var conversation = await context.ChatConversations
            .SingleOrDefaultAsync(
                item => item.ProposalId == selectedProposal.Id,
                cancellationToken);
        if (conversation is null)
        {
            return await chatConversationService
                .EnsureForAcceptedProposalAsync(
                    selectedProposal,
                    cancellationToken);
        }

        ValidateWinningConversation(conversation, assignment);
        return conversation.Id;
    }

    private static void ValidateWinningConversation(
        ChatConversation conversation,
        ContractCaseAssignment assignment)
    {
        if (conversation.IsClosed)
        {
            throw new BusinessException(
                "The winning proposal conversation is closed.");
        }

        if (conversation.LegalCaseId != assignment.LegalCaseId
            || conversation.ClientUserId != assignment.ClientUserId
            || conversation.LawyerUserId != assignment.LawyerUserId)
        {
            throw new BusinessException(
                "The winning proposal conversation does not match the contract assignment.");
        }
    }

    private static void Validate(ContractCaseAssignment assignment)
    {
        if (assignment.ContractId == Guid.Empty
            || assignment.ProposalId == Guid.Empty
            || assignment.LegalCaseId == Guid.Empty
            || assignment.ClientUserId == Guid.Empty
            || assignment.LawyerUserId == Guid.Empty
            || assignment.OccurredAt == default)
        {
            throw new BusinessException(
                "Contract case assignment data is incomplete.");
        }
    }
}
