using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartCourt.Common.Enums;
using SmartCourt.Common.Models;
using SmartCourt.Features.Auth.Enums;
using SmartCourt.Features.Cases.Enums;
using SmartCourt.Features.Proposals.DTOs;
using SmartCourt.Features.Proposals.Entities;
using SmartCourt.Features.Proposals.Enums;
using SmartCourt.Features.Proposals.Shared;
using SmartCourt.Infrastructure.Providers.Events;
using SmartCourt.Interfaces;
using SmartCourt.Persistence;

namespace SmartCourt.Features.Proposals.CreateProposal;

public sealed class CreateProposalHandler(
    ApplicationDbContext context,
    ICurrentUserService currentUserService,
    TimeProvider timeProvider,
    IOutboxWriter outboxWriter,
    IValidator<CreateProposalCommand> validator)
    : IRequestHandler<CreateProposalCommand, ApiResponse<ProposalDetailDto>>
{
    private static readonly CaseStatus[] ProposalEligibleCaseStatuses =
    [
        CaseStatus.Submitted,
        CaseStatus.Analyzed,
        CaseStatus.Finalized
    ];

    public async Task<ApiResponse<ProposalDetailDto>> Handle(
        CreateProposalCommand request,
        CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return ApiResponse<ProposalDetailDto>.Fail(
                validationResult.Errors.Select(error => error.ErrorMessage).ToList());
        }

        var clientUserId = ProposalAccess.GetRequiredUserId(currentUserService);
        if (!await ProposalAccess.HasRoleAsync(
                context,
                clientUserId,
                "Client",
                cancellationToken))
        {
            return ApiResponse<ProposalDetailDto>.Fail("Only clients can send proposals.", 403);
        }

        var legalCase = await context.LegalCases
            .AsNoTracking()
            .SingleOrDefaultAsync(
                item => item.Id == request.LegalCaseId
                    && item.ClientUserId == clientUserId,
                cancellationToken);
        if (legalCase is null)
        {
            return ApiResponse<ProposalDetailDto>.Fail("Case was not found.", 404);
        }

        if (!ProposalEligibleCaseStatuses.Contains(legalCase.Status))
        {
            return ApiResponse<ProposalDetailDto>.Fail(
                "Proposals can only be sent after the case has been submitted.",
                409);
        }

        var lawyerExists = await (
                from user in context.Users
                where user.Id == request.LawyerUserId
                    && user.Status == UserStatus.Active
                    && context.UserRoles.Any(userRole =>
                        userRole.UserId == user.Id
                        && context.Roles.Any(role =>
                            role.Id == userRole.RoleId && role.Name == "Lawyer"))
                select user.Id)
            .AnyAsync(cancellationToken);
        if (!lawyerExists)
        {
            return ApiResponse<ProposalDetailDto>.Fail(
                "The selected lawyer is not eligible to receive proposals.",
                409);
        }

        var hasActiveProposal = await context.Proposals.AnyAsync(
            proposal => proposal.LegalCaseId == request.LegalCaseId
                && proposal.LawyerUserId == request.LawyerUserId
                && (proposal.Status == ProposalStatus.Pending
                    || proposal.Status == ProposalStatus.Accepted),
            cancellationToken);
        if (hasActiveProposal)
        {
            return ApiResponse<ProposalDetailDto>.Fail(
                "An active proposal already exists for this case and lawyer.",
                409);
        }

        var now = timeProvider.GetUtcNow().UtcDateTime;
        var proposal = new Proposal(
            Guid.NewGuid(),
            legalCase.Id,
            clientUserId,
            request.LawyerUserId,
            request.Message,
            now);
        context.Proposals.Add(proposal);

        await outboxWriter.EnqueueAsync(
            new OutboxEvent(
                ContractPaymentEventTypes.ProposalCreated,
                1,
                new ProposalEventPayload(
                    proposal.Id,
                    proposal.LegalCaseId,
                    proposal.ClientUserId,
                    proposal.LawyerUserId),
                nameof(Proposal),
                proposal.Id,
                Guid.NewGuid()),
            cancellationToken);
        await context.SaveChangesAsync(cancellationToken);

        var detail = await ProposalReadModel.FindDetailAsync(
            context,
            proposal.Id,
            clientUserId,
            cancellationToken);
        return ApiResponse<ProposalDetailDto>.Created(detail!);
    }
}
