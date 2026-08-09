using MediatR;
using SmartCourt.Common.Models;
using SmartCourt.Features.Proposals.DTOs;

namespace SmartCourt.Features.Proposals.CancelProposal;

public sealed record CancelProposalCommand(Guid ProposalId, string Reason)
    : IRequest<ApiResponse<ProposalDetailDto>>;
