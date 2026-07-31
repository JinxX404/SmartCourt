using System.Security.Cryptography;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using SmartCourt.Common.Exceptions;
using SmartCourt.Features.Contracts.Dependencies;
using SmartCourt.Features.Contracts.Domain;
using SmartCourt.Features.Contracts.DTOs;
using SmartCourt.Features.Contracts.Entities;
using SmartCourt.Features.Contracts.Enums;
using SmartCourt.Features.Disputes.Enums;
using SmartCourt.Features.Milestones.Entities;
using SmartCourt.Features.Milestones.Enums;
using SmartCourt.Features.Payments.Entities;
using SmartCourt.Features.Payments.Enums;
using SmartCourt.Features.Payments.Integration;
using SmartCourt.Features.Users.Integration;
using SmartCourt.Infrastructure.Providers.Events;
using SmartCourt.Interfaces;
using SmartCourt.Persistence;

namespace SmartCourt.Features.Contracts;

public sealed class ContractService : IContractService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;
    private readonly IContractCreationDependencyGate _creationGate;
    private readonly IContractUserEligibilityService _userEligibilityService;
    private readonly IOutboxWriter _outboxWriter;
    private readonly IContractQueryService _contractQueryService;
    private readonly IReadOnlyCollection<IContractTerminationSettlementService>
        _terminationSettlementServices;
    private readonly TimeProvider _timeProvider;

    public ContractService(
        ApplicationDbContext dbContext,
        ICurrentUserService currentUserService,
        IContractCreationDependencyGate creationGate,
        IContractUserEligibilityService userEligibilityService,
        IContractQueryService contractQueryService,
        IOutboxWriter outboxWriter,
        IEnumerable<IContractTerminationSettlementService>
            terminationSettlementServices,
        TimeProvider timeProvider)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
        _creationGate = creationGate;
        _userEligibilityService = userEligibilityService;
        _contractQueryService = contractQueryService;
        _outboxWriter = outboxWriter;
        _terminationSettlementServices =
            terminationSettlementServices.ToArray();
        _timeProvider = timeProvider;
    }

    public async Task<ContractDetailDto> CreateAsync(
        CreateContractRequest request,
        CancellationToken cancellationToken)
    {
        var actorUserId = GetActorUserId();
        var facts = await _creationGate.VerifyAsync(
            request.ProposalId,
            actorUserId,
            cancellationToken);
        if (await _dbContext.Contracts.AnyAsync(
                contract => contract.ProposalId == facts.ProposalId,
                cancellationToken))
        {
            throw new ConflictException(
                "تم إنشاء عقد لهذا العرض مسبقًا.");
        }

        var now = UtcNow;
        var correlationId = Guid.NewGuid();
        await using var transaction =
            await _dbContext.Database.BeginTransactionAsync(
                cancellationToken);
        var contract = new Contract(
            Guid.NewGuid(),
            facts.ProposalId,
            facts.LegalCaseId,
            facts.ClientUserId,
            facts.LawyerUserId,
            request.Title,
            request.TermsAndConditions,
            now);
        _dbContext.Contracts.Add(contract);
        _dbContext.ContractStateHistories.Add(
            new ContractStateHistory(
                Guid.NewGuid(),
                contract.Id,
                null,
                ContractStatus.Draft,
                ContractPaymentEventTypes.ContractCreated,
                actorUserId,
                "تم إنشاء مسودة العقد من العرض المقبول.",
                correlationId,
                now));
        await EnqueueContractEventAsync(
            ContractPaymentEventTypes.ContractCreated,
            contract.Id,
            correlationId,
            cancellationToken);
        try
        {
            await SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception)
            when (IsDuplicateProposalConstraintViolation(exception))
        {
            throw new ConflictException(
                "تم إنشاء عقد لهذا العرض مسبقًا.");
        }

        await transaction.CommitAsync(cancellationToken);
        return await _contractQueryService.MapDetailAsync(contract, cancellationToken);
    }

    public Task<ContractDetailDto> GetAsync(
        Guid contractId,
        CancellationToken cancellationToken)
    {
        return _contractQueryService.GetAsync(
            contractId,
            cancellationToken);
    }

    public async Task<ContractDetailDto> UpdateDraftAsync(
        Guid contractId,
        UpdateContractRequest request,
        string ifMatch,
        CancellationToken cancellationToken)
    {
        var actorUserId = GetActorUserId();
        await using var transaction =
            await _dbContext.Database.BeginTransactionAsync(
                cancellationToken);
        var contract = await GetContractForMutationAsync(
            contractId,
            cancellationToken);
        if (contract.LawyerUserId != actorUserId)
        {
            throw new ForbiddenAccessException(
                "محامي العقد فقط هو من يمكنه تعديل المسودة.");
        }

        EnsureExpectedVersion(contract, ifMatch);
        if (contract.Status != ContractStatus.Draft)
        {
            throw new BusinessException(
                "لا يمكن تعديل العقد إلا عندما يكون في حالة مسودة.");
        }

        contract.Title = request.Title;
        contract.TermsAndConditions = request.TermsAndConditions;
        contract.AcceptedByClientAt = null;
        contract.AcceptedByLawyerAt = null;
        contract.UpdatedAt = UtcNow;
        await SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return await MapDetailAsync(contract, cancellationToken);
    }

    public async Task<ContractActionResultDto> AcceptAsync(
        Guid contractId,
        string ifMatch,
        CancellationToken cancellationToken)
    {
        var actorUserId = GetActorUserId();
        var now = UtcNow;
        var correlationId = Guid.NewGuid();
        await using var transaction =
            await _dbContext.Database.BeginTransactionAsync(
                cancellationToken);
        var contract = await GetContractForMutationAsync(
            contractId,
            cancellationToken);
        EnsureParticipant(contract, actorUserId);
        EnsureExpectedVersion(contract, ifMatch);
        if (contract.Status != ContractStatus.Draft)
        {
            throw new BusinessException(
                "لا يمكن قبول العقد إلا عندما يكون في حالة مسودة.");
        }

        if (contract.ClientUserId == actorUserId)
        {
            if (contract.AcceptedByClientAt.HasValue)
            {
                throw new ConflictException(
                    "قام العميل بقبول النسخة الحالية من العقد مسبقًا.");
            }

            contract.AcceptedByClientAt = now;
        }
        else
        {
            if (contract.AcceptedByLawyerAt.HasValue)
            {
                throw new ConflictException(
                    "قام المحامي بقبول النسخة الحالية من العقد مسبقًا.");
            }

            contract.AcceptedByLawyerAt = now;
        }

        contract.UpdatedAt = now;
        await EnqueueContractEventAsync(
            ContractPaymentEventTypes.ContractAccepted,
            contract.Id,
            correlationId,
            cancellationToken);
        await TryActivateAsync(
            contract,
            actorUserId,
            correlationId,
            now,
            cancellationToken);
        await SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return ToActionResult(contract, now);
    }

    public async Task<ContractActionResultDto> EvaluateActivationAsync(
        Guid contractId,
        CancellationToken cancellationToken)
    {
        var actorUserId = GetActorUserId();
        var now = UtcNow;
        var correlationId = Guid.NewGuid();
        await using var transaction =
            await _dbContext.Database.BeginTransactionAsync(
                cancellationToken);
        var contract = await GetContractForMutationAsync(
            contractId,
            cancellationToken);
        EnsureParticipant(contract, actorUserId);
        await TryActivateAsync(
            contract,
            actorUserId,
            correlationId,
            now,
            cancellationToken);
        await SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return ToActionResult(contract, now);
    }

    public async Task<PagedResult<ContractStateHistoryDto>>
        GetStateHistoryAsync(
            Guid contractId,
            ContractStateHistoryQuery query,
            CancellationToken cancellationToken)
    {
        await GetAuthorizedContractAsync(contractId, cancellationToken);
        var history = _dbContext.ContractStateHistories
            .AsNoTracking()
            .Where(item => item.ContractId == contractId);
        var totalCount = await history.CountAsync(cancellationToken);
        var items = await history
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

    public async Task<ContractActionResultDto> EvaluateCompletionAsync(
        Guid contractId,
        CancellationToken cancellationToken)
    {
        var actorUserId = GetActorUserId();
        var now = UtcNow;
        var correlationId = Guid.NewGuid();
        await using var transaction =
            await _dbContext.Database.BeginTransactionAsync(
                cancellationToken);
        var contract = await GetContractForMutationAsync(
            contractId,
            cancellationToken);
        EnsureParticipant(contract, actorUserId);
        if (contract.Status is ContractStatus.Completed
            or ContractStatus.Terminated)
        {
            await transaction.CommitAsync(cancellationToken);
            return ToActionResult(contract, now);
        }

        var milestones = await _dbContext.Milestones
            .Where(milestone => milestone.ContractId == contract.Id)
            .Select(
                milestone => new
                {
                    milestone.Amount,
                    milestone.AcceptedByClientAt,
                    milestone.AcceptedByLawyerAt,
                    milestone.Status
                })
            .ToListAsync(cancellationToken);
        var approvedMilestones = milestones
            .Where(milestone =>
                milestone.Amount > 0m
                && milestone.AcceptedByClientAt.HasValue
                && milestone.AcceptedByLawyerAt.HasValue)
            .ToArray();
        var hasActiveDispute = await _dbContext.Disputes.AnyAsync(
            dispute =>
                dispute.ContractId == contract.Id
                && dispute.Status != DisputeStatus.Closed,
            cancellationToken);
        var hasPendingProviderAttempt =
            await _dbContext.PaymentTransactions.AnyAsync(
                payment =>
                    payment.ContractId == contract.Id
                    && payment.Status
                        == PaymentTransactionStatus.Processing,
                cancellationToken);
        var hasUnsettledHold = await _dbContext.EscrowHolds.AnyAsync(
            hold =>
                hold.ContractId == contract.Id
                && (hold.Status == EscrowHoldStatus.Funded
                    || hold.Status == EscrowHoldStatus.Frozen),
            cancellationToken);
        var allApprovedMilestonesFinished =
            approvedMilestones.Length > 0
            && approvedMilestones.All(milestone =>
                milestone.Status is MilestoneStatus.Released
                    or MilestoneStatus.Refunded
                    or MilestoneStatus.Cancelled);
        if (allApprovedMilestonesFinished
            && !hasActiveDispute
            && !hasPendingProviderAttempt
            && !hasUnsettledHold)
        {
            var previousStatus = contract.Status;
            ContractTransitionGuard.EnsureCanTransition(
                previousStatus,
                ContractStatus.Completed);
            contract.Status = ContractStatus.Completed;
            contract.CompletedAt = now;
            contract.UpdatedAt = now;
            AddHistory(
                contract,
                previousStatus,
                ContractStatus.Completed,
                ContractPaymentEventTypes.ContractCompleted,
                actorUserId,
                "اكتملت جميع مراحل العقد وتمت تسويتها.",
                correlationId,
                now);
            await EnqueueContractEventAsync(
                ContractPaymentEventTypes.ContractCompleted,
                contract.Id,
                correlationId,
                cancellationToken);
        }

        await SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return ToActionResult(contract, now);
    }

    public async Task<ContractDetailDto> TerminateAsync(
        Guid contractId,
        TerminateContractRequest request,
        string ifMatch,
        CancellationToken cancellationToken)
    {
        var actorUserId = GetActorUserId();
        var correlationId = Guid.NewGuid();
        var contract = await GetContractForMutationAsync(
            contractId,
            cancellationToken);
        EnsureParticipant(contract, actorUserId);
        EnsureExpectedVersion(contract, ifMatch);
        if (contract.Status is ContractStatus.Completed
            or ContractStatus.Terminated)
        {
            throw new BusinessException(
                "لا يمكن إنهاء عقد مكتمل أو منتهٍ.");
        }

        var hasProcessingPayment =
            await _dbContext.Milestones.AnyAsync(
                milestone =>
                    milestone.ContractId == contract.Id
                    && milestone.Status
                        == MilestoneStatus.FundingProcessing,
                cancellationToken)
            || await _dbContext.PaymentTransactions.AnyAsync(
                payment =>
                    payment.ContractId == contract.Id
                    && payment.OperationType
                        == PaymentOperationType.Deposit
                    && payment.Status
                        == PaymentTransactionStatus.Processing,
                cancellationToken);
        if (hasProcessingPayment)
        {
            throw new ConflictException(
                "لا يمكن إنهاء العقد قبل حسم عملية الدفع قيد المعالجة.");
        }

        var requiresSettlement = await _dbContext.EscrowHolds.AnyAsync(
            hold =>
                hold.ContractId == contract.Id
                && (hold.Status == EscrowHoldStatus.Funded
                    || hold.Status == EscrowHoldStatus.Frozen),
            cancellationToken);
        if (requiresSettlement)
        {
            var settlementService = GetTerminationSettlementService();
            var settlement =
                await settlementService.SettleForTerminationAsync(
                    contract.Id,
                    actorUserId,
                    request.Reason,
                    correlationId,
                    cancellationToken);
            if (!settlement.Completed)
            {
                throw new ConflictException(
                    "لم تكتمل التسوية المالية المطلوبة لإنهاء العقد.");
            }

            _dbContext.ChangeTracker.Clear();
        }

        var now = UtcNow;
        await using var transaction =
            await _dbContext.Database.BeginTransactionAsync(
                cancellationToken);
        contract = await GetContractForMutationAsync(
            contractId,
            cancellationToken);
        EnsureParticipant(contract, actorUserId);
        EnsureExpectedVersion(contract, ifMatch);
        var blockingMilestone = await _dbContext.Milestones.AnyAsync(
            milestone =>
                milestone.ContractId == contract.Id
                && (milestone.Status == MilestoneStatus.FundedInProgress
                    || milestone.Status == MilestoneStatus.Submitted
                    || milestone.Status == MilestoneStatus.AcceptedHold
                    || milestone.Status == MilestoneStatus.Disputed
                    || milestone.Status == MilestoneStatus.FundingProcessing),
            cancellationToken);
        if (blockingMilestone)
        {
            throw new ConflictException(
                "لا يمكن إنهاء العقد قبل تسوية جميع المراحل النشطة.");
        }

        var futureMilestones = await _dbContext.Milestones
            .Where(milestone =>
                milestone.ContractId == contract.Id
                && (milestone.Status == MilestoneStatus.Draft
                    || milestone.Status == MilestoneStatus.AwaitingFunding))
            .ToListAsync(cancellationToken);
        foreach (var milestone in futureMilestones)
        {
            milestone.Status = MilestoneStatus.Cancelled;
            milestone.UpdatedAt = now;
        }

        var previousStatus = contract.Status;
        ContractTransitionGuard.EnsureCanTransition(
            previousStatus,
            ContractStatus.Terminated);
        contract.Status = ContractStatus.Terminated;
        contract.TerminatedAt = now;
        contract.TerminationReason = request.Reason;
        contract.TerminatedByUserId = actorUserId;
        contract.UpdatedAt = now;
        AddHistory(
            contract,
            previousStatus,
            ContractStatus.Terminated,
            ContractPaymentEventTypes.ContractTerminated,
            actorUserId,
            request.Reason,
            correlationId,
            now);
        await EnqueueContractTerminatedEventAsync(
            contract,
            actorUserId,
            correlationId,
            cancellationToken);
        await SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return await _contractQueryService.MapDetailAsync(contract, cancellationToken);
    }

    private async Task<bool> TryActivateAsync(
        Contract contract,
        Guid actorUserId,
        Guid correlationId,
        DateTime now,
        CancellationToken cancellationToken)
    {
        if (contract.Status != ContractStatus.Draft
            || !contract.AcceptedByClientAt.HasValue
            || !contract.AcceptedByLawyerAt.HasValue)
        {
            return false;
        }

        var hasApprovedMilestone = await _dbContext.Milestones.AnyAsync(
            milestone =>
                milestone.ContractId == contract.Id
                && milestone.Amount > 0
                && milestone.AcceptedByClientAt != null
                && milestone.AcceptedByLawyerAt != null
                && milestone.Status != MilestoneStatus.Cancelled,
            cancellationToken);
        if (!hasApprovedMilestone)
        {
            return false;
        }

        ContractTransitionGuard.EnsureCanTransition(
            ContractStatus.Draft,
            ContractStatus.Active);
        contract.Status = ContractStatus.Active;
        contract.ActivatedAt = now;
        contract.UpdatedAt = now;
        AddHistory(
            contract,
            ContractStatus.Draft,
            ContractStatus.Active,
            ContractPaymentEventTypes.ContractActivated,
            actorUserId,
            "وافق طرفا العقد على نسخة تتضمن مرحلة معتمدة ومسعّرة.",
            correlationId,
            now);
        await EnqueueContractEventAsync(
            ContractPaymentEventTypes.ContractActivated,
            contract.Id,
            correlationId,
            cancellationToken);
        return true;
    }

    private async Task<Contract> GetAuthorizedContractAsync(
        Guid contractId,
        CancellationToken cancellationToken)
    {
        var actorUserId = GetActorUserId();
        var contract = await _dbContext.Contracts
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
            await _userEligibilityService.FindEligibilityAsync(
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

    private async Task<Contract> GetContractForMutationAsync(
        Guid contractId,
        CancellationToken cancellationToken)
    {
        if (contractId == Guid.Empty)
        {
            throw new BusinessException("معرّف العقد مطلوب.");
        }

        return await _dbContext.Contracts.SingleOrDefaultAsync(
                contract => contract.Id == contractId,
                cancellationToken)
            ?? throw new NotFoundException("العقد غير موجود.");
    }

    private async Task<ContractDetailDto> MapDetailAsync(
        Contract contract,
        CancellationToken cancellationToken)
    {
        var milestones = await _dbContext.Milestones
            .AsNoTracking()
            .Where(milestone => milestone.ContractId == contract.Id)
            .OrderBy(milestone => milestone.OrderNumber)
            .ToListAsync(cancellationToken);
        var milestoneIds = milestones
            .Select(milestone => milestone.Id)
            .ToArray();
        var holds = await _dbContext.EscrowHolds
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
            milestoneDtos,
            paymentDtos,
            GetPermittedActions(contract));
    }

    private static ContractMilestoneDto MapMilestone(
        Milestone milestone,
        EscrowHold? hold)
    {
        var fundingStatus = milestone.Status switch
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
        return new ContractMilestoneDto(
            milestone.Id,
            milestone.OrderNumber,
            milestone.Title,
            milestone.Description,
            milestone.Amount,
            milestone.DurationDays,
            milestone.DueDate,
            milestone.Status,
            fundingStatus,
            hold?.Id,
            milestone.FundedAt,
            milestone.SubmittedAt,
            milestone.AutoAcceptEligibleAt,
            milestone.HoldExpiresAt,
            hold?.NetAmount);
    }

    private IReadOnlyList<string> GetPermittedActions(Contract contract)
    {
        var actorUserId = GetActorUserId();
        var actions = new List<string>();
        if (contract.Status == ContractStatus.Draft)
        {
            if (contract.LawyerUserId == actorUserId)
            {
                actions.Add("Update");
            }

            if ((contract.ClientUserId == actorUserId
                    && !contract.AcceptedByClientAt.HasValue)
                || (contract.LawyerUserId == actorUserId
                    && !contract.AcceptedByLawyerAt.HasValue))
            {
                actions.Add("Accept");
            }
        }

        if ((contract.Status is ContractStatus.Draft
                or ContractStatus.Active
                or ContractStatus.SuspendedByDispute)
            && (contract.ClientUserId == actorUserId
                || contract.LawyerUserId == actorUserId))
        {
            actions.Add("Terminate");
        }

        return actions;
    }

    private void EnsureExpectedVersion(
        Contract contract,
        string ifMatch)
    {
        var expectedVersion = ParseIfMatch(ifMatch);
        if (contract.RowVersion.Length == 0
            || expectedVersion.Length != contract.RowVersion.Length
            || !CryptographicOperations.FixedTimeEquals(
                expectedVersion,
                contract.RowVersion))
        {
            throw new ConflictException(
                "تم تعديل العقد بواسطة عملية أخرى. يرجى إعادة تحميله والمحاولة مرة أخرى.");
        }

        _dbContext.Entry(contract)
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
                "قيمة If-Match غير صالحة.");
        }

        try
        {
            var rowVersion = Convert.FromBase64String(ifMatch[1..^1]);
            return rowVersion.Length > 0
                ? rowVersion
                : throw new BusinessException(
                    "قيمة If-Match غير صالحة.");
        }
        catch (FormatException exception)
        {
            throw new BusinessException(
                "قيمة If-Match غير صالحة.",
                exception);
        }
    }

    private static void EnsureParticipant(
        Contract contract,
        Guid actorUserId)
    {
        if (contract.ClientUserId != actorUserId
            && contract.LawyerUserId != actorUserId)
        {
            throw new ForbiddenAccessException(
                "هذا الإجراء متاح لطرفي العقد فقط.");
        }
    }

    private IContractTerminationSettlementService
        GetTerminationSettlementService()
    {
        if (_terminationSettlementServices.Count == 1)
        {
            return _terminationSettlementServices.Single();
        }

        throw new BusinessException(
            "خدمة التسوية المالية اللازمة لإنهاء العقد غير متاحة.");
    }

    private void AddHistory(
        Contract contract,
        ContractStatus previousStatus,
        ContractStatus newStatus,
        string trigger,
        Guid actorUserId,
        string reason,
        Guid correlationId,
        DateTime occurredAt)
    {
        _dbContext.ContractStateHistories.Add(
            ContractStateHistoryFactory.Create(
                Guid.NewGuid(),
                contract.Id,
                previousStatus,
                newStatus,
                trigger,
                actorUserId,
                reason,
                correlationId,
                occurredAt));
    }

    private async Task EnqueueContractEventAsync(
        string eventType,
        Guid contractId,
        Guid correlationId,
        CancellationToken cancellationToken)
    {
        await _outboxWriter.EnqueueAsync(
            new OutboxEvent(
                eventType,
                1,
                new ContractPaymentAggregateEventPayload(contractId),
                "Contract",
                contractId,
                correlationId),
            cancellationToken);
    }

    private async Task EnqueueContractTerminatedEventAsync(
        Contract contract,
        Guid actorUserId,
        Guid correlationId,
        CancellationToken cancellationToken)
    {
        await _outboxWriter.EnqueueAsync(
            new OutboxEvent(
                ContractPaymentEventTypes.ContractTerminated,
                1,
                new ContractTerminatedEventPayload(
                    contract.Id,
                    contract.LegalCaseId,
                    actorUserId),
                "Contract",
                contract.Id,
                correlationId),
            cancellationToken);
    }

    private async Task SaveChangesAsync(
        CancellationToken cancellationToken)
    {
        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new ConflictException(
                "تم تعديل العقد بواسطة عملية أخرى. يرجى إعادة تحميله والمحاولة مرة أخرى.");
        }
    }

    private static bool IsDuplicateProposalConstraintViolation(
        DbUpdateException exception)
    {
        return exception.InnerException is SqlException
            {
                Number: 2601 or 2627
            } sqlException
            && sqlException.Message.Contains(
                "UX_Contracts_ProposalId",
                StringComparison.Ordinal);
    }

    private Guid GetActorUserId()
    {
        if (!_currentUserService.IsAuthenticated
            || !_currentUserService.UserId.HasValue
            || _currentUserService.UserId.Value == Guid.Empty)
        {
            throw new AuthenticationException(
                "يجب تسجيل الدخول لإتمام هذا الإجراء.");
        }

        return _currentUserService.UserId.Value;
    }

    private static ContractActionResultDto ToActionResult(
        Contract contract,
        DateTime occurredAt)
    {
        return new ContractActionResultDto(
            contract.Id,
            contract.Status.ToString(),
            occurredAt);
    }

    private DateTime UtcNow =>
        _timeProvider.GetUtcNow().UtcDateTime;
}
