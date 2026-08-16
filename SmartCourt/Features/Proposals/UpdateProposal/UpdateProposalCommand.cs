using MediatR;
using SmartCourt.Common.Models;
using SmartCourt.Features.Proposals.DTOs;

namespace SmartCourt.Features.Proposals.UpdateProposal;

public sealed record UpdateProposalCommand(Guid ProposalId, string Message)
    : IRequest<ApiResponse<ProposalDetailDto>>;
