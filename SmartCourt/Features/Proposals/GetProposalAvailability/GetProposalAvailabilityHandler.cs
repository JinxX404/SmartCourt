using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartCourt.Common.Enums;
using SmartCourt.Common.Models;
using SmartCourt.Features.Proposals.DTOs;
using SmartCourt.Features.Proposals.Enums;
using SmartCourt.Features.Proposals.Expiration;
using SmartCourt.Features.Proposals.Shared;
using SmartCourt.Interfaces;
using SmartCourt.Persistence;

namespace SmartCourt.Features.Proposals.GetProposalAvailability;

public sealed class GetProposalAvailabilityHandler(
    ApplicationDbContext context,
    ICurrentUserService currentUserService,
    IProposalExpirationService expirationService)
    : IRequestHandler<
        GetProposalAvailabilityQuery,
        ApiResponse<ProposalSlotAvailabilityDto>>
{
    public async Task<ApiResponse<ProposalSlotAvailabilityDto>> Handle(
        GetProposalAvailabilityQuery request,
        CancellationToken cancellationToken)
    {
        if (request.LegalCaseId == Guid.Empty)
        {
            return ApiResponse<ProposalSlotAvailabilityDto>.Fail(
                "Case ID is required.");
        }

        var clientUserId = ProposalAccess.GetRequiredUserId(currentUserService);
        var caseStatus = await context.Cases
            .Where(legalCase => legalCase.Id == request.LegalCaseId
                && legalCase.ClientId == clientUserId)
            .Select(legalCase => (CaseStatus?)legalCase.Status)
            .SingleOrDefaultAsync(cancellationToken);
        if (!caseStatus.HasValue)
        {
            return ApiResponse<ProposalSlotAvailabilityDto>.Fail(
                "Case was not found.",
                404);
        }

        await expirationService.ExpireDueForCaseAsync(
            request.LegalCaseId,
            cancellationToken);
        var activeCount = await context.Proposals.CountAsync(
            proposal => proposal.LegalCaseId == request.LegalCaseId
                && (proposal.Status == ProposalStatus.Pending
                    || proposal.Status == ProposalStatus.Accepted),
            cancellationToken);
        var availableSlots = Math.Max(
            0,
            ProposalPolicy.ActiveProposalLimitPerCase - activeCount);
        var result = new ProposalSlotAvailabilityDto(
            request.LegalCaseId,
            activeCount,
            ProposalPolicy.ActiveProposalLimitPerCase,
            availableSlots,
            caseStatus == CaseStatus.Matched && availableSlots > 0);
        return ApiResponse<ProposalSlotAvailabilityDto>.Ok(result);
    }
}
