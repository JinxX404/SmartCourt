using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartCourt.Common.Models;
using SmartCourt.Features.Proposals.DTOs;
using SmartCourt.Features.Proposals.Enums;
using SmartCourt.Features.Proposals.Shared;
using SmartCourt.Interfaces;
using SmartCourt.Persistence;

namespace SmartCourt.Features.Proposals.UpdateProposal;

public sealed class UpdateProposalHandler(
    ApplicationDbContext context,
    ICurrentUserService currentUserService,
    TimeProvider timeProvider,
    IValidator<UpdateProposalCommand> validator)
    : IRequestHandler<UpdateProposalCommand, ApiResponse<ProposalDetailDto>>
{
    public async Task<ApiResponse<ProposalDetailDto>> Handle(
        UpdateProposalCommand request,
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

        if (proposal.Status != ProposalStatus.Pending)
        {
            return ApiResponse<ProposalDetailDto>.Fail(
                "لا يمكن تعديل العرض في حالته الحالية بعد الآن.",
                409);
        }

        proposal.UpdateMessage(request.Message, timeProvider.GetUtcNow());
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
