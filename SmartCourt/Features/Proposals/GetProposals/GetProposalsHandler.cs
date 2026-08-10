using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartCourt.Common.Models;
using SmartCourt.Features.Proposals.DTOs;
using SmartCourt.Features.Proposals.Enums;
using SmartCourt.Features.Proposals.Shared;
using SmartCourt.Interfaces;
using SmartCourt.Persistence;

namespace SmartCourt.Features.Proposals.GetProposals;

public sealed class GetProposalsHandler(
    ApplicationDbContext context,
    ICurrentUserService currentUserService,
    IValidator<GetProposalsQuery> validator)
    : IRequestHandler<GetProposalsQuery, ApiResponse<ProposalPageDto>>
{
    public async Task<ApiResponse<ProposalPageDto>> Handle(
        GetProposalsQuery request,
        CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return ApiResponse<ProposalPageDto>.Fail(
                validationResult.Errors.Select(error => error.ErrorMessage).ToList());
        }

        var actorUserId = ProposalAccess.GetRequiredUserId(currentUserService);
        var isClient = await ProposalAccess.HasRoleAsync(
            context,
            actorUserId,
            "Client",
            cancellationToken);
        var isLawyer = await ProposalAccess.HasRoleAsync(
            context,
            actorUserId,
            "Lawyer",
            cancellationToken);

        if (!isClient && !isLawyer)
        {
            return ApiResponse<ProposalPageDto>.Fail(
                "The proposal inbox is not available for this account.",
                403);
        }

        var query =
            from proposal in context.Proposals.AsNoTracking()
            join legalCase in context.Cases
                on proposal.LegalCaseId equals legalCase.Id
            join client in context.Users
                on proposal.ClientUserId equals client.Id
            join lawyer in context.Users
                on proposal.LawyerUserId equals lawyer.Id
            join conversation in context.ChatConversations
                on proposal.Id equals conversation.ProposalId into conversationJoin
            from conversation in conversationJoin.DefaultIfEmpty()
            select new { proposal, legalCase, client, lawyer, conversation };

        query = isClient
            ? query.Where(item => item.proposal.ClientUserId == actorUserId)
            : query.Where(item => item.proposal.LawyerUserId == actorUserId);

        if (request.Status.HasValue)
        {
            query = query.Where(item => item.proposal.Status == request.Status.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim();
            query = query.Where(item =>
                item.legalCase.Title.Contains(search)
                || item.client.FullName.Contains(search)
                || item.lawyer.FullName.Contains(search));
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var rows = await query
            .OrderByDescending(item => item.proposal.CreatedAt)
            .ThenBy(item => item.proposal.Id)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(item => new
            {
                item.proposal.Id,
                item.proposal.LegalCaseId,
                CaseTitle = item.legalCase.Title,
                item.proposal.ClientUserId,
                ClientName = item.client.FullName,
                item.proposal.LawyerUserId,
                LawyerName = item.lawyer.FullName,
                item.proposal.Status,
                item.proposal.CreatedAt,
                item.proposal.RespondedAt,
                item.proposal.ExpiresAt,
                item.proposal.ClosedAt,
                item.proposal.ClosedByUserId,
                ConversationId = item.conversation == null
                    ? null
                    : (Guid?)item.conversation.Id
            })
            .ToListAsync(cancellationToken);

        var items = rows.Select(item => new ProposalListItemDto(
            item.Id,
            item.LegalCaseId,
            item.CaseTitle,
            item.ClientUserId,
            item.ClientName,
            item.LawyerUserId,
            item.LawyerName,
            item.Status.ToString(),
            item.CreatedAt,
            item.RespondedAt,
            item.ConversationId,
            item.ExpiresAt,
            item.ClosedAt,
            item.ClosedByUserId)).ToList();

        var page = new ProposalPageDto(
            items,
            request.Page,
            request.PageSize,
            totalCount,
            request.Page * request.PageSize < totalCount);
        return ApiResponse<ProposalPageDto>.Ok(page);
    }
}
