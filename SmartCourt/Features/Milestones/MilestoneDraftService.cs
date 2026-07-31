using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using SmartCourt.Common.Domain;
using SmartCourt.Common.Exceptions;
using SmartCourt.Features.Contracts;
using SmartCourt.Features.Contracts.DTOs;
using SmartCourt.Features.Contracts.Enums;
using SmartCourt.Features.Milestones.Domain;
using SmartCourt.Features.Milestones.DTOs;
using SmartCourt.Features.Milestones.Entities;
using SmartCourt.Features.Milestones.Enums;
using SmartCourt.Features.Payments.Entities;
using SmartCourt.Features.Payments.Enums;
using SmartCourt.Interfaces;
using SmartCourt.Persistence;

namespace SmartCourt.Features.Milestones;

public sealed class MilestoneDraftService(
    ApplicationDbContext dbContext,
    ICurrentUserService currentUserService,
    IContractService contractService,
    TimeProvider timeProvider) : IMilestoneDraftService
{
    public async Task<MilestoneDto> AddAsync(
        Guid contractId,
        AddMilestoneRequest request,
        CancellationToken cancellationToken)
    {
        var actorUserId = GetActorUserId();
        var contract = await GetContractAsync(
            contractId,
            cancellationToken);
        EnsureParticipant(contract, actorUserId);
        EnsureNegotiationAllowed(contract);

        var expectedOrder = await dbContext.Milestones
            .Where(milestone => milestone.ContractId == contractId)
            .Select(milestone => (int?)milestone.OrderNumber)
            .MaxAsync(cancellationToken) + 1 ?? 1;
        if (request.OrderNumber != expectedOrder)
        {
            throw new BusinessException(
                $"ترتيب المرحلة الجديدة يجب أن يكون {expectedOrder}.");
        }

        var now = UtcNow;
        var milestone = new Milestone(
            Guid.NewGuid(),
            contractId,
            request.Title,
            request.Description,
            request.OrderNumber,
            request.Amount,
            request.DurationDays,
            request.DueDate,
            now);
        dbContext.Milestones.Add(milestone);
        try
        {
            await SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception)
            when (IsDuplicateOrderConstraintViolation(exception))
        {
            throw new ConflictException(
                "يوجد بالفعل مرحلة أخرى بنفس الترتيب داخل العقد.");
        }

        return MapMilestone(
            milestone,
            hold: null,
            contract,
            isCurrentSequentialMilestone: expectedOrder == 1,
            actorUserId);
    }

    public async Task<IReadOnlyList<MilestoneDto>> ListAsync(
        Guid contractId,
        CancellationToken cancellationToken)
    {
        var actorUserId = GetActorUserId();
        var contract = await GetContractAsync(
            contractId,
            cancellationToken);
        var milestones = await dbContext.Milestones
            .AsNoTracking()
            .Where(milestone => milestone.ContractId == contractId)
            .OrderBy(milestone => milestone.OrderNumber)
            .ToListAsync(cancellationToken);
        var milestoneIds = milestones
            .Select(milestone => milestone.Id)
            .ToArray();
        var holds = await dbContext.EscrowHolds
            .AsNoTracking()
            .Where(hold => milestoneIds.Contains(hold.MilestoneId))
            .ToDictionaryAsync(
                hold => hold.MilestoneId,
                cancellationToken);
        var currentSequentialId = milestones
            .Where(milestone => !IsTerminal(milestone.Status))
            .Select(milestone => (Guid?)milestone.Id)
            .FirstOrDefault();

        return milestones
            .Select(milestone =>
            {
                holds.TryGetValue(milestone.Id, out var hold);
                return MapMilestone(
                    milestone,
                    hold,
                    contract,
                    milestone.Id == currentSequentialId,
                    actorUserId);
            })
            .ToArray();
    }

    public async Task<MilestoneDto> UpdateDraftAsync(
        Guid contractId,
        Guid milestoneId,
        UpdateMilestoneRequest request,
        string ifMatch,
        CancellationToken cancellationToken)
    {
        var actorUserId = GetActorUserId();
        var contract = await GetContractAsync(
            contractId,
            cancellationToken);
        EnsureParticipant(contract, actorUserId);
        EnsureNegotiationAllowed(contract);
        var milestone = await GetMilestoneForMutationAsync(
            milestoneId,
            cancellationToken);
        EnsureBelongsToContract(milestone, contractId);
        EnsureDraft(milestone);
        EnsureExpectedVersion(milestone, ifMatch);

        milestone.Title = request.Title;
        milestone.Description = request.Description;
        milestone.DurationDays = request.DurationDays;
        milestone.DueDate = request.DueDate;
        milestone.AcceptedByClientAt = null;
        milestone.AcceptedByLawyerAt = null;
        milestone.UpdatedAt = UtcNow;
        await SaveChangesAsync(cancellationToken);

        return MapMilestone(
            milestone,
            hold: null,
            contract,
            await IsCurrentSequentialMilestoneAsync(
                milestone,
                cancellationToken),
            actorUserId);
    }

    private Guid GetActorUserId() => currentUserService.RequireUserId(
        "يجب تسجيل الدخول للوصول إلى خدمات المراحل.");

    private async Task<ContractDetailDto> GetContractAsync(
        Guid contractId,
        CancellationToken cancellationToken)
    {
        return await contractService.GetAsync(
            contractId,
            cancellationToken);
    }

    private async Task<Milestone> GetMilestoneForMutationAsync(
        Guid milestoneId,
        CancellationToken cancellationToken)
    {
        if (milestoneId == Guid.Empty)
        {
            throw new BusinessException(
                "معرّف المرحلة مطلوب لترفيذ هذه العملية.");
        }

        return await dbContext.Milestones
            .SingleOrDefaultAsync(
                milestone => milestone.Id == milestoneId,
                cancellationToken)
            ?? throw new NotFoundException(
                "المرحلة المطلوبة غير موجودة.");
    }

    private static void EnsureParticipant(
        ContractDetailDto contract,
        Guid actorUserId)
    {
        if (contract.ClientUserId != actorUserId
            && contract.LawyerUserId != actorUserId)
        {
            throw new ForbiddenAccessException(
                "هذا الإجراء متاح لطرفي العقد فقط.");
        }
    }

    private static void EnsureNegotiationAllowed(
        ContractDetailDto contract)
    {
        if (contract.Status != ContractStatus.Draft)
        {
            throw new ConflictException(
                "يمكن تعديل المراحل أو التفاوض عليها أثناء مرحلة المسودة فقط.");
        }
    }

    private static void EnsureBelongsToContract(
        Milestone milestone,
        Guid contractId)
    {
        if (milestone.ContractId != contractId)
        {
            throw new BusinessException(
                "المرحلة لا تنتمي إلى العقد المحدد.");
        }
    }

    private static void EnsureDraft(Milestone milestone)
    {
        if (milestone.Status != MilestoneStatus.Draft)
        {
            throw new BusinessException(
                "يمكن تنفيذ هذا الإجراء على مراحل المسودة فقط.");
        }
    }

    private void EnsureExpectedVersion(
        Milestone milestone,
        string ifMatch)
    {
        var expectedVersion = ConcurrencyGuard.ParseIfMatch(
            ifMatch,
            "قيمة If-Match الخاصة بالمرحلة غير صالحة.");
        ConcurrencyGuard.EnsureExpectedVersion(
            milestone.RowVersion,
            expectedVersion,
            "تم تعديل المرحلة بواسطة عملية أخرى. يرجى إعادة تحميلها والمحاولة مرة أخرى.");

        dbContext.Entry(milestone)
            .Property(item => item.RowVersion)
            .OriginalValue = expectedVersion;
    }

    private async Task<bool> IsCurrentSequentialMilestoneAsync(
        Milestone milestone,
        CancellationToken cancellationToken)
    {
        var currentSequentialId = await dbContext.Milestones
            .AsNoTracking()
            .Where(item => item.ContractId == milestone.ContractId
                && item.Status != MilestoneStatus.Cancelled
                && item.Status != MilestoneStatus.Refunded
                && item.Status != MilestoneStatus.Released)
            .OrderBy(item => item.OrderNumber)
            .Select(item => (Guid?)item.Id)
            .FirstOrDefaultAsync(cancellationToken);
        return currentSequentialId == milestone.Id;
    }

    private static MilestoneDto MapMilestone(
        Milestone milestone,
        EscrowHold? hold,
        ContractDetailDto contract,
        bool isCurrentSequentialMilestone,
        Guid actorUserId)
    {
        return new MilestoneDto(
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
            hold?.NetAmount)
        {
            PermittedActions = GetPermittedActions(
                milestone,
                contract,
                isCurrentSequentialMilestone,
                actorUserId)
        };
    }

    private static IReadOnlyList<string> GetPermittedActions(
        Milestone milestone,
        ContractDetailDto contract,
        bool isCurrentSequentialMilestone,
        Guid actorUserId)
    {
        var isClient = actorUserId == contract.ClientUserId;
        var isLawyer = actorUserId == contract.LawyerUserId;
        var actions = new List<string>();
        if (milestone.Status == MilestoneStatus.Draft
            && (isClient || isLawyer))
        {
            actions.Add("Update");
            if (isClient && !milestone.AcceptedByClientAt.HasValue
                || isLawyer && !milestone.AcceptedByLawyerAt.HasValue)
            {
                actions.Add("Approve");
            }
        }

        if (milestone.Status == MilestoneStatus.AwaitingFunding
            && isLawyer
            && isCurrentSequentialMilestone
            && !milestone.ReadyForFundingAt.HasValue)
        {
            actions.Add("ReadyForFunding");
        }

        if (milestone.Status == MilestoneStatus.FundedInProgress
            && isLawyer)
        {
            actions.Add("Submit");
        }

        if (milestone.Status == MilestoneStatus.Submitted
            && isClient)
        {
            actions.Add("Accept");
            actions.Add("RequestChanges");
        }

        return actions;
    }

    private static bool IsTerminal(MilestoneStatus status)
    {
        return status is MilestoneStatus.Released
            or MilestoneStatus.Refunded
            or MilestoneStatus.Cancelled;
    }

    private static bool IsDuplicateOrderConstraintViolation(
        DbUpdateException exception)
    {
        return exception.InnerException is SqlException
            {
                Number: 2601 or 2627
            } sqlException
            && sqlException.Message.Contains(
                "IX_Milestones_ContractId_OrderNumber",
                StringComparison.OrdinalIgnoreCase);
    }

    private async Task SaveChangesAsync(
        CancellationToken cancellationToken)
    {
        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new ConflictException(
                "تم تعديل المرحلة بواسطة عملية أخرى. يرجى إعادة تحميلها والمحاولة مرة أخرى.");
        }
    }

    private DateTime UtcNow => timeProvider.GetUtcNow().UtcDateTime;
}
