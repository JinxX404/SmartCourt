using System.Security.Cryptography;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using SmartCourt.Common.Exceptions;
using SmartCourt.Features.Contracts;
using SmartCourt.Features.Contracts.DTOs;
using SmartCourt.Features.Contracts.Enums;
using SmartCourt.Features.Files.Integration;
using SmartCourt.Features.Milestones.Domain;
using SmartCourt.Features.Milestones.DTOs;
using SmartCourt.Features.Milestones.Entities;
using SmartCourt.Features.Milestones.Enums;
using SmartCourt.Features.Payments.Entities;
using SmartCourt.Features.Payments.Enums;
using SmartCourt.Features.Payments.FundingVerification;
using SmartCourt.Infrastructure.Providers.Events;
using SmartCourt.Interfaces;
using SmartCourt.Persistence;

namespace SmartCourt.Features.Milestones;

public sealed class MilestoneService(
    ApplicationDbContext dbContext,
    ICurrentUserService currentUserService,
    IContractService contractService,
    IMilestoneFundingVerifier fundingVerifier,
    IContractFileAccessService fileAccessService,
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

    public async Task<MilestoneDto> SubmitAsync(
        Guid milestoneId,
        SubmitMilestoneRequest request,
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
                "محامي العقد فقط هو من يمكنه تسليم أعمال المرحلة.");
        }

        if (contract.Status != ContractStatus.Active)
        {
            throw new BusinessException(
                "يجب أن يكون العقد نشطًا قبل تسليم أعمال المرحلة.");
        }

        if (milestone.Status != MilestoneStatus.FundedInProgress)
        {
            throw new BusinessException(
                "يمكن تسليم أعمال المرحلة عندما تكون ممولة وقيد التنفيذ فقط.");
        }

        await using var transaction = dbContext.Database.IsRelational()
            ? await dbContext.Database.BeginTransactionAsync(
                cancellationToken)
            : null;
        var verifiedFunding = await fundingVerifier.VerifyAsync(
            milestone.Id,
            FundingVerificationOperation.Submission,
            cancellationToken);
        if (verifiedFunding.ContractId != milestone.ContractId
            || verifiedFunding.GrossAmount != milestone.Amount
            || !string.Equals(
                verifiedFunding.Currency,
                "EGP",
                StringComparison.Ordinal))
        {
            throw new BusinessException(
                "بيانات تمويل المرحلة لا تطابق العقد أو المبلغ أو العملة المطلوبة للتسليم.");
        }

        var authorizedFiles =
            await fileAccessService.AuthorizeForUseAsync(
                actorUserId,
                request.StoredFileIds,
                ContractFilePurpose.MilestoneSubmission,
                milestone.Id,
                cancellationToken);
        var authorizedFileIds = authorizedFiles
            .Where(file => file.OwnerUserId == actorUserId)
            .Select(file => file.StoredFileId)
            .ToHashSet();
        if (authorizedFileIds.Count != request.StoredFileIds.Count
            || request.StoredFileIds.Any(
                fileId => !authorizedFileIds.Contains(fileId)))
        {
            throw new ForbiddenAccessException(
                "تعذر التحقق من ملكية جميع ملفات تسليم المرحلة للمحامي الحالي.");
        }

        var latestVersion = await dbContext.MilestoneSubmissions
            .Where(submission =>
                submission.MilestoneId == milestone.Id)
            .Select(submission => (int?)submission.Version)
            .MaxAsync(cancellationToken) ?? 0;
        var nextVersion = latestVersion + 1;
        var now = UtcNow;
        var correlationId = Guid.NewGuid();
        var submission = new MilestoneSubmission(
            Guid.NewGuid(),
            milestone.Id,
            verifiedFunding.EscrowHoldId,
            actorUserId,
            nextVersion,
            request.Notes,
            now);
        dbContext.MilestoneSubmissions.Add(submission);
        dbContext.MilestoneSubmissionAttachments.AddRange(
            request.StoredFileIds.Select(fileId =>
                new MilestoneSubmissionAttachment(
                    Guid.NewGuid(),
                    submission.Id,
                    fileId,
                    now)));

        var previousStatus = milestone.Status;
        MilestoneTransitionGuard.EnsureCanTransition(
            previousStatus,
            MilestoneStatus.Submitted);
        milestone.Status = MilestoneStatus.Submitted;
        milestone.SubmittedAt = now;
        milestone.AutoAcceptEligibleAt = now.AddDays(7);
        milestone.AutoAcceptJobId = null;
        milestone.SubmissionVersion = nextVersion;
        milestone.UpdatedAt = now;
        dbContext.MilestoneStateHistories.Add(
            MilestoneStateHistoryFactory.Create(
                Guid.NewGuid(),
                milestone.Id,
                previousStatus,
                MilestoneStatus.Submitted,
                ContractPaymentEventTypes.MilestoneSubmitted,
                actorUserId,
                $"سلّم المحامي النسخة رقم {nextVersion} من أعمال المرحلة.",
                correlationId,
                now));
        await outboxWriter.EnqueueAsync(
            new OutboxEvent(
                ContractPaymentEventTypes.MilestoneSubmitted,
                1,
                new MilestoneSubmissionEventPayload(
                    milestone.Id,
                    verifiedFunding.EscrowHoldId,
                    nextVersion),
                "Milestone",
                milestone.Id,
                correlationId),
            cancellationToken);

        try
        {
            await SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception)
            when (IsDuplicateSubmissionVersionConstraintViolation(
                exception))
        {
            throw new ConflictException(
                "تم تسجيل تسليم آخر لهذه المرحلة بالتزامن. يرجى إعادة تحميل المرحلة والمحاولة مرة أخرى.");
        }

        if (transaction is not null)
        {
            await transaction.CommitAsync(cancellationToken);
        }

        var hold = await dbContext.EscrowHolds
            .AsNoTracking()
            .SingleAsync(
                item => item.Id == verifiedFunding.EscrowHoldId,
                cancellationToken);
        return MapMilestone(
            milestone,
            hold,
            contract,
            await IsCurrentSequentialMilestoneAsync(
                milestone,
                cancellationToken),
            actorUserId);
    }

    public async Task<MilestoneActionResultDto> CreateChangeRequestAsync(
        Guid milestoneId,
        CreateMilestoneChangeRequest request,
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
        EnsureFundedWorkCanBeChanged(milestone);
        EnsureExpectedVersion(milestone, ifMatch);
        EnsureActualExtension(milestone, request);

        var hasPendingRequest =
            await dbContext.MilestoneChangeRequests.AnyAsync(
                item =>
                    item.MilestoneId == milestone.Id
                    && item.Status == ChangeRequestStatus.Pending,
                cancellationToken);
        if (hasPendingRequest)
        {
            throw new ConflictException(
                "يوجد طلب تعديل معلق لهذه المرحلة بالفعل.");
        }

        var now = UtcNow;
        var correlationId = Guid.NewGuid();
        var changeRequest = new MilestoneChangeRequest(
            Guid.NewGuid(),
            milestone.Id,
            actorUserId,
            request.ProposedDescription,
            request.ProposedDurationDays,
            request.ProposedDueDate,
            request.Reason,
            now);
        dbContext.MilestoneChangeRequests.Add(changeRequest);
        await EnqueueChangeRequestEventAsync(
            ContractPaymentEventTypes.MilestoneChangesRequested,
            changeRequest,
            correlationId,
            cancellationToken);
        try
        {
            await SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception)
            when (IsDuplicatePendingRequestConstraintViolation(exception))
        {
            throw new ConflictException(
                "يوجد طلب تعديل معلق لهذه المرحلة بالفعل.");
        }

        return ToActionResult(changeRequest, now);
    }

    public async Task<MilestoneActionResultDto> ApproveChangeRequestAsync(
        Guid changeRequestId,
        string ifMatch,
        CancellationToken cancellationToken)
    {
        var actorUserId = GetActorUserId();
        var changeRequest = await GetChangeRequestForMutationAsync(
            changeRequestId,
            cancellationToken);
        var milestone = await GetMilestoneForMutationAsync(
            changeRequest.MilestoneId,
            cancellationToken);
        var contract = await GetContractAsync(
            milestone.ContractId,
            cancellationToken);
        EnsureDecisionActor(contract, changeRequest, actorUserId);
        EnsurePending(changeRequest);
        EnsureFundedWorkCanBeChanged(milestone);
        EnsureExpectedVersion(changeRequest, ifMatch);
        EnsureExtensionStillMovesForward(milestone, changeRequest);

        if (changeRequest.ProposedDescription is not null)
        {
            milestone.Description =
                changeRequest.ProposedDescription;
        }

        if (changeRequest.ProposedDurationDays.HasValue)
        {
            milestone.DurationDays =
                changeRequest.ProposedDurationDays;
        }

        if (changeRequest.ProposedDueDate.HasValue)
        {
            milestone.DueDate = changeRequest.ProposedDueDate;
        }

        var now = UtcNow;
        var correlationId = Guid.NewGuid();
        ChangeRequestTransitionGuard.EnsureCanTransition(
            changeRequest.Status,
            ChangeRequestStatus.Approved);
        changeRequest.Status = ChangeRequestStatus.Approved;
        changeRequest.DecidedByUserId = actorUserId;
        changeRequest.DecidedAt = now;
        changeRequest.DecisionReason =
            "وافق الطرف الآخر على طلب تعديل المرحلة.";
        milestone.UpdatedAt = now;
        await EnqueueChangeRequestEventAsync(
            ContractPaymentEventTypes.MilestoneChangeRequestApproved,
            changeRequest,
            correlationId,
            cancellationToken);
        await SaveChangesAsync(cancellationToken);

        return ToActionResult(changeRequest, now);
    }

    public async Task<MilestoneActionResultDto> RejectChangeRequestAsync(
        Guid changeRequestId,
        RejectChangeRequest request,
        string ifMatch,
        CancellationToken cancellationToken)
    {
        var actorUserId = GetActorUserId();
        var changeRequest = await GetChangeRequestForMutationAsync(
            changeRequestId,
            cancellationToken);
        var milestone = await GetMilestoneForMutationAsync(
            changeRequest.MilestoneId,
            cancellationToken);
        var contract = await GetContractAsync(
            milestone.ContractId,
            cancellationToken);
        EnsureDecisionActor(contract, changeRequest, actorUserId);
        EnsurePending(changeRequest);
        EnsureExpectedVersion(changeRequest, ifMatch);

        var now = UtcNow;
        var correlationId = Guid.NewGuid();
        ChangeRequestTransitionGuard.EnsureCanTransition(
            changeRequest.Status,
            ChangeRequestStatus.Rejected);
        changeRequest.Status = ChangeRequestStatus.Rejected;
        changeRequest.DecidedByUserId = actorUserId;
        changeRequest.DecidedAt = now;
        changeRequest.DecisionReason = request.Reason;
        await EnqueueChangeRequestEventAsync(
            ContractPaymentEventTypes.MilestoneChangeRequestRejected,
            changeRequest,
            correlationId,
            cancellationToken);
        await SaveChangesAsync(cancellationToken);

        return ToActionResult(changeRequest, now);
    }

    public async Task<MilestoneActionResultDto> CancelChangeRequestAsync(
        Guid changeRequestId,
        string ifMatch,
        CancellationToken cancellationToken)
    {
        var actorUserId = GetActorUserId();
        var changeRequest = await GetChangeRequestForMutationAsync(
            changeRequestId,
            cancellationToken);
        if (changeRequest.RequestedByUserId != actorUserId)
        {
            throw new ForbiddenAccessException(
                "مقدم طلب التعديل فقط هو من يمكنه إلغاء الطلب.");
        }

        EnsurePending(changeRequest);
        EnsureExpectedVersion(changeRequest, ifMatch);
        var now = UtcNow;
        var correlationId = Guid.NewGuid();
        ChangeRequestTransitionGuard.EnsureCanTransition(
            changeRequest.Status,
            ChangeRequestStatus.Cancelled);
        changeRequest.Status = ChangeRequestStatus.Cancelled;
        changeRequest.DecidedByUserId = actorUserId;
        changeRequest.DecidedAt = now;
        changeRequest.DecisionReason =
            "ألغى مقدم الطلب طلب تعديل المرحلة.";
        await EnqueueChangeRequestEventAsync(
            ContractPaymentEventTypes.MilestoneChangeRequestCancelled,
            changeRequest,
            correlationId,
            cancellationToken);
        await SaveChangesAsync(cancellationToken);

        return ToActionResult(changeRequest, now);
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

    private async Task<MilestoneChangeRequest>
        GetChangeRequestForMutationAsync(
            Guid changeRequestId,
            CancellationToken cancellationToken)
    {
        if (changeRequestId == Guid.Empty)
        {
            throw new BusinessException(
                "معرّف طلب تعديل المرحلة مطلوب.");
        }

        return await dbContext.MilestoneChangeRequests
                .SingleOrDefaultAsync(
                    request => request.Id == changeRequestId,
                    cancellationToken)
            ?? throw new NotFoundException(
                "طلب تعديل المرحلة غير موجود.");
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

        if (milestone.Status == MilestoneStatus.FundedInProgress
            && isLawyer)
        {
            actions.Add("Submit");
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

    private static void EnsureFundedWorkCanBeChanged(Milestone milestone)
    {
        if (milestone.Status != MilestoneStatus.FundedInProgress
            || !milestone.FundedAt.HasValue)
        {
            throw new BusinessException(
                "يمكن طلب تعديل المدة أو الوصف أثناء تنفيذ المرحلة الممولة فقط.");
        }
    }

    private static void EnsureActualExtension(
        Milestone milestone,
        CreateMilestoneChangeRequest request)
    {
        if (request.ProposedDurationDays.HasValue
            && milestone.DurationDays.HasValue
            && request.ProposedDurationDays.Value
                <= milestone.DurationDays.Value)
        {
            throw new BusinessException(
                "يجب أن تزيد مدة المرحلة المقترحة عن مدتها الحالية.");
        }

        if (request.ProposedDueDate.HasValue
            && milestone.DueDate.HasValue
            && request.ProposedDueDate.Value <= milestone.DueDate.Value)
        {
            throw new BusinessException(
                "يجب أن يكون الموعد النهائي المقترح بعد الموعد الحالي.");
        }

        var descriptionChanged = request.ProposedDescription is not null
            && !string.Equals(
                request.ProposedDescription,
                milestone.Description,
                StringComparison.Ordinal);
        var durationChanged = request.ProposedDurationDays.HasValue
            && request.ProposedDurationDays != milestone.DurationDays;
        var dueDateChanged = request.ProposedDueDate.HasValue
            && request.ProposedDueDate != milestone.DueDate;
        if (!descriptionChanged && !durationChanged && !dueDateChanged)
        {
            throw new BusinessException(
                "يجب أن يتضمن طلب التعديل تغييرًا فعليًا في وصف المرحلة أو مدتها أو موعدها النهائي.");
        }
    }

    private static void EnsureExtensionStillMovesForward(
        Milestone milestone,
        MilestoneChangeRequest changeRequest)
    {
        if (changeRequest.ProposedDurationDays.HasValue
            && milestone.DurationDays.HasValue
            && changeRequest.ProposedDurationDays.Value
                <= milestone.DurationDays.Value)
        {
            throw new BusinessException(
                "لم تعد مدة المرحلة المقترحة تمدد المدة الحالية، لذلك لا يمكن اعتماد الطلب.");
        }

        if (changeRequest.ProposedDueDate.HasValue
            && milestone.DueDate.HasValue
            && changeRequest.ProposedDueDate.Value <= milestone.DueDate.Value)
        {
            throw new BusinessException(
                "لم يعد الموعد النهائي المقترح لاحقًا للموعد الحالي، لذلك لا يمكن اعتماد الطلب.");
        }
    }

    private static void EnsureDecisionActor(
        ContractDetailDto contract,
        MilestoneChangeRequest changeRequest,
        Guid actorUserId)
    {
        EnsureParticipant(contract, actorUserId);
        if (changeRequest.RequestedByUserId == actorUserId)
        {
            throw new ForbiddenAccessException(
                "لا يمكن لمقدم طلب التعديل اعتماد الطلب أو رفضه.");
        }
    }

    private static void EnsurePending(MilestoneChangeRequest changeRequest)
    {
        if (changeRequest.Status != ChangeRequestStatus.Pending)
        {
            throw new ConflictException(
                "تم حسم طلب تعديل المرحلة مسبقًا ولا يمكن تغيير حالته.");
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

    private void EnsureExpectedVersion(
        MilestoneChangeRequest changeRequest,
        string ifMatch)
    {
        var expectedVersion = ParseIfMatch(ifMatch);
        if (changeRequest.RowVersion.Length == 0
            || expectedVersion.Length != changeRequest.RowVersion.Length
            || !CryptographicOperations.FixedTimeEquals(
                expectedVersion,
                changeRequest.RowVersion))
        {
            throw new ConflictException(
                "تم تعديل طلب التعديل بواسطة عملية أخرى. يرجى إعادة تحميله والمحاولة مرة أخرى.");
        }

        dbContext.Entry(changeRequest)
            .Property(item => item.RowVersion)
            .OriginalValue = expectedVersion;
    }

    private async Task EnqueueChangeRequestEventAsync(
        string eventType,
        MilestoneChangeRequest changeRequest,
        Guid correlationId,
        CancellationToken cancellationToken)
    {
        await outboxWriter.EnqueueAsync(
            new OutboxEvent(
                eventType,
                1,
                new MilestoneChangeRequestEventPayload(
                    changeRequest.MilestoneId,
                    changeRequest.Id,
                    changeRequest.Status.ToString()),
                "MilestoneChangeRequest",
                changeRequest.Id,
                correlationId),
            cancellationToken);
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

    private static bool IsDuplicatePendingRequestConstraintViolation(
        DbUpdateException exception)
    {
        return exception.InnerException is SqlException
            {
                Number: 2601 or 2627
            } sqlException
            && sqlException.Message.Contains(
                "UX_MilestoneChangeRequests_Pending",
                StringComparison.Ordinal);
    }

    private static bool IsDuplicateSubmissionVersionConstraintViolation(
        DbUpdateException exception)
    {
        return exception.InnerException is SqlException
            {
                Number: 2601 or 2627
            } sqlException
            && sqlException.Message.Contains(
                "UX_MilestoneSubmissions_MilestoneId_Version",
                StringComparison.Ordinal);
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

    private static MilestoneActionResultDto ToActionResult(
        MilestoneChangeRequest changeRequest,
        DateTime occurredAt)
    {
        return new MilestoneActionResultDto(
            changeRequest.Id,
            changeRequest.Status.ToString(),
            occurredAt);
    }

    private DateTime UtcNow =>
        timeProvider.GetUtcNow().UtcDateTime;
}
