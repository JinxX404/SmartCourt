using MediatR;
using SmartCourt.Common.Models;
using SmartCourt.Features.Proposals.DTOs;

namespace SmartCourt.Features.Proposals.AcceptProposal;

public sealed record AcceptProposalCommand(Guid ProposalId)
    : IRequest<ApiResponse<ProposalDetailDto>>;
