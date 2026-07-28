using System.Security.Cryptography;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
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
using SmartCourt.Infrastructure.Providers.Events;
using SmartCourt.Interfaces;
using SmartCourt.Persistence;

namespace SmartCourt.Features.Milestones;

public sealed class MilestoneService(
    ApplicationDbContext dbContext,
    ICurrentUserService currentUserService,
    IContractService contractService,
    IOutboxWriter outboxWriter,
    TimeProvider timeProvider) : IMilestoneService
{
    private const string MilestoneApprovedTrigger = "MilestoneApproved";

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

    public async Task<MilestoneActionResultDto> ApproveAsync(
        Guid milestoneId,
        string ifMatch,
        CancellationToken cancellationToken)
    {
        var actorUserId = GetActorUserId();
        var milestone = await GetMilestoneForMutationAsync(
            milestoneId,
            cancellationToken);
        var contract = await GetContractAsync(
            milestone.ContractId,
            cancellationToken);
        EnsureParticipant(contract, actorUserId);
        EnsureNegotiationAllowed(contract);
        EnsureDraft(milestone);
        EnsureExpectedVersion(milestone, ifMatch);

        var now = UtcNow;
        if (actorUserId == contract.ClientUserId)
        {
            if (milestone.AcceptedByClientAt.HasValue)
            {
                throw new ConflictException(
                    "وافق العميل على النسخة الحالية من المرحلة مسبقًا.");
            }

            milestone.AcceptedByClientAt = now;
        }
        else
        {
            if (milestone.AcceptedByLawyerAt.HasValue)
            {
                throw new ConflictException(
                    "وافق المحامي على النسخة الحالية من المرحلة مسبقًا.");
            }

            milestone.AcceptedByLawyerAt = now;
        }

        var transitioned = milestone.AcceptedByClientAt.HasValue
            && milestone.AcceptedByLawyerAt.HasValue;
        if (transitioned)
        {
            var correlationId = Guid.NewGuid();
            var previousStatus = milestone.Status;
            MilestoneTransitionGuard.EnsureCanTransition(
                previousStatus,
                MilestoneStatus.AwaitingFunding);
            milestone.Status = MilestoneStatus.AwaitingFunding;
            dbContext.MilestoneStateHistories.Add(
                MilestoneStateHistoryFactory.Create(
                    Guid.NewGuid(),
                    milestone.Id,
                    previousStatus,
                    MilestoneStatus.AwaitingFunding,
                    MilestoneApprovedTrigger,
                    actorUserId,
                    "وافق طرفا العقد على شروط المرحلة.",
                    correlationId,
                    now));
        }

        milestone.UpdatedAt = now;
        await SaveChangesAsync(cancellationToken);

        if (transitioned)
        {
            await contractService.EvaluateActivationAsync(
                contract.Id,
                cancellationToken);
        }

        return ToActionResult(milestone, now);
    }

    public async Task<MilestoneActionResultDto> MarkReadyForFundingAsync(
        Guid milestoneId,
        string ifMatch,
        CancellationToken cancellationToken)
    {
        var actorUserId = GetActorUserId();
        var milestone = await GetMilestoneForMutationAsync(
            milestoneId,
            cancellationToken);
        var contract = await GetContractAsync(
            milestone.ContractId,
            cancellationToken);
        if (actorUserId != contract.LawyerUserId)
        {
            throw new ForbiddenAccessException(
                "محامي العقد فقط هو من يمكنه تجهيز المرحلة للتمويل.");
        }

        if (contract.Status != ContractStatus.Active)
        {
            throw new BusinessException(
                "يجب أن يكون العقد نشطًا قبل تجهيز المرحلة للتمويل.");
        }

        if (milestone.Status != MilestoneStatus.AwaitingFunding)
        {
            throw new BusinessException(
                "يمكن تجهيز المرحلة للتمويل بعد موافقة الطرفين عليها فقط.");
        }

        EnsureExpectedVersion(milestone, ifMatch);
        if (milestone.ReadyForFundingAt.HasValue)
        {
            throw new ConflictException(
                "تم تجهيز المرحلة الحالية للتمويل مسبقًا.");
        }

        if (!await IsCurrentSequentialMilestoneAsync(
                milestone,
                cancellationToken))
        {
            throw new BusinessException(
                "يجب تسوية المراحل السابقة قبل تجهيز هذه المرحلة للتمويل.");
        }

        var hasUnsettledHold = await dbContext.EscrowHolds.AnyAsync(
            hold =>
                hold.ContractId == contract.Id
                && hold.MilestoneId != milestone.Id
                && (hold.Status == EscrowHoldStatus.Funded
                    || hold.Status == EscrowHoldStatus.Frozen),
            cancellationToken);
        var hasProcessingMilestone = await dbContext.Milestones.AnyAsync(
            item =>
                item.ContractId == contract.Id
                && item.Id != milestone.Id
                && item.Status == MilestoneStatus.FundingProcessing,
            cancellationToken);
        if (hasUnsettledHold || hasProcessingMilestone)
        {
            throw new ConflictException(
                "لا يمكن تجهيز مرحلة جديدة قبل حسم التمويل أو التسوية الحالية.");
        }

        var now = UtcNow;
        var correlationId = Guid.NewGuid();
        milestone.ReadyForFundingAt = now;
        milestone.UpdatedAt = now;
        await outboxWriter.EnqueueAsync(
            new OutboxEvent(
                ContractPaymentEventTypes.MilestoneReadyForFunding,
                1,
                new ContractPaymentAggregateEventPayload(milestone.Id),
                "Milestone",
                milestone.Id,
                correlationId),
            cancellationToken);
        await SaveChangesAsync(cancellationToken);

        return ToActionResult(milestone, now);
    }

    private async Task<ContractDetailDto> GetContractAsync(
        Guid contractId,
        CancellationToken cancellationToken)
    {
        if (contractId == Guid.Empty)
        {
            throw new BusinessException("معرّف العقد مطلوب.");
        }

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
            throw new BusinessException("معرّف المرحلة مطلوب.");
        }

        return await dbContext.Milestones.SingleOrDefaultAsync(
                milestone => milestone.Id == milestoneId,
                cancellationToken)
            ?? throw new NotFoundException("المرحلة غير موجودة.");
    }

    private async Task<bool> IsCurrentSequentialMilestoneAsync(
        Milestone milestone,
        CancellationToken cancellationToken)
    {
        return !await dbContext.Milestones.AnyAsync(
            item =>
                item.ContractId == milestone.ContractId
                && item.OrderNumber < milestone.OrderNumber
                && item.Status != MilestoneStatus.Released
                && item.Status != MilestoneStatus.Refunded
                && item.Status != MilestoneStatus.Cancelled,
            cancellationToken);
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
            GetFundingStatus(milestone, hold),
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

    private static MilestoneFundingStatus GetFundingStatus(
        Milestone milestone,
        EscrowHold? hold)
    {
        return milestone.Status switch
        {
            MilestoneStatus.FundingProcessing =>
                MilestoneFundingStatus.Processing,
            MilestoneStatus.Released or MilestoneStatus.Refunded =>
                MilestoneFundingStatus.Settled,
            _ when hold?.Status is EscrowHoldStatus.Released
                or EscrowHoldStatus.Refunded =>
                MilestoneFundingStatus.Settled,
            _ when hold is not null =>
                MilestoneFundingStatus.Funded,
            _ => MilestoneFundingStatus.Unfunded
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

        return actions;
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
        if (contract.Status is not ContractStatus.Draft
            and not ContractStatus.Active)
        {
            throw new BusinessException(
                "لا يمكن التفاوض على مراحل عقد غير نشط أو غير موجود كمسودة.");
        }
    }

    private static void EnsureBelongsToContract(
        Milestone milestone,
        Guid contractId)
    {
        if (milestone.ContractId != contractId)
        {
            throw new NotFoundException(
                "المرحلة غير موجودة ضمن العقد المحدد.");
        }
    }

    private static void EnsureDraft(Milestone milestone)
    {
        if (milestone.Status != MilestoneStatus.Draft)
        {
            throw new BusinessException(
                "لا يمكن تعديل أو اعتماد شروط مرحلة خرجت من حالة المسودة.");
        }
    }

    private void EnsureExpectedVersion(
        Milestone milestone,
        string ifMatch)
    {
        var expectedVersion = ParseIfMatch(ifMatch);
        if (milestone.RowVersion.Length == 0
            || expectedVersion.Length != milestone.RowVersion.Length
            || !CryptographicOperations.FixedTimeEquals(
                expectedVersion,
                milestone.RowVersion))
        {
            throw new ConflictException(
                "تم تعديل المرحلة بواسطة عملية أخرى. يرجى إعادة تحميلها والمحاولة مرة أخرى.");
        }

        dbContext.Entry(milestone)
            .Property(item => item.RowVersion)
            .OriginalValue = expectedVersion;
    }

    private static byte[] ParseIfMatch(string ifMatch)
    {
        if (string.IsNullOrWhiteSpace(ifMatch)
            || ifMatch.Length < 3
            || ifMatch[0] != '"'
            || ifMatch[^1] != '"'
            || ifMatch.StartsWith("W/\"", StringComparison.Ordinal))
        {
            throw new BusinessException(
                "قيمة If-Match الخاصة بالمرحلة غير صالحة.");
        }

        try
        {
            var rowVersion = Convert.FromBase64String(ifMatch[1..^1]);
            return rowVersion.Length > 0
                ? rowVersion
                : throw new BusinessException(
                    "قيمة If-Match الخاصة بالمرحلة غير صالحة.");
        }
        catch (FormatException exception)
        {
            throw new BusinessException(
                "قيمة If-Match الخاصة بالمرحلة غير صالحة.",
                exception);
        }
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

    private static bool IsDuplicateOrderConstraintViolation(
        DbUpdateException exception)
    {
        return exception.InnerException is SqlException
            {
                Number: 2601 or 2627
            } sqlException
            && sqlException.Message.Contains(
                "UX_Milestones_ContractId_OrderNumber",
                StringComparison.Ordinal);
    }

    private Guid GetActorUserId()
    {
        if (!currentUserService.IsAuthenticated
            || !currentUserService.UserId.HasValue
            || currentUserService.UserId.Value == Guid.Empty)
        {
            throw new AuthenticationException(
                "يجب تسجيل الدخول لإتمام هذا الإجراء.");
        }

        return currentUserService.UserId.Value;
    }

    private static bool IsTerminal(MilestoneStatus status)
        => status is MilestoneStatus.Released
            or MilestoneStatus.Refunded
            or MilestoneStatus.Cancelled;

    private static MilestoneActionResultDto ToActionResult(
        Milestone milestone,
        DateTime occurredAt)
    {
        return new MilestoneActionResultDto(
            milestone.Id,
            milestone.Status.ToString(),
            occurredAt);
    }

    private DateTime UtcNow =>
        timeProvider.GetUtcNow().UtcDateTime;
}
