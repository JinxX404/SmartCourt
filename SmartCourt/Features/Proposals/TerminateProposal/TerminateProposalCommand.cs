using MediatR;
using SmartCourt.Common.Models;
using SmartCourt.Features.Proposals.DTOs;

namespace SmartCourt.Features.Proposals.TerminateProposal;

public sealed record TerminateProposalCommand(Guid ProposalId, string Reason)
    : IRequest<ApiResponse<ProposalDetailDto>>;
