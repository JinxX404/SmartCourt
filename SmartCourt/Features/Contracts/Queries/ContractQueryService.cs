using Microsoft.EntityFrameworkCore;
using SmartCourt.Common.Domain;
using SmartCourt.Common.Exceptions;
using SmartCourt.Common.Models;
using SmartCourt.Features.Contracts.DTOs;
using SmartCourt.Features.Contracts.Entities;
using SmartCourt.Features.Contracts.Enums;
using SmartCourt.Features.Milestones.Domain;
using SmartCourt.Features.Milestones.Entities;
using SmartCourt.Features.Milestones.Enums;
using SmartCourt.Features.Payments.Entities;
using SmartCourt.Features.Users.Integration;
using SmartCourt.Interfaces;
using SmartCourt.Persistence;

namespace SmartCourt.Features.Contracts;

public sealed class ContractQueryService(
    ApplicationDbContext dbContext,
    ICurrentUserService currentUserService,
    IContractUserEligibilityService userEligibilityService) : IContractQueryService
{
    public async Task<PagedResult<ContractSummaryDto>> ListAsync(
        ContractListQuery query,
        CancellationToken cancellationToken)
    {
        var actorUserId = GetActorUserId();
        var eligibility =
            await userEligibilityService.FindEligibilityAsync(
                actorUserId,
                cancellationToken);
        var hasModeratorAccess = eligibility is not null
            && eligibility.UserId == actorUserId
            && (eligibility.CanActAsModerator
                || eligibility.CanActAsSuperAdministrator);

        var contracts = dbContext.Contracts.AsNoTracking();
        if (!hasModeratorAccess)
        {
            contracts = contracts.Where(contract =>
                contract.ClientUserId == actorUserId
                || contract.LawyerUserId == actorUserId);
        }

        if (query.Status.HasValue)
        {
            contracts = contracts.Where(
                contract => contract.Status == query.Status.Value);
        }

        var totalCount = await contracts.CountAsync(cancellationToken);
        var items = await contracts
            .OrderByDescending(contract => contract.UpdatedAt)
            .ThenBy(contract => contract.Id)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(contract => new ContractSummaryDto(
                contract.Id,
                contract.LegalCaseId,
                contract.ClientUserId,
                contract.LawyerUserId,
                contract.Title,
                contract.Currency,
                contract.Status,
                contract.ActivatedAt,
                contract.CompletedAt))
            .ToListAsync(cancellationToken);
        return new PagedResult<ContractSummaryDto>(
            items,
            query.Page,
            query.PageSize,
            totalCount,
            query.Page * query.PageSize < totalCount);
    }

    public async Task<ContractDetailDto> GetAsync(
        Guid contractId,
        CancellationToken cancellationToken)
    {
        var contract = await GetAuthorizedContractAsync(
            contractId,
            cancellationToken);
        return await MapDetailAsync(contract, cancellationToken);
    }

    public async Task<PagedResult<ContractStateHistoryDto>> GetStateHistoryAsync(
        Guid contractId,
        ContractStateHistoryQuery query,
        CancellationToken cancellationToken)
    {
        await GetAuthorizedContractAsync(
            contractId,
            cancellationToken);
        var histories = dbContext.ContractStateHistories
            .AsNoTracking()
            .Where(item => item.ContractId == contractId);
        var totalCount = await histories.CountAsync(cancellationToken);
        var items = await histories
            .OrderByDescending(item => item.CreatedAt)
            .ThenBy(item => item.Id)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(item => new ContractStateHistoryDto(
                item.Id,
                item.PreviousStatus,
                item.NewStatus,
                item.Trigger,
                item.ActorUserId,
                item.Reason,
                item.CreatedAt))
            .ToListAsync(cancellationToken);
        return new PagedResult<ContractStateHistoryDto>(
            items,
            query.Page,
            query.PageSize,
            totalCount,
            query.Page * query.PageSize < totalCount);
    }

    public async Task<ContractDetailDto> MapDetailAsync(
        Contract contract,
        CancellationToken cancellationToken)
    {
        var milestones = await dbContext.Milestones
            .AsNoTracking()
            .Where(milestone => milestone.ContractId == contract.Id)
            .OrderBy(milestone => milestone.OrderNumber)
            .ToListAsync(cancellationToken);
        var milestoneIds = milestones
            .Select(milestone => milestone.Id)
            .ToArray();
        var holds = await dbContext.EscrowHolds
            .AsNoTracking()
            .Where(hold => milestoneIds.Contains(hold.MilestoneId))
            .ToListAsync(cancellationToken);
        var holdByMilestone = holds.ToDictionary(
            hold => hold.MilestoneId);
        var milestoneDtos = milestones
            .Select(milestone =>
            {
                holdByMilestone.TryGetValue(milestone.Id, out var hold);
                return MapMilestone(milestone, hold);
            })
            .ToArray();
        var paymentDtos = holds
            .OrderBy(hold => milestones
                .Single(item => item.Id == hold.MilestoneId)
                .OrderNumber)
            .Select(hold => new ContractPaymentDto(
                hold.Id,
                hold.MilestoneId,
                hold.GrossAmount,
                hold.PlatformFeeAmount,
                hold.NetAmount,
                "EGP",
                hold.Status,
                hold.HoldExpiresAt,
                hold.SettledAt))
            .ToArray();
        var currentTotal = milestones
            .Where(milestone =>
                milestone.Amount > 0
                && milestone.AcceptedByClientAt.HasValue
                && milestone.AcceptedByLawyerAt.HasValue
                && milestone.Status != MilestoneStatus.Cancelled)
            .Sum(milestone => milestone.Amount);
        return new ContractDetailDto(
            contract.Id,
            contract.ProposalId,
            contract.LegalCaseId,
            contract.ClientUserId,
            contract.LawyerUserId,
            contract.Title,
            contract.TermsAndConditions,
            contract.Currency,
            contract.Status,
            contract.AcceptedByClientAt,
            contract.AcceptedByLawyerAt,
            contract.ActivatedAt,
            contract.CompletedAt,
            contract.TerminatedAt,
            currentTotal,
            $"\"{Convert.ToBase64String(contract.RowVersion)}\"",
            milestoneDtos,
            paymentDtos,
            GetPermittedActions(contract, GetActorUserId()));
    }

    private Guid GetActorUserId() => currentUserService.RequireUserId(
        "يجب تسجيل الدخول للوصول إلى خدمات العقود.");

    private async Task<Contract> GetAuthorizedContractAsync(
        Guid contractId,
        CancellationToken cancellationToken)
    {
        var actorUserId = GetActorUserId();
        var contract = await dbContext.Contracts
            .AsNoTracking()
            .SingleOrDefaultAsync(
                item => item.Id == contractId,
                cancellationToken)
            ?? throw new NotFoundException("العقد غير موجود.");
        if (contract.ClientUserId == actorUserId
            || contract.LawyerUserId == actorUserId)
        {
            return contract;
        }

        var eligibility =
            await userEligibilityService.FindEligibilityAsync(
                actorUserId,
                cancellationToken);
        if (eligibility is null
            || eligibility.UserId != actorUserId
            || (!eligibility.CanActAsModerator
                && !eligibility.CanActAsSuperAdministrator))
        {
            throw new ForbiddenAccessException(
                "غير مصرح لك بالاطلاع على هذا العقد.");
        }

        return contract;
    }

    private static ContractMilestoneDto MapMilestone(
        Milestone milestone,
        EscrowHold? hold)
    {
        return new ContractMilestoneDto(
            milestone.Id,
            milestone.OrderNumber,
            milestone.Title,
            milestone.Description,
            milestone.Amount,
            milestone.DurationDays,
            milestone.DueDate,
            milestone.Status,
            MilestoneFundingStatusResolver.Resolve(milestone.Status, hold),
            hold?.Id,
            milestone.FundedAt,
            milestone.SubmittedAt,
            milestone.AutoAcceptEligibleAt,
            milestone.HoldExpiresAt,
            hold?.NetAmount,
            "\"" + Convert.ToBase64String(milestone.RowVersion) + "\"",
            milestone.Type,
            milestone.Deliverables);
    }

    private static IReadOnlyList<string> GetPermittedActions(
        Contract contract,
        Guid actorUserId)
    {
        var isClient = actorUserId == contract.ClientUserId;
        var isLawyer = actorUserId == contract.LawyerUserId;
        var actions = new List<string>();
        if (contract.Status == ContractStatus.Draft
            && (isClient || isLawyer))
        {
            actions.Add("Update");
            if (isClient && !contract.AcceptedByClientAt.HasValue
                || isLawyer && !contract.AcceptedByLawyerAt.HasValue)
            {
                actions.Add("Accept");
            }
        }

        if (contract.Status is ContractStatus.Draft or ContractStatus.Active
            && (isClient || isLawyer))
        {
            actions.Add("Terminate");
        }

        return actions;
    }
}
