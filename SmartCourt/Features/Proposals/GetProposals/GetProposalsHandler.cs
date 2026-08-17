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
        var requiredRole = request.Scope == ProposalListScope.LawyerInbox
            ? "Lawyer"
            : "Client";
        if (!await ProposalAccess.HasRoleAsync(
                context,
                actorUserId,
                requiredRole,
                cancellationToken))
        {
            return ApiResponse<ProposalPageDto>.Fail(
                "The proposal inbox is not available for this account.",
                403);
        }

        if (request.Scope == ProposalListScope.ClientCase
            && !await context.Cases.AnyAsync(
                legalCase => legalCase.Id == request.LegalCaseId
                    && legalCase.ClientId == actorUserId,
                cancellationToken))
        {
            return ApiResponse<ProposalPageDto>.Fail(
                "Case was not found.",
                404);
        }

        var statuses = request.Statuses is { Count: > 0 }
            ? request.Statuses.Distinct().ToArray()
            : request.Scope == ProposalListScope.LawyerInbox
                ? [ProposalStatus.Pending]
                : [ProposalStatus.Pending, ProposalStatus.Accepted];

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
            join contract in context.Contracts
                on proposal.Id equals contract.ProposalId into contractJoin
            from contract in contractJoin.DefaultIfEmpty()
            select new
            {
                proposal,
                legalCase,
                client,
                lawyer,
                conversation,
                contract
            };

        query = request.Scope == ProposalListScope.LawyerInbox
            ? query.Where(item => item.proposal.LawyerUserId == actorUserId)
            : query.Where(item => item.proposal.ClientUserId == actorUserId
                && item.proposal.LegalCaseId == request.LegalCaseId);
        query = query.Where(item => statuses.Contains(item.proposal.Status));

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
                CaseStatus = item.legalCase.Status,
                AssignedLawyerUserId = item.legalCase.LawyerId,
                ContractId = item.contract == null
                    ? null
                    : (Guid?)item.contract.Id,
                ContractStatus = item.contract == null
                    ? null
                    : (SmartCourt.Features.Contracts.Enums.ContractStatus?)item.contract.Status,
                ConversationId = item.conversation == null
                    ? null
                    : (Guid?)item.conversation.Id,
                ConversationIsClosed = item.conversation != null
                    && item.conversation.IsClosed,
                item.proposal.CreatedAt,
                item.proposal.RespondedAt,
                item.proposal.ExpiresAt,
                item.proposal.ClosedAt,
                item.proposal.ClosedByUserId
            })
            .ToListAsync(cancellationToken);

        var items = rows.Select(item =>
        {
            var hideConversation = ProposalChatVisibility.IsHiddenFromActor(
                actorUserId,
                item.LawyerUserId,
                item.Status);
            var visibleConversationId = hideConversation
                ? null
                : item.ConversationId;
            var canChat = item.Status == ProposalStatus.Accepted
                && visibleConversationId.HasValue
                && !item.ConversationIsClosed;
            return new ProposalListItemDto(
                item.Id,
                item.LegalCaseId,
                item.CaseTitle,
                item.ClientUserId,
                item.ClientName,
                item.LawyerUserId,
                item.LawyerName,
                item.Status.ToString(),
                item.CaseStatus.ToString(),
                item.AssignedLawyerUserId,
                item.AssignedLawyerUserId == item.LawyerUserId,
                item.ContractId,
                item.ContractStatus?.ToString(),
                visibleConversationId,
                visibleConversationId.HasValue
                    ? item.ConversationIsClosed ? "Closed" : "Open"
                    : null,
                canChat,
                ProposalPermittedActions.Resolve(
                    actorUserId,
                    item.ClientUserId,
                    item.LawyerUserId,
                    item.Status,
                    item.ContractId,
                    item.ConversationId,
                    item.ConversationIsClosed),
                item.CreatedAt,
                item.RespondedAt,
                item.ExpiresAt,
                item.ClosedAt,
                item.ClosedByUserId);
        }).ToList();

        var page = new ProposalPageDto(
            items,
            request.Page,
            request.PageSize,
            totalCount,
            request.Page * request.PageSize < totalCount);
        return ApiResponse<ProposalPageDto>.Ok(page);
    }
}
