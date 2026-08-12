using MediatR;
using SmartCourt.Common.Models;
using SmartCourt.Features.Proposals.DTOs;
using SmartCourt.Features.Proposals.Enums;

namespace SmartCourt.Features.Proposals.GetProposals;

public enum ProposalListScope : byte
{
    LawyerInbox = 1,
    ClientCase = 2
}

public sealed record GetProposalsQuery(
    ProposalListScope Scope,
    Guid? LegalCaseId = null,
    IReadOnlyCollection<ProposalStatus>? Statuses = null,
    string? Search = null,
    int Page = 1,
    int PageSize = 5) : IRequest<ApiResponse<ProposalPageDto>>;
