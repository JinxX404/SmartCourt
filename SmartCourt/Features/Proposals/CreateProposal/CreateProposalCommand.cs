using MediatR;
using SmartCourt.Common.Models;
using SmartCourt.Features.Proposals.DTOs;

namespace SmartCourt.Features.Proposals.CreateProposal;

public sealed record CreateProposalCommand(
    Guid LegalCaseId,
    Guid LawyerUserId,
    string Message) : IRequest<ApiResponse<ProposalDetailDto>>;
