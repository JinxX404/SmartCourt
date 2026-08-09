using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartCourt.Common.Models;
using SmartCourt.Features.Contracts.Enums;
using SmartCourt.Features.Proposals.DTOs;
using SmartCourt.Features.Proposals.Entities;
using SmartCourt.Features.Proposals.Enums;
using SmartCourt.Features.Proposals.Shared;
using SmartCourt.Infrastructure.Providers.Events;
using SmartCourt.Interfaces;
using SmartCourt.Persistence;

namespace SmartCourt.Features.Proposals.TerminateProposal;

public sealed class TerminateProposalHandler(
    ApplicationDbContext context,
    ICurrentUserService currentUserService,
    TimeProvider timeProvider,
    IOutboxWriter outboxWriter,
    IValidator<TerminateProposalCommand> validator)
    : IRequestHandler<TerminateProposalCommand, ApiResponse<ProposalDetailDto>>
{
    public async Task<ApiResponse<ProposalDetailDto>> Handle(
        TerminateProposalCommand request,
        CancellationToken cancellationToken)
    {
        var validation = await validator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
        {
            return ApiResponse<ProposalDetailDto>.Fail(
                validation.Errors.Select(error => error.ErrorMessage).ToList());
        }

        var actorUserId = ProposalAccess.GetRequiredUserId(currentUserService);
        var proposal = await context.Proposals.SingleOrDefaultAsync(
            item => item.Id == request.ProposalId,
            cancellationToken);
        if (proposal is null
            || actorUserId != proposal.ClientUserId
                && actorUserId != proposal.LawyerUserId)
        {
            return ApiResponse<ProposalDetailDto>.Fail(
                "Proposal was not found.",
                404);
        }

        if (proposal.Status != ProposalStatus.Accepted)
        {
            return ApiResponse<ProposalDetailDto>.Fail(
                "Only an accepted proposal can be terminated.",
                409);
        }

        var hasOpenContract = await context.Contracts.AnyAsync(
            contract => contract.ProposalId == proposal.Id
                && contract.Status != ContractStatus.Terminated,
            cancellationToken);
        if (hasOpenContract)
        {
            return ApiResponse<ProposalDetailDto>.Fail(
                "Terminate the proposal's contract before ending this negotiation.",
                409);
        }

        var now = timeProvider.GetUtcNow().UtcDateTime;
        proposal.Terminate(request.Reason, actorUserId, now);
        await ProposalOutbox.EnqueueAsync(
            outboxWriter,
            ContractPaymentEventTypes.ProposalTerminated,
            proposal,
            actorUserId,
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
            actorUserId,
            cancellationToken);
        return ApiResponse<ProposalDetailDto>.Ok(detail!);
    }
}
