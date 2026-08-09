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

namespace SmartCourt.Features.Proposals.CancelProposal;

public sealed class CancelProposalHandler(
    ApplicationDbContext context,
    ICurrentUserService currentUserService,
    TimeProvider timeProvider,
    IOutboxWriter outboxWriter,
    IValidator<CancelProposalCommand> validator)
    : IRequestHandler<CancelProposalCommand, ApiResponse<ProposalDetailDto>>
{
    public async Task<ApiResponse<ProposalDetailDto>> Handle(
        CancelProposalCommand request,
        CancellationToken cancellationToken)
    {
        var validation = await validator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
        {
            return ApiResponse<ProposalDetailDto>.Fail(
                validation.Errors.Select(error => error.ErrorMessage).ToList());
        }

        var clientUserId = ProposalAccess.GetRequiredUserId(currentUserService);
        var proposal = await context.Proposals.SingleOrDefaultAsync(
            item => item.Id == request.ProposalId
                && item.ClientUserId == clientUserId,
            cancellationToken);
        if (proposal is null)
        {
            return ApiResponse<ProposalDetailDto>.Fail(
                "Proposal was not found.",
                404);
        }

        var now = timeProvider.GetUtcNow().UtcDateTime;
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
                "The proposal has already expired.",
                409);
        }

        if (proposal.Status != ProposalStatus.Pending)
        {
            return ApiResponse<ProposalDetailDto>.Fail(
                "Only a pending proposal can be cancelled.",
                409);
        }

        proposal.Cancel(request.Reason, clientUserId, now);
        await ProposalOutbox.EnqueueAsync(
            outboxWriter,
            ContractPaymentEventTypes.ProposalCancelled,
            proposal,
            clientUserId,
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
            clientUserId,
            cancellationToken);
        return ApiResponse<ProposalDetailDto>.Ok(detail!);
    }

}
