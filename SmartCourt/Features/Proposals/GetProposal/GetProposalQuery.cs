using MediatR;
using SmartCourt.Common.Models;
using SmartCourt.Features.Proposals.DTOs;

namespace SmartCourt.Features.Proposals.GetProposal;

public sealed record GetProposalQuery(Guid ProposalId)
    : IRequest<ApiResponse<ProposalDetailDto>>;
