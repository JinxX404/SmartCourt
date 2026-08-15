using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartCourt.Common.Models;
using SmartCourt.Features.Proposals.DTOs;
using SmartCourt.Features.Proposals.Entities;
using SmartCourt.Features.Proposals.Enums;
using SmartCourt.Features.Proposals.Shared;
using SmartCourt.Infrastructure.Providers.Events;
using SmartCourt.Interfaces;
using SmartCourt.Persistence;

namespace SmartCourt.Features.Proposals.RejectProposal;

public sealed class RejectProposalHandler(
    ApplicationDbContext context,
    ICurrentUserService currentUserService,
    TimeProvider timeProvider,
    IOutboxWriter outboxWriter,
    IValidator<RejectProposalCommand> validator)
    : IRequestHandler<RejectProposalCommand, ApiResponse<ProposalDetailDto>>
{
    public async Task<ApiResponse<ProposalDetailDto>> Handle(
        RejectProposalCommand request,
        CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(
            request,
            cancellationToken);
        if (!validationResult.IsValid)
        {
            return ApiResponse<ProposalDetailDto>.Fail(
                validationResult.Errors
                    .Select(error => error.ErrorMessage)
                    .ToList());
        }

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
                "Only lawyers can reject proposals.",
                403);
        }

        var proposal = await context.Proposals
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
                "The proposal expired before it was rejected.",
                409);
        }

        proposal.Reject(request.Reason, now);

        await ProposalOutbox.EnqueueAsync(
            outboxWriter,
            ContractPaymentEventTypes.ProposalRejected,
            proposal,
            lawyerUserId,
            request.Reason,
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
