using System.Data;
using Microsoft.EntityFrameworkCore;
using SmartCourt.Common.Exceptions;
using SmartCourt.Common.Models;
using SmartCourt.Features.Contracts.Domain;
using SmartCourt.Features.Contracts.DTOs;

using SmartCourt.Features.Contracts.Enums;
using SmartCourt.Features.Contracts;
using SmartCourt.Features.Disputes.Domain;
using SmartCourt.Features.Disputes.DTOs;
using SmartCourt.Features.Disputes.Entities;
using SmartCourt.Features.Disputes.Enums;
using SmartCourt.Features.Files.Integration;
using SmartCourt.Features.Milestones.Domain;
using SmartCourt.Features.Milestones.Enums;
using SmartCourt.Features.Payments.Domain;
using SmartCourt.Features.Payments.Entities;
using SmartCourt.Features.Payments.Enums;
using SmartCourt.Features.Payments.FundingVerification;
using SmartCourt.Features.Payments.Settlement;
using SmartCourt.Features.Users.Integration;
using SmartCourt.Infrastructure.Idempotency;
using SmartCourt.Infrastructure.Persistence;
using SmartCourt.Infrastructure.Persistence.Enums;
using SmartCourt.Infrastructure.Providers.Events;
using SmartCourt.Infrastructure.Providers.Jobs;
using SmartCourt.Infrastructure.Providers.Payments;
using SmartCourt.Interfaces;
using SmartCourt.Persistence;

namespace SmartCourt.Features.Disputes;

public sealed class DisputeService(
    ApplicationDbContext dbContext,
    ICurrentUserService currentUserService,
    IContractUserEligibilityService userEligibilityService,
    IContractFileAccessService fileAccessService,
    IMilestoneFundingVerifier fundingVerifier,
    IIdempotencyService idempotencyService,
    IPaymentProvider paymentProvider,
    IContractJobScheduler jobScheduler,
    IContractCompletionEvaluator completionEvaluator,
    IOutboxWriter outboxWriter,
    TimeProvider timeProvider,
    ILogger<DisputeService> logger)
    : IDisputeService, IDisputeSettlementRecoveryService
{
    private const string ResolveOperation = "ResolveDispute";
    private const int RecoveryBatchSize = 100;

    public async Task<DisputeDto> CreateAsync(
        CreateDisputeRequest request,
        CancellationToken cancellationToken)
    {
        var actorUserId = GetActorUserId();
        var now = UtcNow;
        var disputeId = Guid.NewGuid();
        var fileIds = request.StoredFileIds ?? [];

        await using var transaction = await BeginSerializableAsync(
            cancellationToken);
        var milestone = await dbContext.Milestones.SingleOrDefaultAsync(
                item => item.Id == request.MilestoneId,
                cancellationToken)
            ?? throw new NotFoundException("المرحلة المطلوب فتح النزاع بشأنها غير موجودة.");
        var contract = await dbContext.Contracts.SingleOrDefaultAsync(
                item => item.Id == milestone.ContractId,
                cancellationToken)
            ?? throw new NotFoundException("العقد المرتبط بالمرحلة غير موجود.");
        EnsureParticipant(contract.ClientUserId, contract.LawyerUserId, actorUserId);

        if (milestone.Type != MilestoneType.Standard)
        {
            throw new BusinessException(
                "لا يمكن فتح نزاع على مرحلة مصروفات لأنها تُحرر مباشرة بعد التمويل.");
        }

        if (contract.Status is not (ContractStatus.Active or ContractStatus.CompletedOnHold)
            || milestone.Status is not (
                MilestoneStatus.FundedInProgress
                or MilestoneStatus.Submitted
                or MilestoneStatus.AcceptedHold))
        {
            throw new BusinessException(
                "لا يمكن فتح نزاع إلا على مرحلة ممولة قيد التنفيذ أو قيد المراجعة أو ضمن مدة حجز الضمان.");
        }

        var verified = await fundingVerifier.VerifyAsync(
            milestone.Id,
            FundingVerificationOperation.DisputeOpening,
            cancellationToken);
        var hold = await dbContext.EscrowHolds.SingleAsync(
            item => item.Id == verified.EscrowHoldId,
            cancellationToken);

        if (milestone.Status == MilestoneStatus.AcceptedHold)
        {
            if (!hold.HoldExpiresAt.HasValue
                || !milestone.HoldExpiresAt.HasValue
                || hold.HoldExpiresAt.Value != milestone.HoldExpiresAt.Value
                || hold.HoldExpiresAt.Value <= now)
            {
                throw new BusinessException(
                    "انتهت مدة حجز الضمان أو أن بياناتها غير مكتملة، لذلك لا يمكن فتح نزاع مالي على هذه المرحلة.");
            }
        }
        else
        {
            if (hold.Status != EscrowHoldStatus.Funded)
            {
                throw new BusinessException(
                    "حجز الضمان المرتبط بالمرحلة ليس في حالة ممولة صالحة لفتح النزاع.");
            }
        }

        if (await dbContext.Disputes.AnyAsync(
                item => item.MilestoneId == milestone.Id
                    && item.Status != DisputeStatus.Closed
                    && item.Status != DisputeStatus.Cancelled,
                cancellationToken))
        {
            throw new BusinessException(
                "يوجد نزاع قائم بالفعل على هذه المرحلة ولا يمكن فتح نزاع آخر قبل حسمه أو إغلاقه.");
        }

        var dispute = new Dispute(
            disputeId,
            contract.Id,
            milestone.Id,
            actorUserId,
            request.Category,
            request.Title,
            request.Description,
            request.RequestedOutcome,
            now)
        {
            PreviousMilestoneStatus = milestone.Status,
            PreviousContractStatus = contract.Status
        };
        dbContext.Disputes.Add(dispute);
        if (fileIds.Count > 0)
        {
            await fileAccessService.AuthorizeForUseAsync(
                actorUserId,
                fileIds,
                ContractFilePurpose.DisputeEvidence,
                dispute.Id,
                cancellationToken);
        }

        dbContext.DisputeEvidence.AddRange(fileIds.Select(fileId =>
            new DisputeEvidence(
                Guid.NewGuid(),
                dispute.Id,
                actorUserId,
                fileId,
                content: null,
                now)));

        EscrowHoldTransitionGuard.EnsureCanTransition(
            hold.Status,
            EscrowHoldStatus.Frozen);
        hold.Status = EscrowHoldStatus.Frozen;
        hold.FrozenAt = now;
        hold.UpdatedAt = now;

        MilestoneTransitionGuard.EnsureCanTransition(
            milestone.Status,
            MilestoneStatus.Disputed);
        var previousMilestoneStatus = milestone.Status;
        milestone.Status = MilestoneStatus.Disputed;
        milestone.UpdatedAt = now;

        ContractTransitionGuard.EnsureCanTransition(
            contract.Status,
            ContractStatus.SuspendedByDispute);
        var previousContractStatus = contract.Status;
        contract.Status = ContractStatus.SuspendedByDispute;
        contract.UpdatedAt = now;

        var correlationId = Guid.NewGuid();
        dbContext.MilestoneStateHistories.Add(
            MilestoneStateHistoryFactory.Create(
                Guid.NewGuid(),
                milestone.Id,
                previousMilestoneStatus,
                MilestoneStatus.Disputed,
                ContractPaymentEventTypes.DisputeOpened,
                actorUserId,
                "تم فتح نزاع وتجميد حجز ضمان المرحلة لحين صدور القرار.",
                correlationId,
                now));
        dbContext.ContractStateHistories.Add(
            ContractStateHistoryFactory.Create(
                Guid.NewGuid(),
                contract.Id,
                previousContractStatus,
                ContractStatus.SuspendedByDispute,
                ContractPaymentEventTypes.DisputeOpened,
                actorUserId,
                "تم تعليق تنفيذ العقد مؤقتًا بسبب نزاع مالي قائم.",
                correlationId,
                now));
        await EnqueueDisputeEventAsync(
            ContractPaymentEventTypes.DisputeOpened,
            dispute.Id,
            correlationId,
            cancellationToken);

        await dbContext.SaveChangesAsync(cancellationToken);
        await CommitAsync(transaction, cancellationToken);
        return await MapAsync(dispute, actorUserId, cancellationToken);
    }

    public async Task<PagedResult<DisputeDto>> ListAsync(
        DisputeListQuery query,
        CancellationToken cancellationToken)
    {
        var actorUserId = GetActorUserId();
        var access = await GetAccessAsync(actorUserId, cancellationToken);
        var disputes = dbContext.Disputes.AsNoTracking().AsQueryable();
        if (!access.IsModerator)
        {
            disputes = disputes.Where(item => dbContext.Contracts.Any(contract =>
                contract.Id == item.ContractId
                && (contract.ClientUserId == actorUserId
                    || contract.LawyerUserId == actorUserId)));
        }
        else if (query.AssignedModeratorUserId.HasValue)
        {
            disputes = disputes.Where(item =>
                item.AssignedModeratorUserId
                    == query.AssignedModeratorUserId.Value);
        }

        if (query.ContractId.HasValue)
        {
            disputes = disputes.Where(item => item.ContractId == query.ContractId.Value);
        }

        if (query.MilestoneId.HasValue)
        {
            disputes = disputes.Where(item => item.MilestoneId == query.MilestoneId.Value);
        }

        if (query.Status.HasValue)
        {
            disputes = disputes.Where(item => item.Status == query.Status.Value);
        }

        if (query.Category.HasValue)
        {
            disputes = disputes.Where(item => item.Category == query.Category.Value);
        }

        if (query.RaisedByUserId.HasValue)
        {
            disputes = disputes.Where(item => item.RaisedByUserId == query.RaisedByUserId.Value);
        }

        if (query.FromDate.HasValue)
        {
            disputes = disputes.Where(item => item.CreatedAt >= query.FromDate.Value);
        }

        if (query.ToDate.HasValue)
        {
            disputes = disputes.Where(item => item.CreatedAt <= query.ToDate.Value);
        }

        var totalCount = await disputes.CountAsync(cancellationToken);
        var entities = await disputes
            .OrderByDescending(item => item.CreatedAt)
            .ThenBy(item => item.Id)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync(cancellationToken);
        var items = new List<DisputeDto>(entities.Count);
        foreach (var dispute in entities)
        {
            items.Add(await MapAsync(
                dispute,
                actorUserId,
                cancellationToken));
        }

        return new PagedResult<DisputeDto>(
            items,
            query.Page,
            query.PageSize,
            totalCount,
            query.Page * query.PageSize < totalCount);
    }

    public async Task<DisputeDto> GetAsync(
        Guid disputeId,
        CancellationToken cancellationToken)
    {
        var actorUserId = GetActorUserId();
        var dispute = await GetAuthorizedAsync(
            disputeId,
            actorUserId,
            moderatorMutation: false,
            cancellationToken);
        return await MapAsync(dispute, actorUserId, cancellationToken);
    }

    public async Task<DisputeActionResultDto> AddEvidenceAsync(
        Guid disputeId,
        AddDisputeEvidenceRequest request,
        CancellationToken cancellationToken)
    {
        var actorUserId = GetActorUserId();
        var dispute = await GetAuthorizedAsync(
            disputeId,
            actorUserId,
            moderatorMutation: false,
            cancellationToken);
        if (dispute.Status is not (
            DisputeStatus.Open
            or DisputeStatus.Assigned
            or DisputeStatus.UnderReview))
        {
            throw new BusinessException(
                "لا يمكن إضافة أدلة بعد صدور قرار النزاع أو إغلاقه.");
        }

        var fileIds = request.StoredFileIds ?? [];
        if (fileIds.Count > 0)
        {
            await fileAccessService.AuthorizeForUseAsync(
                actorUserId,
                fileIds,
                ContractFilePurpose.DisputeEvidence,
                dispute.Id,
                cancellationToken);
        }

        var now = UtcNow;
        if (!string.IsNullOrWhiteSpace(request.Content))
        {
            dbContext.DisputeEvidence.Add(
                new DisputeEvidence(
                    Guid.NewGuid(),
                    dispute.Id,
                    actorUserId,
                    storedFileId: null,
                    request.Content.Trim(),
                    now));
        }

        dbContext.DisputeEvidence.AddRange(fileIds.Select(fileId =>
            new DisputeEvidence(
                Guid.NewGuid(),
                dispute.Id,
                actorUserId,
                fileId,
                content: null,
                now)));
        dispute.UpdatedAt = now;
        await dbContext.SaveChangesAsync(cancellationToken);
        return new DisputeActionResultDto(
            dispute.Id,
            dispute.Status.ToString(),
            now);
    }

    public async Task<EvidenceDownloadUrlDto> GetEvidenceDownloadUrlAsync(
        Guid disputeId,
        Guid evidenceId,
        CancellationToken cancellationToken)
    {
        var actorUserId = GetActorUserId();
        var dispute = await GetAuthorizedAsync(
            disputeId,
            actorUserId,
            moderatorMutation: false,
            cancellationToken);

        var evidence = await dbContext.DisputeEvidence
            .AsNoTracking()
            .SingleOrDefaultAsync(
                item => item.Id == evidenceId && item.DisputeId == dispute.Id,
                cancellationToken)
            ?? throw new NotFoundException("دليل النزاع المطلوب غير موجود.");

        if (!evidence.StoredFileId.HasValue)
        {
            throw new BusinessException("لا يوجد ملف مرفق بهذا الدليل.");
        }

        var readAccess = await fileAccessService.GetAuthorizedReadAccessAsync(
            actorUserId,
            evidence.StoredFileId.Value,
            ContractFilePurpose.DisputeEvidence,
            dispute.Id,
            cancellationToken)
            ?? throw new ForbiddenAccessException("غير مصرح لك بتحميل هذا الملف.");

        return new EvidenceDownloadUrlDto(
            evidence.Id,
            evidence.StoredFileId.Value,
            readAccess.SignedUri.ToString(),
            readAccess.ExpiresAt);
    }

    public async Task<DisputeDto> AssignAsync(
        Guid disputeId,
        AssignDisputeRequest request,
        CancellationToken cancellationToken)
    {
        var actorUserId = GetActorUserId();
        var actorAccess = await GetAccessAsync(actorUserId, cancellationToken);
        EnsureModerator(actorAccess);
        var target = await userEligibilityService.FindEligibilityAsync(
            request.ModeratorUserId,
            cancellationToken);
        if (target is null || !target.IsActive || !target.CanActAsModerator)
        {
            throw new BusinessException(
                "لا يمكن تعيين المستخدم المحدد لأنه ليس مشرفًا نشطًا ومؤهلًا لإدارة النزاعات.");
        }

        var dispute = await GetForMutationAsync(disputeId, cancellationToken);
        if (dispute.Status != DisputeStatus.Open)
        {
            throw new BusinessException(
                "لا يمكن تعيين مشرف إلا لنزاع مفتوح لم يسبق تعيينه.");
        }

        DisputeTransitionGuard.EnsureCanTransition(
            dispute.Status,
            DisputeStatus.Assigned);
        var now = UtcNow;
        dispute.AssignedModeratorUserId = request.ModeratorUserId;
        dispute.Status = DisputeStatus.Assigned;
        dispute.UpdatedAt = now;
        var correlationId = Guid.NewGuid();
        await EnqueueDisputeEventAsync(
            ContractPaymentEventTypes.DisputeAssigned,
            dispute.Id,
            correlationId,
            cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
        return await MapAsync(dispute, actorUserId, cancellationToken);
    }

    public async Task<DisputeDto> ReassignAsync(
        Guid disputeId,
        ReassignDisputeRequest request,
        CancellationToken cancellationToken)
    {
        var actorUserId = GetActorUserId();
        var actorAccess = await GetAccessAsync(actorUserId, cancellationToken);
        EnsureModerator(actorAccess);

        var target = await userEligibilityService.FindEligibilityAsync(
            request.ModeratorUserId,
            cancellationToken);
        if (target is null || !target.IsActive || !target.CanActAsModerator)
        {
            throw new BusinessException(
                "لا يمكن إعادة تعيين النزاع للمستخدم المحدد لأنه ليس مشرفًا نشطًا ومؤهلًا.");
        }

        var dispute = await GetForMutationAsync(disputeId, cancellationToken);
        if (dispute.Status is not (DisputeStatus.Open or DisputeStatus.Assigned))
        {
            throw new BusinessException(
                "لا يمكن إعادة تعيين المشرف إلا للنزاعات المفتوحة أو المعينة قبل بدء المراجعة.");
        }

        DisputeTransitionGuard.EnsureCanTransition(
            dispute.Status,
            DisputeStatus.Assigned);

        var now = UtcNow;
        dispute.AssignedModeratorUserId = request.ModeratorUserId;
        dispute.Status = DisputeStatus.Assigned;
        dispute.UpdatedAt = now;
        var correlationId = Guid.NewGuid();
        await EnqueueDisputeEventAsync(
            ContractPaymentEventTypes.DisputeAssigned,
            dispute.Id,
            correlationId,
            cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
        return await MapAsync(dispute, actorUserId, cancellationToken);
    }

    public async Task<DisputeActionResultDto> WithdrawAsync(
        Guid disputeId,
        WithdrawDisputeRequest request,
        CancellationToken cancellationToken)
    {
        var actorUserId = GetActorUserId();
        var dispute = await GetForMutationAsync(disputeId, cancellationToken);
        var access = await GetAccessAsync(actorUserId, cancellationToken);

        if (dispute.RaisedByUserId != actorUserId && !access.IsSuperAdministrator)
        {
            throw new ForbiddenAccessException(
                "لا يمكن سحب النزاع إلا بواسطة الطرف الذي قام بفتحه أو المشرف العام.");
        }

        if (dispute.Status is not (
            DisputeStatus.Open
            or DisputeStatus.Assigned
            or DisputeStatus.UnderReview))
        {
            throw new BusinessException(
                "لا يمكن سحب النزاع بعد صدور القرار النهائي أو إغلاقه.");
        }

        DisputeTransitionGuard.EnsureCanTransition(
            dispute.Status,
            DisputeStatus.Cancelled);

        await using var transaction = await BeginSerializableAsync(cancellationToken);
        var now = UtcNow;
        var correlationId = Guid.NewGuid();

        var milestone = await dbContext.Milestones.SingleAsync(
            item => item.Id == dispute.MilestoneId,
            cancellationToken);
        var contract = await dbContext.Contracts.SingleAsync(
            item => item.Id == dispute.ContractId,
            cancellationToken);
        var hold = await dbContext.EscrowHolds.SingleOrDefaultAsync(
            item => item.MilestoneId == dispute.MilestoneId,
            cancellationToken);

        dispute.Status = DisputeStatus.Cancelled;
        dispute.CancelledAt = now;
        dispute.CancelledByUserId = actorUserId;
        dispute.CancellationReason = request.Reason.Trim();
        dispute.UpdatedAt = now;

        if (hold is not null && hold.Status == EscrowHoldStatus.Frozen)
        {
            EscrowHoldTransitionGuard.EnsureCanTransition(
                hold.Status,
                EscrowHoldStatus.Funded);
            hold.Status = EscrowHoldStatus.Funded;
            hold.FrozenAt = null;
            hold.UpdatedAt = now;
        }

        var restoredMilestoneStatus = dispute.PreviousMilestoneStatus
            ?? MilestoneStatus.FundedInProgress;
        if (milestone.Status == MilestoneStatus.Disputed)
        {
            MilestoneTransitionGuard.EnsureCanTransition(
                milestone.Status,
                restoredMilestoneStatus);
            var prevMilestoneStatus = milestone.Status;
            milestone.Status = restoredMilestoneStatus;
            milestone.UpdatedAt = now;
            dbContext.MilestoneStateHistories.Add(
                MilestoneStateHistoryFactory.Create(
                    Guid.NewGuid(),
                    milestone.Id,
                    prevMilestoneStatus,
                    restoredMilestoneStatus,
                    "DisputeCancelled",
                    actorUserId,
                    $"تم سحب النزاع وإعادة المرحلة لحالتها السابقة: {request.Reason}",
                    correlationId,
                    now));
        }

        var restoredContractStatus = dispute.PreviousContractStatus
            ?? ContractStatus.Active;
        if (contract.Status == ContractStatus.SuspendedByDispute)
        {
            ContractTransitionGuard.EnsureCanTransition(
                contract.Status,
                restoredContractStatus);
            var prevContractStatus = contract.Status;
            contract.Status = restoredContractStatus;
            contract.UpdatedAt = now;
            dbContext.ContractStateHistories.Add(
                ContractStateHistoryFactory.Create(
                    Guid.NewGuid(),
                    contract.Id,
                    prevContractStatus,
                    restoredContractStatus,
                    "DisputeCancelled",
                    actorUserId,
                    $"استؤنف العقد بعد سحب النزاع القائم: {request.Reason}",
                    correlationId,
                    now));
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        await CommitAsync(transaction, cancellationToken);

        return new DisputeActionResultDto(
            dispute.Id,
            dispute.Status.ToString(),
            now);
    }

    public async Task<DisputeDto> StartReviewAsync(
        Guid disputeId,
        CancellationToken cancellationToken)
    {
        var actorUserId = GetActorUserId();
        var dispute = await GetForModeratorMutationAsync(
            disputeId,
            actorUserId,
            cancellationToken);
        if (dispute.Status != DisputeStatus.Assigned)
        {
            throw new BusinessException(
                "لا يمكن بدء المراجعة إلا بعد تعيين مشرف للنزاع.");
        }

        DisputeTransitionGuard.EnsureCanTransition(
            dispute.Status,
            DisputeStatus.UnderReview);
        dispute.Status = DisputeStatus.UnderReview;
        dispute.UpdatedAt = UtcNow;
        await dbContext.SaveChangesAsync(cancellationToken);
        return await MapAsync(dispute, actorUserId, cancellationToken);
    }

    public async Task<DisputeDto> ResolveAsync(
        Guid disputeId,
        ResolveDisputeRequest request,
        string? idempotencyKey,
        CancellationToken cancellationToken)
    {
        var actorUserId = GetActorUserId();
        var preview = await GetForModeratorMutationAsync(
            disputeId,
            actorUserId,
            cancellationToken);
        if (preview.Status != DisputeStatus.UnderReview)
        {
            throw new BusinessException(
                "لا يمكن إصدار قرار النزاع قبل بدء المشرف المعيّن لمرحلة المراجعة.");
        }

        var holdId = await dbContext.EscrowHolds
            .Where(item => item.MilestoneId == preview.MilestoneId)
            .Select(item => item.Id)
            .SingleOrDefaultAsync(cancellationToken);
        if (holdId == Guid.Empty)
        {
            throw new BusinessException(
                "حجز الضمان المرتبط بالنزاع غير موجود.");
        }

        IdempotencyReservation reservation;
        try
        {
            reservation = await idempotencyService.ReserveAsync(
                IdempotencyScope.ForHoldSettlement(
                    actorUserId,
                    ResolveOperation,
                    holdId),
                idempotencyKey,
                request,
                cancellationToken);
        }
        catch (BusinessException exception)
        {
            throw new BusinessException(
                "تعذر قبول مفتاح تسوية النزاع لأنه مستخدم لقرار مختلف أو لأن القرار ما زال قيد المعالجة.",
                exception);
        }

        if (reservation.IsReplay)
        {
            var replay = await GetForMutationAsync(
                reservation.ResultReferenceId ?? disputeId,
                cancellationToken);
            return await MapAsync(replay, actorUserId, cancellationToken);
        }

        var pendingTransactionIds = new List<Guid>();
        await using var transaction =
            await SerializableOperationTransaction.CreateAsync(
                dbContext,
                cancellationToken);
        var dispute = await GetForModeratorMutationAsync(
            disputeId,
            actorUserId,
            cancellationToken);
        if (await dbContext.DisputeResolutions.AnyAsync(
                item => item.DisputeId == dispute.Id,
                cancellationToken))
        {
            throw new BusinessException(
                "صدر قرار نهائي لهذا النزاع بالفعل ولا يمكن استبداله بقرار آخر.");
        }

        var milestone = await dbContext.Milestones.SingleAsync(
            item => item.Id == dispute.MilestoneId,
            cancellationToken);
        var contract = await dbContext.Contracts.SingleAsync(
            item => item.Id == dispute.ContractId,
            cancellationToken);
        var hold = await dbContext.EscrowHolds.SingleAsync(
            item => item.Id == holdId,
            cancellationToken);
        var account = await dbContext.EscrowAccounts.SingleAsync(
            item => item.Id == hold.EscrowAccountId,
            cancellationToken);
        var wallet = await dbContext.LawyerWallets.SingleOrDefaultAsync(
                item => item.LawyerUserId == contract.LawyerUserId,
                cancellationToken)
            ?? throw new BusinessException(
                "محفظة المحامي المرتبطة بحجز الضمان غير موجودة.");

        EnsureSettlementState(dispute, milestone, contract, hold, account, wallet);
        var breakdown = ValidateBreakdown(request, hold);
        var now = UtcNow;
        var correlationId = Guid.NewGuid();
        var resolution = new DisputeResolution(
            Guid.NewGuid(),
            dispute.Id,
            request.ResolutionType,
            breakdown.GrossAmount,
            breakdown.ClientRefundAmount,
            breakdown.LawyerNetAmount,
            breakdown.PlatformFeeAmount,
            request.Summary,
            actorUserId,
            now,
            now);
        dbContext.DisputeResolutions.Add(resolution);

        PaymentTransaction? refundTransaction = null;
        PaymentTransaction? releaseTransaction = null;
        if (breakdown.ClientRefundAmount > 0m)
        {
            refundTransaction = CreateProviderTransaction(
                dispute,
                hold,
                PaymentOperationType.Refund,
                breakdown.ClientRefundAmount,
                $"dispute-refund-{dispute.Id:N}",
                now);
            dbContext.PaymentTransactions.Add(refundTransaction);
        }

        if (breakdown.LawyerNetAmount > 0m)
        {
            releaseTransaction = CreateProviderTransaction(
                dispute,
                hold,
                PaymentOperationType.Release,
                breakdown.LawyerNetAmount,
                $"dispute-release-{dispute.Id:N}",
                now);
            dbContext.PaymentTransactions.Add(releaseTransaction);
        }

        DisputeTransitionGuard.EnsureCanTransition(
            dispute.Status,
            DisputeStatus.Resolved);
        dispute.Status = DisputeStatus.Resolved;
        dispute.ResolutionType = request.ResolutionType;
        dispute.ResolutionAmount = breakdown.ClientRefundAmount;
        dispute.ResolutionSummary = request.Summary;
        dispute.ResolvedByUserId = actorUserId;
        dispute.ResolvedAt = now;
        dispute.UpdatedAt = now;

        if (request.PenaltyType.HasValue)
        {
            var access = await GetAccessAsync(actorUserId, cancellationToken);
            if (!access.IsSuperAdministrator)
            {
                throw new ForbiddenAccessException(
                    "لا يملك المشرف الحالي صلاحية تطبيق عقوبة إدارية على المحامي.");
            }

            dbContext.LawyerPenalties.Add(CreatePenalty(
                contract.LawyerUserId,
                dispute.Id,
                request.PenaltyType.Value,
                request.PenaltyReason!,
                actorUserId,
                now));
        }

        await EnqueueDisputeEventAsync(
            ContractPaymentEventTypes.DisputeResolved,
            dispute.Id,
            correlationId,
            cancellationToken);

        await dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAndCloseAsync(cancellationToken);

        if (refundTransaction is not null)
        {
            var result = await ExecuteRefundAsync(
                refundTransaction,
                hold,
                request.Summary,
                cancellationToken);
            await transaction.BeginAsync(cancellationToken);
            ApplyProviderResult(refundTransaction, result, now);
            await dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAndCloseAsync(cancellationToken);
            if (refundTransaction.Status
                != PaymentTransactionStatus.Completed)
            {
                pendingTransactionIds.Add(refundTransaction.Id);
            }
        }

        if (releaseTransaction is not null)
        {
            var result = await ExecuteReleaseAsync(
                releaseTransaction,
                hold,
                cancellationToken);
            await transaction.BeginAsync(cancellationToken);
            ApplyProviderResult(releaseTransaction, result, now);
            await dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAndCloseAsync(cancellationToken);
            if (releaseTransaction.Status
                != PaymentTransactionStatus.Completed)
            {
                pendingTransactionIds.Add(releaseTransaction.Id);
            }
        }

        await transaction.BeginAsync(cancellationToken);

        if (pendingTransactionIds.Count == 0)
        {
            await FinalizeSettlementAsync(
                dispute,
                resolution,
                milestone,
                contract,
                hold,
                account,
                wallet,
                refundTransaction,
                releaseTransaction,
                actorUserId,
                correlationId,
                now,
                cancellationToken);
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        if (pendingTransactionIds.Count == 0)
        {
            await idempotencyService.CompleteAsync(
                reservation.RecordId,
                200,
                new { DisputeId = dispute.Id, Status = dispute.Status.ToString() },
                dispute.Id,
                cancellationToken);
        }

        await transaction.CommitAndCloseAsync(cancellationToken);
        foreach (var paymentTransactionId in pendingTransactionIds)
        {
            await TryScheduleRecoveryAsync(
                paymentTransactionId,
                cancellationToken);
        }

        if (pendingTransactionIds.Count == 0)
        {
            await completionEvaluator.EvaluateCompletionAsync(
                contract.Id,
                cancellationToken);
        }

        return await MapAsync(dispute, actorUserId, cancellationToken);
    }

    public async Task<DisputeActionResultDto> CloseAsync(
        Guid disputeId,
        CancellationToken cancellationToken)
    {
        var actorUserId = GetActorUserId();
        var dispute = await GetForModeratorMutationAsync(
            disputeId,
            actorUserId,
            cancellationToken);
        if (dispute.Status != DisputeStatus.Resolved)
        {
            throw new BusinessException(
                "لا يمكن إغلاق النزاع قبل صدور القرار النهائي.");
        }

        var hold = await dbContext.EscrowHolds.SingleOrDefaultAsync(
            item => item.MilestoneId == dispute.MilestoneId,
            cancellationToken);
        var hasPendingSettlement = hold is null
            || hold.Status is EscrowHoldStatus.Funded or EscrowHoldStatus.Frozen
            || await dbContext.PaymentTransactions.AnyAsync(
                item => item.EscrowHoldId == hold.Id
                    && item.OperationType != PaymentOperationType.Deposit
                    && item.Status == PaymentTransactionStatus.Processing,
                cancellationToken);
        if (hasPendingSettlement)
        {
            throw new BusinessException(
                "لا يمكن إغلاق النزاع قبل اكتمال جميع عمليات التسوية والمطابقة المالية.");
        }

        var resolvedEventProcessed = await dbContext.OutboxMessages.AnyAsync(
            item => item.AggregateType == "Dispute"
                && item.AggregateId == dispute.Id
                && item.EventType == ContractPaymentEventTypes.DisputeResolved
                && item.Status == OutboxStatus.Processed,
            cancellationToken);
        if (!resolvedEventProcessed)
        {
            throw new BusinessException(
                "لا يمكن إغلاق النزاع قبل اكتمال إرسال إشعارات قرار التسوية.");
        }

        DisputeTransitionGuard.EnsureCanTransition(
            dispute.Status,
            DisputeStatus.Closed);
        var now = UtcNow;
        dispute.Status = DisputeStatus.Closed;
        dispute.ClosedAt = now;
        dispute.UpdatedAt = now;
        await EnqueueDisputeEventAsync(
            ContractPaymentEventTypes.DisputeClosed,
            dispute.Id,
            Guid.NewGuid(),
            cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
        await completionEvaluator.EvaluateCompletionAsync(
            dispute.ContractId,
            cancellationToken);
        return new DisputeActionResultDto(
            dispute.Id,
            dispute.Status.ToString(),
            now);
    }

    public async Task<DisputeStatsDto> GetStatsAsync(
        CancellationToken cancellationToken)
    {
        var actorUserId = GetActorUserId();
        var access = await GetAccessAsync(actorUserId, cancellationToken);
        EnsureModerator(access);

        var query = dbContext.Disputes.AsNoTracking();
        var totalOpen = await query.CountAsync(d => d.Status == DisputeStatus.Open, cancellationToken);
        var totalAssigned = await query.CountAsync(d => d.Status == DisputeStatus.Assigned, cancellationToken);
        var totalUnderReview = await query.CountAsync(d => d.Status == DisputeStatus.UnderReview, cancellationToken);
        var totalResolved = await query.CountAsync(d => d.Status == DisputeStatus.Resolved, cancellationToken);
        var totalClosed = await query.CountAsync(d => d.Status == DisputeStatus.Closed, cancellationToken);
        var totalCancelled = await query.CountAsync(d => d.Status == DisputeStatus.Cancelled, cancellationToken);
        var unassignedCount = await query.CountAsync(
            d => d.Status == DisputeStatus.Open && !d.AssignedModeratorUserId.HasValue,
            cancellationToken);

        return new DisputeStatsDto(
            totalOpen,
            totalAssigned,
            totalUnderReview,
            totalResolved,
            totalClosed,
            totalCancelled,
            unassignedCount);
    }

    public async Task<JobExecutionResult> RecoverPendingSettlementsAsync(
        CancellationToken cancellationToken)
    {
        var disputeIds = await (
                from dispute in dbContext.Disputes.AsNoTracking()
                join hold in dbContext.EscrowHolds.AsNoTracking()
                    on dispute.MilestoneId equals hold.MilestoneId
                where dispute.Status == DisputeStatus.Resolved
                    && hold.Status == EscrowHoldStatus.Frozen
                orderby dispute.UpdatedAt, dispute.Id
                select dispute.Id)
            .Take(RecoveryBatchSize)
            .ToListAsync(cancellationToken);
        var completed = 0;
        foreach (var disputeId in disputeIds)
        {
            if (await RecoverPendingSettlementAsync(
                    disputeId,
                    cancellationToken))
            {
                completed++;
            }

            dbContext.ChangeTracker.Clear();
        }

        return completed == 0
            ? JobExecutionResult.NoOp("NoPendingDisputeSettlementCompleted")
            : JobExecutionResult.Completed(
                "PendingDisputeSettlementsCompleted",
                completed);
    }

    private async Task<bool> RecoverPendingSettlementAsync(
        Guid disputeId,
        CancellationToken cancellationToken)
    {
        await using var transaction =
            await SerializableOperationTransaction.CreateAsync(
                dbContext,
                cancellationToken);
        var dispute = await dbContext.Disputes.SingleOrDefaultAsync(
            item => item.Id == disputeId,
            cancellationToken);
        if (dispute is null || dispute.Status != DisputeStatus.Resolved)
        {
            await transaction.CommitAndCloseAsync(cancellationToken);
            return false;
        }

        var resolution = await dbContext.DisputeResolutions.SingleOrDefaultAsync(
            item => item.DisputeId == dispute.Id,
            cancellationToken)
            ?? throw new BusinessException(
                "لا يمكن استرداد تسوية النزاع لعدم وجود القرار المالي النهائي.");
        var milestone = await dbContext.Milestones.SingleAsync(
            item => item.Id == dispute.MilestoneId,
            cancellationToken);
        var contract = await dbContext.Contracts.SingleAsync(
            item => item.Id == dispute.ContractId,
            cancellationToken);
        var hold = await dbContext.EscrowHolds.SingleAsync(
            item => item.MilestoneId == dispute.MilestoneId,
            cancellationToken);
        if (hold.Status != EscrowHoldStatus.Frozen
            || milestone.Status != MilestoneStatus.Disputed)
        {
            await transaction.CommitAndCloseAsync(cancellationToken);
            return false;
        }

        var account = await dbContext.EscrowAccounts.SingleOrDefaultAsync(
            item => item.Id == hold.EscrowAccountId,
            cancellationToken)
            ?? throw new BusinessException(
                "حساب الضمان المطلوب لاسترداد تسوية النزاع غير موجود.");
        var wallet = await dbContext.LawyerWallets.SingleOrDefaultAsync(
            item => item.LawyerUserId == contract.LawyerUserId,
            cancellationToken)
            ?? throw new BusinessException(
                "محفظة المحامي المطلوبة لاسترداد تسوية النزاع غير موجودة.");
        var now = UtcNow;
        var pendingTransactionIds = new List<Guid>();
        var refundTransaction = await RecoverProviderSettlementAsync(
            dispute,
            resolution,
            hold,
            PaymentOperationType.Refund,
            resolution.ClientRefundAmount,
            now,
            pendingTransactionIds,
            transaction,
            cancellationToken);
        var releaseGrossAmount = resolution.LawyerReleaseAmount;
        var releaseTransaction = await RecoverProviderSettlementAsync(
            dispute,
            resolution,
            hold,
            PaymentOperationType.Release,
            releaseGrossAmount,
            now,
            pendingTransactionIds,
            transaction,
            cancellationToken);

        if ((resolution.ClientRefundAmount > 0m && refundTransaction is null)
            || (releaseGrossAmount > 0m && releaseTransaction is null))
        {
            await dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAndCloseAsync(cancellationToken);
            foreach (var paymentTransactionId in pendingTransactionIds)
            {
                await TryScheduleRecoveryAsync(
                    paymentTransactionId,
                    cancellationToken);
            }

            return false;
        }

        var correlationId = Guid.NewGuid();
        await FinalizeSettlementAsync(
            dispute,
            resolution,
            milestone,
            contract,
            hold,
            account,
            wallet,
            refundTransaction,
            releaseTransaction,
            resolution.ResolvedByUserId,
            correlationId,
            now,
            cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        var reservationId = await dbContext.IdempotencyRecords
            .Where(item => item.ResourceType
                    == IdempotencyScope.HoldSettlementResourceType
                && item.ResourceId == hold.Id
                && item.Operation == ResolveOperation
                && item.Status == IdempotencyStatus.Processing)
            .Select(item => (Guid?)item.Id)
            .SingleOrDefaultAsync(cancellationToken);
        if (reservationId.HasValue)
        {
            await idempotencyService.CompleteAsync(
                reservationId.Value,
                200,
                new { DisputeId = dispute.Id, Status = dispute.Status.ToString() },
                dispute.Id,
                cancellationToken);
        }

        await transaction.CommitAndCloseAsync(cancellationToken);
        await completionEvaluator.EvaluateCompletionAsync(
            contract.Id,
            cancellationToken);
        return true;
    }

    private async Task<PaymentTransaction?> RecoverProviderSettlementAsync(
        Dispute dispute,
        DisputeResolution resolution,
        EscrowHold hold,
        PaymentOperationType operationType,
        decimal amount,
        DateTimeOffset now,
        ICollection<Guid> pendingTransactionIds,
        SerializableOperationTransaction transaction,
        CancellationToken cancellationToken)
    {
        if (amount == 0m)
        {
            return null;
        }

        var keyPrefix = operationType == PaymentOperationType.Refund
            ? $"dispute-refund-{dispute.Id:N}"
            : $"dispute-release-{dispute.Id:N}";
        var attempts = await dbContext.PaymentTransactions
            .Where(item => item.EscrowHoldId == hold.Id
                && item.OperationType == operationType
                && item.IdempotencyKey.StartsWith(keyPrefix))
            .OrderBy(item => item.CreatedAt)
            .ThenBy(item => item.Id)
            .ToListAsync(cancellationToken);
        var completed = attempts.FirstOrDefault(item =>
            item.Status == PaymentTransactionStatus.Completed
            && item.Amount == amount);
        if (completed is not null)
        {
            return completed;
        }

        var processing = attempts.FirstOrDefault(item =>
            item.Status == PaymentTransactionStatus.Processing
            && item.Amount == amount);
        if (processing is not null)
        {
            pendingTransactionIds.Add(processing.Id);
            return null;
        }

        var attemptNumber = attempts.Count + 1;
        var retry = CreateProviderTransaction(
            dispute,
            hold,
            operationType,
            amount,
            $"{keyPrefix}-{attemptNumber}",
            now);
        dbContext.PaymentTransactions.Add(retry);
        await dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAndCloseAsync(cancellationToken);
        var result = operationType == PaymentOperationType.Refund
            ? await ExecuteRefundAsync(
                retry,
                hold,
                resolution.Summary,
                cancellationToken)
            : await ExecuteReleaseAsync(retry, hold, cancellationToken);
        await transaction.BeginAsync(cancellationToken);
        ApplyProviderResult(retry, result, now);
        if (retry.Status != PaymentTransactionStatus.Completed)
        {
            pendingTransactionIds.Add(retry.Id);
            return null;
        }

        return retry;
    }

    private async Task FinalizeSettlementAsync(
        Dispute dispute,
        DisputeResolution resolution,
        SmartCourt.Features.Milestones.Entities.Milestone milestone,
        SmartCourt.Features.Contracts.Entities.Contract contract,
        EscrowHold hold,
        EscrowAccount account,
        LawyerWallet wallet,
        PaymentTransaction? refundTransaction,
        PaymentTransaction? releaseTransaction,
        Guid actorUserId,
        Guid correlationId,
        DateTimeOffset now,
        CancellationToken cancellationToken)
    {
        var runningBalance = CurrentBalance(account);
        if (runningBalance < resolution.GrossHoldAmount)
        {
            throw new BusinessException(
                "رصيد حساب الضمان لا يكفي لتنفيذ قرار تسوية النزاع.");
        }

        if (resolution.ClientRefundAmount > 0m)
        {
            runningBalance -= resolution.ClientRefundAmount;
            dbContext.EscrowLedgerEntries.Add(new EscrowLedgerEntry(
                Guid.NewGuid(),
                account.Id,
                hold.Id,
                LedgerTransactionType.Refund,
                resolution.ClientRefundAmount,
                runningBalance,
                "DisputeResolution",
                dispute.Id,
                refundTransaction?.Id,
                "رد المبلغ المعتمد للعميل وفق قرار تسوية النزاع.",
                actorUserId,
                correlationId,
                now));
            account.TotalRefunded += resolution.ClientRefundAmount;
        }

        if (resolution.LawyerReleaseAmount > 0m)
        {
            runningBalance -= resolution.LawyerReleaseAmount;
            dbContext.EscrowLedgerEntries.Add(new EscrowLedgerEntry(
                Guid.NewGuid(),
                account.Id,
                hold.Id,
                LedgerTransactionType.Release,
                resolution.LawyerReleaseAmount,
                runningBalance,
                "DisputeResolution",
                dispute.Id,
                releaseTransaction?.Id,
                "تحرير صافي مستحق المحامي وفق قرار تسوية النزاع.",
                actorUserId,
                correlationId,
                now));
            account.TotalReleased += resolution.LawyerReleaseAmount;
        }

        if (resolution.PlatformFeeAmount > 0m)
        {
            runningBalance -= resolution.PlatformFeeAmount;
            dbContext.EscrowLedgerEntries.Add(new EscrowLedgerEntry(
                Guid.NewGuid(),
                account.Id,
                hold.Id,
                LedgerTransactionType.PlatformFee,
                resolution.PlatformFeeAmount,
                runningBalance,
                "DisputeResolution",
                dispute.Id,
                releaseTransaction?.Id,
                "تسجيل رسوم المنصة على الجزء غير المردود من تسوية النزاع.",
                actorUserId,
                correlationId,
                now));
            account.TotalFees += resolution.PlatformFeeAmount;
        }

        account.UpdatedAt = now;
        wallet.PendingBalance -= hold.NetAmount;
        wallet.AvailableBalance += resolution.LawyerReleaseAmount;
        wallet.UpdatedAt = now;
        if (releaseTransaction?.ProviderAmountMinor is > 0)
        {
            var payoutAccount = await dbContext.LawyerPayoutAccounts
                .SingleOrDefaultAsync(
                    item => item.LawyerUserId == contract.LawyerUserId
                        && item.Status == LawyerPayoutAccountStatus.Enabled,
                    cancellationToken)
                ?? throw new BusinessException(
                    "حساب سحب المحامي غير متاح لإتمام تسوية النزاع.");
            if (payoutAccount.AvailableProviderAmountMinor >
                long.MaxValue - releaseTransaction.ProviderAmountMinor.Value)
            {
                throw new BusinessException(
                    "تجاوز رصيد مزود الدفع الحد العددي المسموح به.");
            }

            payoutAccount.AvailableProviderAmountMinor +=
                releaseTransaction.ProviderAmountMinor.Value;
            payoutAccount.DefaultCurrency =
                releaseTransaction.ProviderCurrency
                ?? payoutAccount.DefaultCurrency;
            payoutAccount.UpdatedAt = now;
        }

        var targetHoldStatus = resolution.ResolutionType
            == DisputeResolutionType.FullRefund
            ? EscrowHoldStatus.Refunded
            : EscrowHoldStatus.Released;
        EscrowHoldTransitionGuard.EnsureCanTransition(
            hold.Status,
            targetHoldStatus);
        hold.Status = targetHoldStatus;
        hold.SettledAt = now;
        hold.SettlementType = resolution.ResolutionType switch
        {
            DisputeResolutionType.FullRefund => SettlementType.Refund,
            DisputeResolutionType.FullRelease => SettlementType.Release,
            _ => SettlementType.PartialSplit
        };
        hold.ProviderRefundTransactionId = refundTransaction?.Id;
        hold.ProviderReleaseTransactionId = releaseTransaction?.Id;
        hold.UpdatedAt = now;

        var targetMilestoneStatus = resolution.ResolutionType
            == DisputeResolutionType.FullRefund
            ? MilestoneStatus.Refunded
            : MilestoneStatus.Released;
        MilestoneTransitionGuard.EnsureCanTransition(
            milestone.Status,
            targetMilestoneStatus);
        var previousMilestoneStatus = milestone.Status;
        milestone.Status = targetMilestoneStatus;
        milestone.RefundedAt = targetMilestoneStatus == MilestoneStatus.Refunded
            ? now
            : milestone.RefundedAt;
        milestone.ReleasedAt = targetMilestoneStatus == MilestoneStatus.Released
            ? now
            : milestone.ReleasedAt;
        milestone.UpdatedAt = now;
        dbContext.MilestoneStateHistories.Add(
            MilestoneStateHistoryFactory.Create(
                Guid.NewGuid(),
                milestone.Id,
                previousMilestoneStatus,
                targetMilestoneStatus,
                ContractPaymentEventTypes.DisputeResolved,
                actorUserId,
                "تمت تسوية المرحلة ماليًا وفق القرار النهائي للنزاع.",
                correlationId,
                now));

        if (contract.Status == ContractStatus.SuspendedByDispute)
        {
            ContractTransitionGuard.EnsureCanTransition(
                contract.Status,
                ContractStatus.Active);
            var previousContractStatus = contract.Status;
            contract.Status = ContractStatus.Active;
            contract.UpdatedAt = now;
            dbContext.ContractStateHistories.Add(
                ContractStateHistoryFactory.Create(
                    Guid.NewGuid(),
                    contract.Id,
                    previousContractStatus,
                    ContractStatus.Active,
                    ContractPaymentEventTypes.DisputeResolved,
                    actorUserId,
                    "استؤنف العقد بعد اكتمال تسوية النزاع.",
                    correlationId,
                    now));
        }

        if (refundTransaction is not null)
        {
            await outboxWriter.EnqueueAsync(
                new OutboxEvent(
                    ContractPaymentEventTypes.FundsRefunded,
                    1,
                    new FundsRefundedEventPayload(
                        milestone.Id,
                        hold.Id,
                        refundTransaction.Id,
                        resolution.ClientRefundAmount),
                    "EscrowHold",
                    hold.Id,
                    correlationId),
                cancellationToken);
        }

        if (releaseTransaction is not null)
        {
            await outboxWriter.EnqueueAsync(
                new OutboxEvent(
                    ContractPaymentEventTypes.FundsReleased,
                    1,
                    new FundsReleasedEventPayload(
                        milestone.Id,
                        hold.Id,
                        releaseTransaction.Id,
                        resolution.LawyerReleaseAmount,
                        resolution.PlatformFeeAmount),
                    "EscrowHold",
                    hold.Id,
                    correlationId),
                cancellationToken);
        }
    }

    private async Task<DisputeDto> MapAsync(
        Dispute dispute,
        Guid actorUserId,
        CancellationToken cancellationToken)
    {
        var evidence = await dbContext.DisputeEvidence
            .AsNoTracking()
            .Where(item => item.DisputeId == dispute.Id)
            .OrderBy(item => item.CreatedAt)
            .ThenBy(item => item.Id)
            .Select(item => new DisputeEvidenceDto(
                item.Id,
                item.UploadedByUserId,
                item.StoredFileId,
                item.Content,
                item.CreatedAt))
            .ToListAsync(cancellationToken);
        var resolution = await dbContext.DisputeResolutions
            .AsNoTracking()
            .SingleOrDefaultAsync(
                item => item.DisputeId == dispute.Id,
                cancellationToken);
        DisputeSettlementDto? settlement = null;
        if (resolution is not null)
        {
            var hold = await dbContext.EscrowHolds.AsNoTracking()
                .SingleOrDefaultAsync(
                    item => item.MilestoneId == dispute.MilestoneId,
                    cancellationToken);
            var statuses = hold is null
                ? []
                : await dbContext.PaymentTransactions.AsNoTracking()
                    .Where(item => item.EscrowHoldId == hold.Id
                        && item.OperationType != PaymentOperationType.Deposit)
                    .Select(item => item.Status)
                    .ToListAsync(cancellationToken);
            var settlementStatus = hold?.Status is EscrowHoldStatus.Released
                    or EscrowHoldStatus.Refunded
                ? "Completed"
                : statuses.Any(status => status == PaymentTransactionStatus.Failed)
                    ? "Failed"
                    : "Processing";
            settlement = new DisputeSettlementDto(
                settlementStatus,
                resolution.GrossHoldAmount,
                resolution.ClientRefundAmount,
                resolution.LawyerReleaseAmount,
                resolution.PlatformFeeAmount);
        }

        var access = await GetAccessAsync(actorUserId, cancellationToken);
        var isAssigned = dispute.AssignedModeratorUserId == actorUserId
            || access.IsSuperAdministrator;
        var canWithdraw = (dispute.RaisedByUserId == actorUserId || access.IsSuperAdministrator)
            && dispute.Status is DisputeStatus.Open or DisputeStatus.Assigned or DisputeStatus.UnderReview;

        return new DisputeDto(
            dispute.Id,
            dispute.ContractId,
            dispute.MilestoneId,
            dispute.RaisedByUserId,
            dispute.AssignedModeratorUserId,
            dispute.Category,
            dispute.Title,
            dispute.Description,
            dispute.Status,
            dispute.RequestedOutcome,
            dispute.ResolutionType,
            dispute.ResolutionSummary,
            dispute.ResolvedAt,
            dispute.ClosedAt,
            dispute.CancelledAt,
            dispute.CancellationReason,
            dispute.CreatedAt,
            dispute.UpdatedAt,
            evidence,
            settlement,
            new DisputePermittedActionsDto(
                dispute.Status is DisputeStatus.Open
                    or DisputeStatus.Assigned
                    or DisputeStatus.UnderReview,
                access.IsModerator && dispute.Status == DisputeStatus.Open,
                access.IsModerator && dispute.Status is DisputeStatus.Open or DisputeStatus.Assigned,
                isAssigned && dispute.Status == DisputeStatus.Assigned,
                isAssigned && dispute.Status == DisputeStatus.UnderReview,
                isAssigned && dispute.Status == DisputeStatus.Resolved
                    && settlement?.Status == "Completed",
                canWithdraw));
    }

    private async Task<Dispute> GetAuthorizedAsync(
        Guid disputeId,
        Guid actorUserId,
        bool moderatorMutation,
        CancellationToken cancellationToken)
    {
        var dispute = await GetForMutationAsync(disputeId, cancellationToken);
        var contract = await dbContext.Contracts.AsNoTracking().SingleAsync(
            item => item.Id == dispute.ContractId,
            cancellationToken);
        if (contract.ClientUserId == actorUserId
            || contract.LawyerUserId == actorUserId)
        {
            if (moderatorMutation)
            {
                throw new ForbiddenAccessException(
                    "إدارة النزاع وإصدار القرار متاحة للمشرفين المخولين فقط.");
            }

            return dispute;
        }

        var access = await GetAccessAsync(actorUserId, cancellationToken);
        EnsureModerator(access);
        return dispute;
    }

    private async Task<Dispute> GetForModeratorMutationAsync(
        Guid disputeId,
        Guid actorUserId,
        CancellationToken cancellationToken)
    {
        var dispute = await GetAuthorizedAsync(
            disputeId,
            actorUserId,
            moderatorMutation: true,
            cancellationToken);
        var access = await GetAccessAsync(actorUserId, cancellationToken);
        if (!access.IsSuperAdministrator
            && dispute.AssignedModeratorUserId != actorUserId)
        {
            throw new ForbiddenAccessException(
                "لا يمكن إدارة هذا النزاع إلا بواسطة المشرف المعيّن له.");
        }

        return dispute;
    }

    private async Task<Dispute> GetForMutationAsync(
        Guid disputeId,
        CancellationToken cancellationToken)
    {
        if (disputeId == Guid.Empty)
        {
            throw new BusinessException("معرّف النزاع مطلوب.");
        }

        return await dbContext.Disputes.SingleOrDefaultAsync(
                item => item.Id == disputeId,
                cancellationToken)
            ?? throw new NotFoundException("النزاع المطلوب غير موجود.");
    }

    private async Task<DisputeAccess> GetAccessAsync(
        Guid actorUserId,
        CancellationToken cancellationToken)
    {
        var eligibility = await userEligibilityService.FindEligibilityAsync(
            actorUserId,
            cancellationToken);
        return new DisputeAccess(
            eligibility is not null
                && eligibility.IsActive
                && (eligibility.CanActAsModerator
                    || eligibility.CanActAsSuperAdministrator),
            eligibility is not null
                && eligibility.IsActive
                && eligibility.CanActAsSuperAdministrator);
    }

    private static void EnsureModerator(DisputeAccess access)
    {
        if (!access.IsModerator)
        {
            throw new ForbiddenAccessException(
                "الوصول إلى إدارة النزاعات متاح للمشرفين المخولين فقط.");
        }
    }

    private static void EnsureParticipant(
        Guid clientUserId,
        Guid lawyerUserId,
        Guid actorUserId)
    {
        if (actorUserId != clientUserId && actorUserId != lawyerUserId)
        {
            throw new ForbiddenAccessException(
                "لا يمكن فتح النزاع إلا بواسطة أحد طرفي العقد.");
        }
    }

    private static void EnsureSettlementState(
        Dispute dispute,
        SmartCourt.Features.Milestones.Entities.Milestone milestone,
        SmartCourt.Features.Contracts.Entities.Contract contract,
        EscrowHold hold,
        EscrowAccount account,
        LawyerWallet wallet)
    {
        if (dispute.Status != DisputeStatus.UnderReview
            || milestone.Status != MilestoneStatus.Disputed
            || contract.Status != ContractStatus.SuspendedByDispute
            || hold.Status != EscrowHoldStatus.Frozen)
        {
            throw new BusinessException(
                "لا يمكن تسوية النزاع لأن حالة العقد أو المرحلة أو حجز الضمان تغيرت.");
        }

        if (hold.ContractId != contract.Id
            || hold.MilestoneId != milestone.Id
            || account.ContractId != contract.Id
            || !string.Equals(account.Currency, "EGP", StringComparison.Ordinal)
            || !string.Equals(wallet.Currency, "EGP", StringComparison.Ordinal)
            || hold.GrossAmount != hold.NetAmount + hold.PlatformFeeAmount
            || wallet.PendingBalance < hold.NetAmount)
        {
            throw new BusinessException(
                "بيانات التسوية المالية لا تتطابق مع العقد والمرحلة وحجز الضمان والمحفظة.");
        }
    }

    private static SettlementBreakdown ValidateBreakdown(
        ResolveDisputeRequest request,
        EscrowHold hold)
    {
        var breakdown = SettlementCalculator.CalculateFromFundedHold(
            hold.GrossAmount,
            hold.PlatformFeeAmount,
            request.ClientRefundAmount);
        var validType = request.ResolutionType switch
        {
            DisputeResolutionType.FullRefund =>
                breakdown.ClientRefundAmount == hold.GrossAmount,
            DisputeResolutionType.FullRelease =>
                breakdown.ClientRefundAmount == 0m,
            DisputeResolutionType.PartialSplit =>
                breakdown.ClientRefundAmount > 0m
                && breakdown.ClientRefundAmount < hold.GrossAmount,
            _ => false
        };
        if (!validType
            || request.LawyerReleaseAmount != breakdown.LawyerNetAmount)
        {
            throw new BusinessException(
                "المبالغ المكتوبة لا تطابق قيمة حجز الضمان ورسوم المنصة ونوع قرار النزاع.");
        }

        return breakdown;
    }

    private PaymentTransaction CreateProviderTransaction(
        Dispute dispute,
        EscrowHold hold,
        PaymentOperationType operationType,
        decimal amount,
        string providerIdempotencyKey,
        DateTimeOffset now)
        => new(
            Guid.NewGuid(),
            dispute.ContractId,
            dispute.MilestoneId,
            operationType,
            paymentProvider.GetType().Name,
            providerIdempotencyKey,
            amount,
            now)
        {
            EscrowHoldId = hold.Id
        };

    private async Task<ProviderResult> ExecuteRefundAsync(
        PaymentTransaction transaction,
        EscrowHold hold,
        string reason,
        CancellationToken cancellationToken)
    {
        try
        {
            var depositTransaction = paymentProvider
                is ILawyerPayoutAccountProvider
                ? await GetProviderDepositAsync(hold, cancellationToken)
                : null;
            return await paymentProvider.RefundAsync(
                new ProviderRefundRequest(
                    transaction.Amount,
                    transaction.Currency,
                    hold.Id,
                    transaction.IdempotencyKey,
                    transaction.Id,
                    reason,
                    depositTransaction?.ProviderTransactionId
                        ?? string.Empty),
                cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
        {
            logger.LogWarning(
                exception,
                "تعذر التأكد من نتيجة رد مبلغ النزاع للحجز {EscrowHoldId}.",
                hold.Id);
            return UnknownResult(transaction, hold.Id);
        }
    }

    private async Task<ProviderResult> ExecuteReleaseAsync(
        PaymentTransaction transaction,
        EscrowHold hold,
        CancellationToken cancellationToken)
    {
        try
        {
            PaymentTransaction? depositTransaction = null;
            LawyerPayoutAccount? payoutAccount = null;
            if (paymentProvider is ILawyerPayoutAccountProvider)
            {
                depositTransaction = await GetProviderDepositAsync(
                    hold,
                    cancellationToken);
                var contract = await dbContext.Contracts
                    .AsNoTracking()
                    .SingleAsync(
                        item => item.Id == hold.ContractId,
                        cancellationToken);
                payoutAccount = await dbContext.LawyerPayoutAccounts
                    .AsNoTracking()
                    .SingleOrDefaultAsync(
                        item => item.LawyerUserId == contract.LawyerUserId
                            && item.Status == LawyerPayoutAccountStatus.Enabled,
                        cancellationToken)
                    ?? throw new BusinessException(
                        "يجب تفعيل حساب سحب المحامي قبل تحرير أموال النزاع.");
            }
            return await paymentProvider.ReleaseAsync(
                new ProviderReleaseRequest(
                    transaction.Amount,
                    transaction.Currency,
                    hold.Id,
                    transaction.IdempotencyKey,
                    transaction.Id,
                    depositTransaction?.ProviderTransactionId
                        ?? string.Empty,
                    depositTransaction?.ProviderRelatedTransactionId
                        ?? string.Empty,
                    payoutAccount?.ProviderAccountId ?? string.Empty,
                    hold.GrossAmount),
                cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
        {
            logger.LogWarning(
                exception,
                "تعذر التأكد من نتيجة تحرير مبلغ النزاع للحجز {EscrowHoldId}.",
                hold.Id);
            return UnknownResult(transaction, hold.Id);
        }
    }

    private async Task<PaymentTransaction> GetProviderDepositAsync(
        EscrowHold hold,
        CancellationToken cancellationToken)
    {
        var deposit = await dbContext.PaymentTransactions
            .AsNoTracking()
            .SingleOrDefaultAsync(
                item => item.Id == hold.ProviderDepositTransactionId
                    && item.OperationType == PaymentOperationType.Deposit
                    && item.Status == PaymentTransactionStatus.Completed,
                cancellationToken);
        if (deposit is null
            || string.IsNullOrWhiteSpace(deposit.ProviderTransactionId))
        {
            throw new BusinessException(
                "معرّف عملية الإيداع لدى مزود الدفع غير متاح للتسوية.");
        }

        return deposit;
    }

    private static ProviderResult UnknownResult(
        PaymentTransaction transaction,
        Guid businessId)
        => new(
            transaction.Amount,
            transaction.Currency,
            businessId,
            transaction.IdempotencyKey,
            transaction.Id,
            ProviderOperationOutcome.Unknown,
            ProviderTransactionId: null,
            "تعذر التأكد من نتيجة عملية التسوية لدى مزود الدفع.");

    private static void ApplyProviderResult(
        PaymentTransaction transaction,
        ProviderResult result,
        DateTimeOffset now)
    {
        if (result.Amount != transaction.Amount
            || !string.Equals(result.Currency, transaction.Currency, StringComparison.Ordinal)
            || result.BusinessId != transaction.EscrowHoldId
            || !string.Equals(
                result.ProviderIdempotencyKey,
                transaction.IdempotencyKey,
                StringComparison.Ordinal)
            || result.CorrelationId != transaction.Id)
        {
            transaction.Status = PaymentTransactionStatus.Processing;
            transaction.FailureReason =
                "بيانات نتيجة مزود الدفع لا تطابق طلب تسوية النزاع.";
            transaction.UpdatedAt = now;
            return;
        }

        transaction.Status = result.Outcome switch
        {
            ProviderOperationOutcome.Succeeded => PaymentTransactionStatus.Completed,
            ProviderOperationOutcome.Failed => PaymentTransactionStatus.Failed,
            _ => PaymentTransactionStatus.Processing
        };
        transaction.ProviderTransactionId = result.Outcome
            == ProviderOperationOutcome.Succeeded
            ? result.ProviderTransactionId
            : null;
        transaction.ProviderRelatedTransactionId =
            result.RelatedProviderTransactionId;
        transaction.ProviderStatus = result.ProviderStatus;
        transaction.ProviderObjectType = result.ProviderObjectType;
        transaction.ProviderAmountMinor = result.ProviderMoney?.AmountMinor;
        transaction.ProviderCurrency = result.ProviderMoney?.Currency;
        if (transaction.Status == PaymentTransactionStatus.Completed
            && (string.IsNullOrWhiteSpace(transaction.ProviderTransactionId)
                || transaction.ProviderTransactionId.Length > 200))
        {
            transaction.Status = PaymentTransactionStatus.Processing;
            transaction.ProviderTransactionId = null;
            transaction.FailureReason =
                "لم يرسل مزود الدفع معرّفًا صالحًا لعملية التسوية الناجحة.";
        }
        else
        {
            transaction.FailureReason = transaction.Status
                == PaymentTransactionStatus.Completed
                ? null
                : result.FailureReason
                    ?? "لم تكتمل عملية تسوية النزاع لدى مزود الدفع.";
        }

        transaction.ProcessedAt = transaction.Status
            is PaymentTransactionStatus.Completed or PaymentTransactionStatus.Failed
            ? now
            : null;
        transaction.UpdatedAt = now;
    }

    private static LawyerPenalty CreatePenalty(
        Guid lawyerUserId,
        Guid disputeId,
        PenaltyType penaltyType,
        string reason,
        Guid actorUserId,
        DateTimeOffset now)
    {
        var endsAt = penaltyType switch
        {
            PenaltyType.Suspension12Months => now.AddMonths(12),
            PenaltyType.Suspension24Months => now.AddMonths(24),
            _ => (DateTimeOffset?)null
        };
        return new LawyerPenalty(
            Guid.NewGuid(),
            lawyerUserId,
            disputeId,
            penaltyType,
            reason,
            now,
            endsAt,
            actorUserId,
            now);
    }

    private async Task EnqueueDisputeEventAsync(
        string eventType,
        Guid disputeId,
        Guid correlationId,
        CancellationToken cancellationToken)
    {
        await outboxWriter.EnqueueAsync(
            new OutboxEvent(
                eventType,
                1,
                new ContractPaymentAggregateEventPayload(disputeId),
                "Dispute",
                disputeId,
                correlationId),
            cancellationToken);
    }

    private async Task TryScheduleRecoveryAsync(
        Guid paymentTransactionId,
        CancellationToken cancellationToken)
    {
        try
        {
            await jobScheduler.ScheduleProviderReconciliationAsync(
                paymentTransactionId,
                UtcNow.AddMinutes(5),
                cancellationToken);
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            logger.LogWarning(
                exception,
                "تعذر جدولة مطابقة معاملة تسوية النزاع {PaymentTransactionId} وستلتقطها المطابقة الدورية.",
                paymentTransactionId);
        }
    }

    private async Task<Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction?>
        BeginSerializableAsync(CancellationToken cancellationToken)
        => dbContext.Database.IsRelational()
            ? await dbContext.Database.BeginTransactionAsync(
                IsolationLevel.Serializable,
                cancellationToken)
            : null;

    private static async Task CommitAsync(
        Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction? transaction,
        CancellationToken cancellationToken)
    {
        if (transaction is not null)
        {
            await transaction.CommitAsync(cancellationToken);
        }
    }

    private Guid GetActorUserId()
    {
        if (!currentUserService.IsAuthenticated
            || !currentUserService.UserId.HasValue
            || currentUserService.UserId.Value == Guid.Empty)
        {
            throw new AuthenticationException(
                "يجب تسجيل الدخول للوصول إلى النزاعات.");
        }

        return currentUserService.UserId.Value;
    }

    private DateTimeOffset UtcNow => timeProvider.GetUtcNow();

    private static decimal CurrentBalance(EscrowAccount account)
        => account.TotalDeposited
            - account.TotalReleased
            - account.TotalRefunded
            - account.TotalFees;

    private sealed record DisputeAccess(
        bool IsModerator,
        bool IsSuperAdministrator);
}
