using MediatR;
using SmartCourt.Common.Models;
using SmartCourt.Features.Proposals.DTOs;

namespace SmartCourt.Features.Proposals.RejectProposal;

public sealed record RejectProposalCommand(Guid ProposalId, string Reason)
    : IRequest<ApiResponse<ProposalDetailDto>>;
