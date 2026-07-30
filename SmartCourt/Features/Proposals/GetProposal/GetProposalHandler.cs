using MediatR;
using SmartCourt.Common.Models;
using SmartCourt.Features.Proposals.DTOs;
using SmartCourt.Features.Proposals.Shared;
using SmartCourt.Interfaces;

namespace SmartCourt.Features.Proposals.GetProposal;

public sealed class GetProposalHandler(
    SmartCourt.Persistence.ApplicationDbContext context,
    ICurrentUserService currentUserService)
    : IRequestHandler<GetProposalQuery, ApiResponse<ProposalDetailDto>>
{
    public async Task<ApiResponse<ProposalDetailDto>> Handle(
        GetProposalQuery request,
        CancellationToken cancellationToken)
    {
        if (request.ProposalId == Guid.Empty)
        {
            return ApiResponse<ProposalDetailDto>.Fail("Proposal ID is required.");
        }

        var actorUserId = ProposalAccess.GetRequiredUserId(currentUserService);
        var proposal = await ProposalReadModel.FindDetailAsync(
            context,
            request.ProposalId,
            actorUserId,
            cancellationToken);
        return proposal is null
            ? ApiResponse<ProposalDetailDto>.Fail("Proposal was not found.", 404)
            : ApiResponse<ProposalDetailDto>.Ok(proposal);
    }
}
