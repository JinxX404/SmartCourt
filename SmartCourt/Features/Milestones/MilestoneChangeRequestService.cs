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
using SmartCourt.Infrastructure.Providers.Events;
using SmartCourt.Interfaces;
using SmartCourt.Persistence;

namespace SmartCourt.Features.Milestones;

public sealed class MilestoneChangeRequestService(
    ApplicationDbContext dbContext,
    ICurrentUserService currentUserService,
    IContractService contractService,
    IOutboxWriter outboxWriter,
    TimeProvider timeProvider) : IMilestoneChangeRequestService
{
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
            ContractPaymentEventTypes.MilestoneChangeRequestCreated,
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

    private async Task<MilestoneChangeRequest> GetChangeRequestForMutationAsync(
        Guid changeRequestId,
        CancellationToken cancellationToken)
    {
        if (changeRequestId == Guid.Empty)
        {
            throw new BusinessException(
                "معرّف طلب التعديل مطلوب لتنفيذ هذه العملية.");
        }

        return await dbContext.MilestoneChangeRequests
            .SingleOrDefaultAsync(
                request => request.Id == changeRequestId,
                cancellationToken)
            ?? throw new NotFoundException(
                "طلب التعديل المطلوب غير موجود.");
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

    private static void EnsureFundedWorkCanBeChanged(Milestone milestone)
    {
        if (milestone.Status != MilestoneStatus.FundedInProgress)
        {
            throw new BusinessException(
                "يمكن تقديم أو معالجة طلبات التعديل عندما تكون المرحلة مُمولة وقيد التنفيذ فقط.");
        }
    }

    private static void EnsurePending(MilestoneChangeRequest changeRequest)
    {
        if (changeRequest.Status != ChangeRequestStatus.Pending)
        {
            throw new BusinessException(
                "طلب التعديل لم يعد في حالة الانتظار.");
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
                "لا يمكن لمقدم طلب التعديل اتخاذ القرار عليه بنفسه.");
        }
    }

    private static void EnsureActualExtension(
        Milestone milestone,
        CreateMilestoneChangeRequest request)
    {
        var descriptionChanged = request.ProposedDescription is not null
            && !string.Equals(
                milestone.Description,
                request.ProposedDescription,
                StringComparison.Ordinal);
        var durationChanged = request.ProposedDurationDays.HasValue
            && request.ProposedDurationDays != milestone.DurationDays;
        var dueDateChanged = request.ProposedDueDate.HasValue
            && request.ProposedDueDate != milestone.DueDate;

        if (!descriptionChanged && !durationChanged && !dueDateChanged)
        {
            throw new BusinessException(
                "يجب تقديم تغييرًا فعليًا واحدًا على الأقل على المرحلة.");
        }

        if (request.ProposedDurationDays.HasValue
            && request.ProposedDurationDays <= milestone.DurationDays)
        {
            throw new BusinessException(
                "عدد أيام المدة المقترحة يجب أن يكون أطول من المدة الحالية.");
        }

        if (request.ProposedDueDate.HasValue
            && request.ProposedDueDate <= milestone.DueDate)
        {
            throw new BusinessException(
                "تاريخ الاستحقاق المقترح يجب أن يكون بعد التاريخ الحالي.");
        }
    }

    private static void EnsureExtensionStillMovesForward(
        Milestone milestone,
        MilestoneChangeRequest changeRequest)
    {
        if (changeRequest.ProposedDurationDays.HasValue
            && changeRequest.ProposedDurationDays <= milestone.DurationDays)
        {
            throw new BusinessException(
                "لم يعد التعديل يقدم تمديدًا زمنيًا للمرحلة.");
        }

        if (changeRequest.ProposedDueDate.HasValue
            && changeRequest.ProposedDueDate <= milestone.DueDate)
        {
            throw new BusinessException(
                "لم يعد تاريخ الاستحقاق المقترح يمثل تمديدًا للمرحلة.");
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

    private void EnsureExpectedVersion(
        MilestoneChangeRequest changeRequest,
        string ifMatch)
    {
        var expectedVersion = ConcurrencyGuard.ParseIfMatch(
            ifMatch,
            "قيمة If-Match الخاصة بالمرحلة غير صالحة.");
        ConcurrencyGuard.EnsureExpectedVersion(
            changeRequest.RowVersion,
            expectedVersion,
            "تم تعديل طلب التعديل بواسطة عملية أخرى. يرجى إعادة تحميله والمحاولة مرة أخرى.");

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

    private static bool IsDuplicatePendingRequestConstraintViolation(
        DbUpdateException exception)
    {
        return exception.InnerException is SqlException
            {
                Number: 2601 or 2627
            } sqlException
            && sqlException.Message.Contains(
                "IX_MilestoneChangeRequests_MilestoneId_Pending",
                StringComparison.OrdinalIgnoreCase);
    }

    private static MilestoneActionResultDto ToActionResult(
        MilestoneChangeRequest changeRequest,
        DateTime now)
        => new(
            changeRequest.Id,
            changeRequest.Status.ToString(),
            now);

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
                "تم تعديل طلب التعديل بواسطة عملية أخرى. يرجى إعادة تحميله والمحاولة مرة أخرى.");
        }
    }

    private DateTime UtcNow => timeProvider.GetUtcNow().UtcDateTime;
}
