using MediatR;
using SmartCourt.Common.Models;
using SmartCourt.Features.Proposals.DTOs;

namespace SmartCourt.Features.Proposals.GetProposalAvailability;

public sealed record GetProposalAvailabilityQuery(Guid LegalCaseId)
    : IRequest<ApiResponse<ProposalSlotAvailabilityDto>>;
