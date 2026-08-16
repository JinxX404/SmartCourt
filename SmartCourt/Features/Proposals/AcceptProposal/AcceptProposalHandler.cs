using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartCourt.Common.Models;
using SmartCourt.Features.Chat.Shared;
using SmartCourt.Common.Enums;
using SmartCourt.Features.Proposals.DTOs;
using SmartCourt.Features.Proposals.Entities;
using SmartCourt.Features.Proposals.Enums;
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
            .Include(item => item.Case)
            .SingleOrDefaultAsync(
                item => item.Id == request.ProposalId,
                cancellationToken);
        if (proposal is null || proposal.LawyerUserId != lawyerUserId)
        {
            return ApiResponse<ProposalDetailDto>.Fail(
                "Proposal was not found.",
                404);
        }

        var now = timeProvider.GetUtcNow();
        if (proposal.Status == ProposalStatus.Pending
            && proposal.ExpiresAt <= now)
        {
            proposal.Expire(now);
            await ProposalOutbox.EnqueueAsync(
                outboxWriter,
                ContractPaymentEventTypes.ProposalExpired,
                proposal,
                actorUserId: null,
                reason: null,
                cancellationToken);
            if (!await ProposalPersistence.TrySaveAsync(context, cancellationToken))
            {
                return ApiResponse<ProposalDetailDto>.Fail(
                    "The proposal changed while it was being processed.",
                    409);
            }
            return ApiResponse<ProposalDetailDto>.Fail(
                "The proposal expired before it was accepted.",
                409);
        }

        proposal.Accept(now);
        proposal.Case.Status = CaseStatus.Matched;
        proposal.Case.UpdatedAt = now;
        await chatConversationService.EnsureForAcceptedProposalAsync(
            proposal,
            cancellationToken);

        await ProposalOutbox.EnqueueAsync(
            outboxWriter,
            ContractPaymentEventTypes.ProposalAccepted,
            proposal,
            lawyerUserId,
            reason: null,
            cancellationToken);

        if (!await ProposalPersistence.TrySaveAsync(context, cancellationToken))
        {
            return ApiResponse<ProposalDetailDto>.Fail(
                "The proposal changed while it was being processed.",
                409);
        }

        var detail = await ProposalReadModel.FindDetailAsync(
            context,
            proposal.Id,
            lawyerUserId,
            cancellationToken);
        return ApiResponse<ProposalDetailDto>.Ok(detail!);
    }

}
