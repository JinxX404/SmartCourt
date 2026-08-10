using MediatR;
using SmartCourt.Common.Models;
using SmartCourt.Features.Proposals.DTOs;
using SmartCourt.Features.Proposals.Enums;

namespace SmartCourt.Features.Proposals.GetProposals;

public sealed record GetProposalsQuery(
    ProposalStatus? Status = null,
    string? Search = null,
    int Page = 1,
    int PageSize = 10) : IRequest<ApiResponse<ProposalPageDto>>;
