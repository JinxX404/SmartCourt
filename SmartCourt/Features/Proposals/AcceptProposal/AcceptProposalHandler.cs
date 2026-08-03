using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartCourt.Common.Models;
using SmartCourt.Features.Chat.Shared;
using SmartCourt.Common.Enums;
using SmartCourt.Features.Proposals.DTOs;
using SmartCourt.Features.Proposals.Entities;
using SmartCourt.Features.Proposals.Shared;
using SmartCourt.Infrastructure.Providers.Events;
using SmartCourt.Interfaces;
using SmartCourt.Persistence;

namespace SmartCourt.Features.Proposals.AcceptProposal;

public sealed class AcceptProposalHandler(
    ApplicationDbContext context,
    ICurrentUserService currentUserService,
    TimeProvider timeProvider,
    IOutboxWriter outboxWriter,
    IChatConversationService chatConversationService)
    : IRequestHandler<AcceptProposalCommand, ApiResponse<ProposalDetailDto>>
{
    public async Task<ApiResponse<ProposalDetailDto>> Handle(
        AcceptProposalCommand request,
        CancellationToken cancellationToken)
    {
        if (request.ProposalId == Guid.Empty)
        {
            return ApiResponse<ProposalDetailDto>.Fail(
                "Proposal ID is required.");
        }

        var lawyerUserId = ProposalAccess.GetRequiredUserId(currentUserService);
        if (!await ProposalAccess.HasRoleAsync(
                context,
                lawyerUserId,
                "Lawyer",
                cancellationToken))
        {
            return ApiResponse<ProposalDetailDto>.Fail(
                "Only lawyers can accept proposals.",
                403);
        }

        var proposal = await context.Proposals
            .Include(item => item.LegalCase)
            .SingleOrDefaultAsync(
                item => item.Id == request.ProposalId,
                cancellationToken);
        if (proposal is null || proposal.LawyerUserId != lawyerUserId)
        {
            return ApiResponse<ProposalDetailDto>.Fail(
                "Proposal was not found.",
                404);
        }

        var now = timeProvider.GetUtcNow().UtcDateTime;
        proposal.Accept(now);
        proposal.LegalCase.Status = CaseStatus.Matched;
        proposal.LegalCase.UpdatedAt = now;
        await chatConversationService.EnsureForAcceptedProposalAsync(
            proposal,
            cancellationToken);

        await outboxWriter.EnqueueAsync(
            new OutboxEvent(
                ContractPaymentEventTypes.ProposalAccepted,
                1,
                new ProposalEventPayload(
                    proposal.Id,
                    proposal.LegalCaseId,
                    proposal.ClientUserId,
                    proposal.LawyerUserId),
                nameof(Proposal),
                proposal.Id,
                Guid.NewGuid()),
            cancellationToken);

        await context.SaveChangesAsync(cancellationToken);

        var detail = await ProposalReadModel.FindDetailAsync(
            context,
            proposal.Id,
            lawyerUserId,
            cancellationToken);
        return ApiResponse<ProposalDetailDto>.Ok(detail!);
    }
}
