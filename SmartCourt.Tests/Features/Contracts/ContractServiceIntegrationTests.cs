using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using SmartCourt.Common.Exceptions;
using SmartCourt.Entities;
using SmartCourt.Common.Enums;
using SmartCourt.Features.Case.Integration;
using SmartCourt.Features.Chat.Entities;
using SmartCourt.Features.Chat.Shared;
using SmartCourt.Features.Contracts;
using SmartCourt.Features.Contracts.Dependencies;
using SmartCourt.Features.Contracts.DTOs;
using SmartCourt.Features.Contracts.Entities;
using SmartCourt.Features.Contracts.Enums;
using SmartCourt.Features.Contracts.Integration;
using SmartCourt.Features.Milestones.Entities;
using SmartCourt.Features.Milestones.Enums;
using SmartCourt.Features.Payments;
using SmartCourt.Features.Payments.Entities;
using SmartCourt.Features.Payments.Enums;
using SmartCourt.Features.Payments.Integration;
using SmartCourt.Features.Proposals.Entities;
using SmartCourt.Features.Proposals.Enums;
using SmartCourt.Features.Users.Integration;
using SmartCourt.Infrastructure.Providers.Events;
using SmartCourt.Infrastructure.Providers.Jobs;
using SmartCourt.Infrastructure.Providers.Payments;
using SmartCourt.Interfaces;
using SmartCourt.Persistence;
using Xunit;

namespace SmartCourt.Tests.Features.Contracts;

public sealed class ContractServiceIntegrationTests : IAsyncLifetime
{
    private static readonly JsonSerializerOptions EventJsonOptions = new(
        JsonSerializerDefaults.Web);

    private readonly string _databaseName =
        $"SmartCourtContractServiceTests_{Guid.NewGuid():N}";
    private readonly DateTime _utcNow =
        new(2026, 8, 10, 9, 0, 0, DateTimeKind.Utc);
    private readonly Guid _clientUserId = Guid.NewGuid();
    private readonly Guid _lawyerUserId = Guid.NewGuid();

    public async Task InitializeAsync()
    {
        await using var context = CreateContext();
        await context.Database.EnsureCreatedAsync();
        context.Users.AddRange(
            CreateUser(_clientUserId, "contract-client"),
            CreateUser(_lawyerUserId, "contract-lawyer"));
        context.Set<SmartCourt.Common.Entities.ClientProfile>().Add(
            new SmartCourt.Common.Entities.ClientProfile { UserId = _clientUserId });
        context.Set<SmartCourt.Common.Entities.LawyerProfile>().Add(
            new SmartCourt.Common.Entities.LawyerProfile { UserId = _lawyerUserId });
        await context.SaveChangesAsync();
    }

    public async Task DisposeAsync()
    {
        await using var context = CreateContext();
        await context.Database.EnsureDeletedAsync();
    }

    [Fact]
    public async Task CreateAsync_UsesAuthoritativeFactsAndWritesHistoryAndOutbox()
    {
        await using var context = CreateContext();
        var proposalId = Guid.NewGuid();
        var legalCaseId = Guid.NewGuid();
        var currentUser = new MutableCurrentUserService(_lawyerUserId);
        var service = CreateService(
            context,
            currentUser,
            new StubCreationGate(
                new ContractCreationFacts(
                    proposalId,
                    legalCaseId,
                    _clientUserId,
                    _lawyerUserId)));
        await AddContractPrerequisitesAsync(
            context,
            proposalId,
            legalCaseId);

        var result = await service.CreateAsync(
            new CreateContractRequest(
                proposalId,
                "عقد تمثيل قانوني",
                "الشروط والأحكام المعتمدة."),
            CancellationToken.None);

        Assert.Equal(proposalId, result.ProposalId);
        Assert.Equal(legalCaseId, result.LegalCaseId);
        Assert.Equal(_clientUserId, result.ClientUserId);
        Assert.Equal(_lawyerUserId, result.LawyerUserId);
        Assert.Equal(ContractStatus.Draft, result.Status);
        var history = await context.ContractStateHistories.SingleAsync();
        Assert.Null(history.PreviousStatus);
        Assert.Equal(ContractStatus.Draft, history.NewStatus);
        Assert.Equal(
            ContractPaymentEventTypes.ContractCreated,
            history.Trigger);
        var outbox = await context.OutboxMessages.SingleAsync();
        Assert.Equal(
            ContractPaymentEventTypes.ContractCreated,
            outbox.EventType);
        Assert.Equal(result.Id, outbox.AggregateId);
    }

    [Fact]
    public async Task CreateAsync_RejectsASecondContractForTheSameProposal()
    {
        await using var context = CreateContext();
        var proposalId = Guid.NewGuid();
        var facts = new ContractCreationFacts(
            proposalId,
            Guid.NewGuid(),
            _clientUserId,
            _lawyerUserId);
        var service = CreateService(
            context,
            new MutableCurrentUserService(_lawyerUserId),
            new StubCreationGate(facts));
        await AddContractPrerequisitesAsync(
            context,
            facts.ProposalId,
            facts.LegalCaseId);
        var request = new CreateContractRequest(
            proposalId,
            "العقد الأول",
            "شروط العقد.");
        await service.CreateAsync(request, CancellationToken.None);

        var exception = await Assert.ThrowsAsync<ConflictException>(() =>
            service.CreateAsync(request, CancellationToken.None));

        Assert.Equal(
            "تم إنشاء عقد لهذا العرض مسبقًا.",
            exception.Message);
        Assert.Single(await context.Contracts.ToListAsync());
    }

    [Fact]
    public async Task UpdateDraftAsync_ClearsBothAcceptances()
    {
        await using var context = CreateContext();
        var contract = CreateContract();
        contract.AcceptedByClientAt = _utcNow.AddMinutes(-5);
        contract.AcceptedByLawyerAt = _utcNow.AddMinutes(-4);
        await AddContractPrerequisitesAsync(
            context,
            contract.ProposalId,
            contract.LegalCaseId);
        context.Contracts.Add(contract);
        await context.SaveChangesAsync();
        var service = CreateService(
            context,
            new MutableCurrentUserService(_lawyerUserId));
        var originalETag = ToETag(contract.RowVersion);

        var result = await service.UpdateDraftAsync(
            contract.Id,
            new UpdateContractRequest(
                "صياغة العقد المعدلة",
                "الشروط المعدلة التي تستلزم قبول الطرفين مجددًا."),
            originalETag,
            CancellationToken.None);

        Assert.Equal("صياغة العقد المعدلة", result.Title);
        Assert.Null(result.AcceptedByClientAt);
        Assert.Null(result.AcceptedByLawyerAt);
        var saved = await context.Contracts.SingleAsync();
        Assert.Null(saved.AcceptedByClientAt);
        Assert.Null(saved.AcceptedByLawyerAt);
        var draftUpdatedEvent = Assert.Single(
            await context.OutboxMessages.Where(item =>
                    item.EventType
                        == ContractPaymentEventTypes.ContractDraftUpdated)
                .ToListAsync());
        Assert.Equal(1, draftUpdatedEvent.EventVersion);
        var draftUpdatedPayload = JsonSerializer.Deserialize<
            ContractDraftUpdatedEventPayload>(
                draftUpdatedEvent.Payload,
                EventJsonOptions);
        Assert.Equal(contract.Id, draftUpdatedPayload!.ContractId);

        var exception = await Assert.ThrowsAsync<ConflictException>(() =>
            service.UpdateDraftAsync(
                contract.Id,
                new UpdateContractRequest(
                    "صياغة أخرى",
                    "هذه محاولة تستخدم نسخة قديمة من العقد."),
                originalETag,
                CancellationToken.None));
        Assert.StartsWith("تم تعديل العقد", exception.Message);
    }

    [Fact]
    public async Task AcceptAsync_ActivatesOnlyAfterBothAcceptAndAMilestoneIsApproved()
    {
        await using var context = CreateContext();
        var contract = CreateContract();
        await AddContractPrerequisitesAsync(
            context,
            contract.ProposalId,
            contract.LegalCaseId);
        context.Contracts.Add(contract);
        await context.SaveChangesAsync();
        var currentUser = new MutableCurrentUserService(_clientUserId);
        var service = CreateService(context, currentUser);

        var firstAcceptance = await service.AcceptAsync(
            contract.Id,
            ToETag(contract.RowVersion),
            CancellationToken.None);

        Assert.Equal(
            ContractStatus.Draft.ToString(),
            firstAcceptance.Status);
        currentUser.UserId = _lawyerUserId;
        var secondAcceptance = await service.AcceptAsync(
            contract.Id,
            ToETag(contract.RowVersion),
            CancellationToken.None);

        Assert.Equal(
            ContractStatus.Draft.ToString(),
            secondAcceptance.Status);
        Assert.Null(contract.ActivatedAt);
        var milestone = CreateMilestone(contract.Id, 1, 1_250m);
        milestone.AcceptedByClientAt = _utcNow.AddMinutes(-2);
        milestone.AcceptedByLawyerAt = _utcNow.AddMinutes(-1);
        context.Milestones.Add(milestone);
        await context.SaveChangesAsync();

        currentUser.UserId = null;
        await ((IContractActivationEvaluator)service)
            .EvaluateActivationAsync(
                contract.Id,
                _lawyerUserId,
                CancellationToken.None);

        Assert.Equal(
            ContractStatus.Active,
            contract.Status);
        Assert.NotNull(contract.ActivatedAt);
        var activationHistory = Assert.Single(
            await context.ContractStateHistories
                .Where(item =>
                    item.Trigger
                    == ContractPaymentEventTypes.ContractActivated)
                .ToListAsync());
        Assert.Equal(_lawyerUserId, activationHistory.ActorUserId);
        Assert.Single(
            await context.OutboxMessages
                .Where(item =>
                    item.EventType
                    == ContractPaymentEventTypes.ContractActivated)
                .ToListAsync());
        var acceptanceEvents = await context.OutboxMessages
            .Where(item =>
                item.EventType == ContractPaymentEventTypes.ContractAccepted)
            .OrderBy(item => item.CreatedAt)
            .ToListAsync();
        Assert.Equal(2, acceptanceEvents.Count);
        Assert.All(acceptanceEvents, item => Assert.Equal(2, item.EventVersion));
        var acceptancePayloads = acceptanceEvents.Select(item =>
                JsonSerializer.Deserialize<
                    ContractAcceptanceRecordedEventPayload>(
                        item.Payload,
                        EventJsonOptions)!)
            .ToArray();
        Assert.Contains(acceptancePayloads, payload =>
            payload.AcceptedByUserId == _clientUserId
            && payload.RequiresCounterpartyAcceptance);
        Assert.Contains(acceptancePayloads, payload =>
            payload.AcceptedByUserId == _lawyerUserId
            && !payload.RequiresCounterpartyAcceptance);
        Assert.Empty(await context.PaymentTransactions.ToListAsync());
        Assert.Empty(await context.EscrowHolds.ToListAsync());

        await ((IContractActivationEvaluator)service)
            .EvaluateActivationAsync(
                contract.Id,
                _lawyerUserId,
                CancellationToken.None);

        Assert.Equal(ContractStatus.Active, contract.Status);
        Assert.Single(
            await context.ContractStateHistories
                .Where(item =>
                    item.Trigger
                    == ContractPaymentEventTypes.ContractActivated)
                .ToListAsync());
    }

    [Fact]
    public async Task AcceptAsync_FinalAcceptanceAtomicallyLinksWinningChatAndAssignsCase()
    {
        await using var context = CreateContext();
        var scenario = await AddActivationScenarioAsync(
            context,
            closeWinningConversation: false);
        var currentUser = new MutableCurrentUserService(_lawyerUserId);
        var service = CreateService(
            context,
            currentUser,
            caseAssignmentService: CreateCaseAssignmentService(context));

        var firstAcceptance = await service.AcceptAsync(
            scenario.Contract.Id,
            ToETag(scenario.Contract.RowVersion),
            CancellationToken.None);
        Assert.Equal(
            ContractStatus.Draft.ToString(),
            firstAcceptance.Status);

        currentUser.UserId = _clientUserId;
        var finalAcceptance = await service.AcceptAsync(
            scenario.Contract.Id,
            ToETag(scenario.Contract.RowVersion),
            CancellationToken.None);
        Assert.Equal(
            ContractStatus.Active.ToString(),
            finalAcceptance.Status);

        await using var verificationContext = CreateContext();
        var savedContract = await verificationContext.Contracts
            .SingleAsync(item => item.Id == scenario.Contract.Id);
        var savedCase = await verificationContext.Cases
            .SingleAsync(item => item.Id == scenario.Contract.LegalCaseId);
        Assert.Equal(ContractStatus.Active, savedContract.Status);
        Assert.NotNull(savedContract.ActivatedAt);
        Assert.Equal(CaseStatus.Assigned, savedCase.Status);
        Assert.Equal(_lawyerUserId, savedCase.LawyerId);
        Assert.Equal(scenario.WinningConversationId, savedCase.ChatId);

        var proposals = await verificationContext.Proposals
            .Where(item => item.LegalCaseId == scenario.Contract.LegalCaseId)
            .ToDictionaryAsync(item => item.Id);
        Assert.Equal(
            ProposalStatus.Accepted,
            proposals[scenario.SelectedProposalId].Status);
        Assert.Equal(
            ProposalStatus.Superseded,
            proposals[scenario.AcceptedCompetitorId].Status);
        Assert.Equal(
            ProposalStatus.Superseded,
            proposals[scenario.PendingCompetitorId].Status);

        Assert.Single(
            await verificationContext.ContractStateHistories
                .Where(item =>
                    item.ContractId == scenario.Contract.Id
                    && item.Trigger
                        == ContractPaymentEventTypes.ContractActivated)
                .ToListAsync());
        Assert.Equal(
            2,
            await verificationContext.OutboxMessages.CountAsync(item =>
                item.EventType
                    == ContractPaymentEventTypes.ContractAccepted));
        Assert.Single(
            await verificationContext.OutboxMessages
                .Where(item =>
                    item.EventType
                        == ContractPaymentEventTypes.ContractActivated)
                .ToListAsync());
        Assert.Equal(
            2,
            await verificationContext.OutboxMessages.CountAsync(item =>
                item.EventType
                    == ContractPaymentEventTypes.ProposalSuperseded));
    }

    [Fact]
    public async Task AcceptAsync_ClosedWinningChatRollsBackFinalAcceptance()
    {
        await using var context = CreateContext();
        var scenario = await AddActivationScenarioAsync(
            context,
            closeWinningConversation: true);
        var currentUser = new MutableCurrentUserService(_lawyerUserId);
        var service = CreateService(
            context,
            currentUser,
            caseAssignmentService: CreateCaseAssignmentService(context));

        await service.AcceptAsync(
            scenario.Contract.Id,
            ToETag(scenario.Contract.RowVersion),
            CancellationToken.None);
        currentUser.UserId = _clientUserId;

        await Assert.ThrowsAsync<BusinessException>(() =>
            service.AcceptAsync(
                scenario.Contract.Id,
                ToETag(scenario.Contract.RowVersion),
                CancellationToken.None));

        await using var verificationContext = CreateContext();
        var savedContract = await verificationContext.Contracts
            .SingleAsync(item => item.Id == scenario.Contract.Id);
        var savedCase = await verificationContext.Cases
            .SingleAsync(item => item.Id == scenario.Contract.LegalCaseId);
        Assert.Equal(ContractStatus.Draft, savedContract.Status);
        Assert.NotNull(savedContract.AcceptedByLawyerAt);
        Assert.Null(savedContract.AcceptedByClientAt);
        Assert.Null(savedContract.ActivatedAt);
        Assert.Equal(CaseStatus.Matched, savedCase.Status);
        Assert.Null(savedCase.LawyerId);
        Assert.Null(savedCase.ChatId);
        Assert.Empty(
            await verificationContext.ContractStateHistories
                .Where(item =>
                    item.ContractId == scenario.Contract.Id
                    && item.Trigger
                        == ContractPaymentEventTypes.ContractActivated)
                .ToListAsync());
        Assert.Single(
            await verificationContext.OutboxMessages
                .Where(item =>
                    item.EventType
                        == ContractPaymentEventTypes.ContractAccepted)
                .ToListAsync());
        Assert.Empty(
            await verificationContext.OutboxMessages
                .Where(item =>
                    item.EventType
                        == ContractPaymentEventTypes.ContractActivated
                    || item.EventType
                        == ContractPaymentEventTypes.ProposalSuperseded)
                .ToListAsync());
        Assert.All(
            await verificationContext.Proposals
                .Where(item =>
                    item.Id == scenario.AcceptedCompetitorId
                    || item.Id == scenario.PendingCompetitorId)
                .ToListAsync(),
            proposal => Assert.NotEqual(
                ProposalStatus.Superseded,
                proposal.Status));
    }

    [Fact]
    public async Task EvaluateActivationAsync_StillRequiresAContractParticipant()
    {
        await using var context = CreateContext();
        var contract = CreateContract();
        await AddContractPrerequisitesAsync(
            context,
            contract.ProposalId,
            contract.LegalCaseId);
        context.Contracts.Add(contract);
        await context.SaveChangesAsync();
        var service = CreateService(
            context,
            new MutableCurrentUserService(Guid.NewGuid()));

        await Assert.ThrowsAsync<ForbiddenAccessException>(() =>
            service.EvaluateActivationAsync(
                contract.Id,
                CancellationToken.None));
    }

    [Fact]
    public async Task GetAsync_DerivesTotalFromMutuallyApprovedNonCancelledMilestones()
    {
        await using var context = CreateContext();
        var contract = CreateContract();
        var approved = CreateMilestone(contract.Id, 1, 500m);
        approved.AcceptedByClientAt = _utcNow;
        approved.AcceptedByLawyerAt = _utcNow;
        var oneSided = CreateMilestone(contract.Id, 2, 300m);
        oneSided.AcceptedByClientAt = _utcNow;
        var cancelled = CreateMilestone(contract.Id, 3, 200m);
        cancelled.AcceptedByClientAt = _utcNow;
        cancelled.AcceptedByLawyerAt = _utcNow;
        cancelled.Status = MilestoneStatus.Cancelled;
        await AddContractPrerequisitesAsync(
            context,
            contract.ProposalId,
            contract.LegalCaseId);
        context.AddRange(contract, approved, oneSided, cancelled);
        await context.SaveChangesAsync();
        var service = CreateService(
            context,
            new MutableCurrentUserService(_clientUserId));

        var result = await service.GetAsync(
            contract.Id,
            CancellationToken.None);

        Assert.Equal(500m, result.CurrentMilestoneTotal);
        Assert.Equal(3, result.Milestones.Count);
    }

    [Fact]
    public async Task GetAsync_AllowsParticipantsAndModeratorsOnly()
    {
        await using var context = CreateContext();
        var contract = CreateContract();
        await AddContractPrerequisitesAsync(
            context,
            contract.ProposalId,
            contract.LegalCaseId);
        context.Contracts.Add(contract);
        await context.SaveChangesAsync();
        var outsiderId = Guid.NewGuid();
        context.Users.Add(CreateUser(outsiderId, "contract-moderator"));
        await context.SaveChangesAsync();
        var currentUser = new MutableCurrentUserService(outsiderId);
        var eligibility = new StubEligibilityService();
        var queryService = CreateQueryService(
            context,
            currentUser,
            eligibility);
        var service = CreateService(
            context,
            currentUser,
            eligibilityService: eligibility);

        var exception =
            await Assert.ThrowsAsync<ForbiddenAccessException>(() =>
                queryService.GetAsync(contract.Id, CancellationToken.None));

        Assert.Equal(
            "غير مصرح لك بالاطلاع على هذا العقد.",
            exception.Message);
        var outsiderList = await queryService.ListAsync(
            new ContractListQuery(),
            CancellationToken.None);
        Assert.Empty(outsiderList.Items);
        eligibility.Results[outsiderId] =
            new ContractUserEligibilityFacts(
                outsiderId,
                IsActive: true,
                CanActAsClient: false,
                CanActAsLawyer: false,
                CanActAsModerator: true,
                CanActAsFinanceAdministrator: false,
                CanActAsSuperAdministrator: false);

        var result = await queryService.GetAsync(
            contract.Id,
            CancellationToken.None);
        var moderatorList = await queryService.ListAsync(
            new ContractListQuery(),
            CancellationToken.None);

        Assert.Equal(contract.Id, result.Id);
        Assert.Contains(
            moderatorList.Items,
            item => item.Id == contract.Id);
    }

    [Fact]
    public async Task EvaluateCompletionAsync_CompletesSettledActiveContractOnce()
    {
        await using var context = CreateContext();
        var contract = CreateContract();
        contract.Status = ContractStatus.Active;
        contract.ActivatedAt = _utcNow.AddDays(-3);
        var milestone = CreateMilestone(contract.Id, 1, 750m);
        milestone.Status = MilestoneStatus.Released;
        milestone.ReleasedAt = _utcNow.AddMinutes(-10);
        milestone.AcceptedByClientAt = _utcNow.AddDays(-2);
        milestone.AcceptedByLawyerAt = _utcNow.AddDays(-2);
        await AddContractPrerequisitesAsync(
            context,
            contract.ProposalId,
            contract.LegalCaseId);
        context.AddRange(contract, milestone);
        await context.SaveChangesAsync();
        var service = CreateService(
            context,
            new MutableCurrentUserService(_clientUserId));

        var result = await service.EvaluateCompletionAsync(
            contract.Id,
            CancellationToken.None);

        Assert.Equal(
            ContractStatus.Completed.ToString(),
            result.Status);
        Assert.NotNull(contract.CompletedAt);
        Assert.Single(
            await context.ContractStateHistories
                .Where(item =>
                    item.Trigger
                    == ContractPaymentEventTypes.ContractCompleted)
                .ToListAsync());

        await service.EvaluateCompletionAsync(
            contract.Id,
            CancellationToken.None);

        Assert.Single(
            await context.OutboxMessages
                .Where(item =>
                    item.EventType
                    == ContractPaymentEventTypes.ContractCompleted)
                .ToListAsync());
    }

    [Fact]
    public async Task EvaluateCompletionAsync_AllowsUnauthenticatedBackgroundJob()
    {
        await using var context = CreateContext();
        var contract = CreateContract();
        contract.Status = ContractStatus.Active;
        contract.ActivatedAt = _utcNow.AddDays(-3);
        var milestone = CreateMilestone(contract.Id, 1, 750m);
        milestone.Status = MilestoneStatus.Released;
        milestone.ReleasedAt = _utcNow.AddMinutes(-10);
        milestone.AcceptedByClientAt = _utcNow.AddDays(-2);
        milestone.AcceptedByLawyerAt = _utcNow.AddDays(-2);
        await AddContractPrerequisitesAsync(
            context,
            contract.ProposalId,
            contract.LegalCaseId);
        context.AddRange(contract, milestone);
        await context.SaveChangesAsync();
        var currentUser = new MutableCurrentUserService(_clientUserId)
        {
            UserId = null
        };
        var service = CreateService(context, currentUser);

        var result = await service.EvaluateCompletionAsync(
            contract.Id,
            CancellationToken.None);

        Assert.Equal(ContractStatus.Completed.ToString(), result.Status);
        var history = await context.ContractStateHistories.SingleAsync(
            item => item.Trigger == ContractPaymentEventTypes.ContractCompleted);
        Assert.Null(history.ActorUserId);
    }

    [Fact]
    public async Task EvaluateCompletionAsync_DoesNotCompleteWithPendingProviderAttempt()
    {
        await using var context = CreateContext();
        var contract = CreateContract();
        contract.Status = ContractStatus.Active;
        var milestone = CreateMilestone(contract.Id, 1, 750m);
        milestone.Status = MilestoneStatus.Released;
        milestone.AcceptedByClientAt = _utcNow.AddDays(-2);
        milestone.AcceptedByLawyerAt = _utcNow.AddDays(-2);
        var paymentTransaction = new PaymentTransaction(
            Guid.NewGuid(),
            contract.Id,
            milestone.Id,
            PaymentOperationType.Release,
            "TestProvider",
            $"release-{Guid.NewGuid():N}",
            750m,
            _utcNow.AddMinutes(-5));
        await AddContractPrerequisitesAsync(
            context,
            contract.ProposalId,
            contract.LegalCaseId);
        context.AddRange(contract, milestone, paymentTransaction);
        await context.SaveChangesAsync();
        var service = CreateService(
            context,
            new MutableCurrentUserService(_clientUserId));

        var result = await service.EvaluateCompletionAsync(
            contract.Id,
            CancellationToken.None);

        Assert.Equal(ContractStatus.Active.ToString(), result.Status);
        Assert.Empty(
            await context.ContractStateHistories
                .Where(item =>
                    item.Trigger
                    == ContractPaymentEventTypes.ContractCompleted)
                .ToListAsync());
    }

    [Fact]
    public async Task EvaluateCompletionAsync_DoesNotCompleteWithUnsettledHold()
    {
        await using var context = CreateContext();
        var contract = CreateContract();
        contract.Status = ContractStatus.Active;
        var milestone = CreateMilestone(contract.Id, 1, 750m);
        milestone.Status = MilestoneStatus.Released;
        milestone.AcceptedByClientAt = _utcNow.AddDays(-2);
        milestone.AcceptedByLawyerAt = _utcNow.AddDays(-2);
        var account = new EscrowAccount(
            Guid.NewGuid(),
            contract.Id,
            _utcNow.AddDays(-2))
        {
            TotalDeposited = 750m
        };
        var hold = new EscrowHold(
            Guid.NewGuid(),
            account.Id,
            contract.Id,
            milestone.Id,
            750m,
            37.5m,
            712.5m,
            Guid.NewGuid(),
            _utcNow.AddDays(-1),
            _utcNow.AddDays(-1));
        await AddContractPrerequisitesAsync(
            context,
            contract.ProposalId,
            contract.LegalCaseId);
        context.AddRange(contract, milestone, account, hold);
        await context.SaveChangesAsync();
        var service = CreateService(
            context,
            new MutableCurrentUserService(_clientUserId));

        var result = await service.EvaluateCompletionAsync(
            contract.Id,
            CancellationToken.None);

        Assert.Equal(ContractStatus.Active.ToString(), result.Status);
        Assert.Empty(
            await context.ContractStateHistories
                .Where(item =>
                    item.Trigger
                    == ContractPaymentEventTypes.ContractCompleted)
                .ToListAsync());
    }

    [Fact]
    public async Task EvaluateCompletionAsync_DoesNotCompleteWithUnapprovedFutureMilestone()
    {
        await using var context = CreateContext();
        var contract = CreateContract();
        contract.Status = ContractStatus.Active;
        var approvedMilestone = CreateMilestone(contract.Id, 1, 750m);
        approvedMilestone.Status = MilestoneStatus.Released;
        approvedMilestone.AcceptedByClientAt = _utcNow.AddDays(-2);
        approvedMilestone.AcceptedByLawyerAt = _utcNow.AddDays(-2);
        var futureMilestone = CreateMilestone(contract.Id, 2, 500m);
        await AddContractPrerequisitesAsync(
            context,
            contract.ProposalId,
            contract.LegalCaseId);
        context.AddRange(
            contract,
            approvedMilestone,
            futureMilestone);
        await context.SaveChangesAsync();
        var service = CreateService(
            context,
            new MutableCurrentUserService(_clientUserId));

        var result = await service.EvaluateCompletionAsync(
            contract.Id,
            CancellationToken.None);

        Assert.Equal(ContractStatus.Active.ToString(), result.Status);
        Assert.Null(contract.CompletedAt);
        Assert.Empty(
            await context.ContractStateHistories
                .Where(item =>
                    item.Trigger
                    == ContractPaymentEventTypes.ContractCompleted)
                .ToListAsync());
    }

    [Fact]
    public async Task TerminateAsync_CancelsFutureMilestonesAndWritesAtomicAudit()
    {
        await using var context = CreateContext();
        var contract = CreateContract();
        var draftMilestone = CreateMilestone(contract.Id, 1, 900m);
        var awaitingFunding = CreateMilestone(contract.Id, 2, 400m);
        awaitingFunding.Status = MilestoneStatus.AwaitingFunding;
        await AddContractPrerequisitesAsync(
            context,
            contract.ProposalId,
            contract.LegalCaseId);
        context.AddRange(contract, draftMilestone, awaitingFunding);
        await context.SaveChangesAsync();
        var service = CreateService(
            context,
            new MutableCurrentUserService(_clientUserId));

        var result = await service.TerminateAsync(
            contract.Id,
            new TerminateContractRequest(
                "اتفق الطرفان على عدم الاستمرار في التعاقد."),
            ToETag(contract.RowVersion),
            CancellationToken.None);

        Assert.Equal(ContractStatus.Terminated, result.Status);
        Assert.Equal(_clientUserId, contract.TerminatedByUserId);
        Assert.All(
            await context.Milestones.ToListAsync(),
            item => Assert.Equal(
                MilestoneStatus.Cancelled,
                item.Status));
        Assert.Single(
            await context.ContractStateHistories
                .Where(item =>
                    item.Trigger
                    == ContractPaymentEventTypes.ContractTerminated)
                .ToListAsync());
        var terminationEvent = Assert.Single(
            await context.OutboxMessages
                .Where(item =>
                    item.EventType
                    == ContractPaymentEventTypes.ContractTerminated)
                .ToListAsync());
        Assert.Contains(
            contract.LegalCaseId.ToString(),
            terminationEvent.Payload,
            StringComparison.OrdinalIgnoreCase);
        var requestEvent = Assert.Single(
            await context.OutboxMessages.Where(item =>
                    item.EventType
                        == ContractPaymentEventTypes.ContractTerminationRequested)
                .ToListAsync());
        var requestPayload = JsonSerializer.Deserialize<
            ContractTerminationRequestedEventPayload>(
                requestEvent.Payload,
                EventJsonOptions);
        Assert.Equal(contract.Id, requestPayload!.ContractId);
        Assert.Equal(_clientUserId, requestPayload.RequestedByUserId);
    }

    [Fact]
    public async Task TerminateAsync_BlocksUnreconciledProviderDeposit()
    {
        await using var context = CreateContext();
        var contract = CreateContract();
        var milestone = CreateMilestone(contract.Id, 1, 900m);
        milestone.Status = MilestoneStatus.AwaitingFunding;
        var paymentTransaction = new PaymentTransaction(
            Guid.NewGuid(),
            contract.Id,
            milestone.Id,
            PaymentOperationType.Deposit,
            "TestProvider",
            $"deposit-{Guid.NewGuid():N}",
            milestone.Amount,
            _utcNow.AddMinutes(-10));
        await AddContractPrerequisitesAsync(
            context,
            contract.ProposalId,
            contract.LegalCaseId);
        context.AddRange(contract, milestone, paymentTransaction);
        await context.SaveChangesAsync();
        var service = CreateService(
            context,
            new MutableCurrentUserService(_clientUserId),
            terminationSettlementServices:
            [new PendingTerminationSettlementService()]);

        var exception = await Assert.ThrowsAsync<ConflictException>(() =>
            service.TerminateAsync(
                contract.Id,
                new TerminateContractRequest(
                    "طلب إنهاء العقد بعد حسم عملية الدفع."),
                ToETag(contract.RowVersion),
                CancellationToken.None));

        Assert.Equal(
            "تم تسجيل طلب إنهاء العقد، وتستمر محاولة إتمام التسوية المالية تلقائيًا.",
            exception.Message);
        Assert.NotEqual(ContractStatus.Terminated, contract.Status);
        Assert.Equal(_clientUserId, contract.TerminatedByUserId);
        Assert.NotNull(contract.TerminationReason);
        Assert.Equal(MilestoneStatus.AwaitingFunding, milestone.Status);
        Assert.Single(
            await context.OutboxMessages.Where(item =>
                    item.EventType
                        == ContractPaymentEventTypes.ContractTerminationRequested)
                .ToListAsync());
        Assert.Empty(
            await context.OutboxMessages.Where(item =>
                    item.EventType
                        == ContractPaymentEventTypes.ContractTerminated)
                .ToListAsync());
    }

    [Fact]
    public async Task RecoverPendingTerminationsAsync_FinalizesPersistedIntent()
    {
        await using var context = CreateContext();
        var contract = CreateContract();
        contract.Status = ContractStatus.Active;
        contract.ActivatedAt = _utcNow.AddDays(-1);
        contract.TerminationReason =
            "طلب إنهاء محفوظ بانتظار الاسترداد التلقائي.";
        contract.TerminatedByUserId = _clientUserId;
        var milestone = CreateMilestone(contract.Id, 1, 900m);
        await AddContractPrerequisitesAsync(
            context,
            contract.ProposalId,
            contract.LegalCaseId);
        context.AddRange(contract, milestone);
        await context.SaveChangesAsync();
        var service = CreateService(
            context,
            new MutableCurrentUserService(_clientUserId));

        var result = await service.RecoverPendingTerminationsAsync(
            CancellationToken.None);

        Assert.Equal(JobExecutionOutcome.Completed, result.Outcome);
        Assert.Equal(1, result.AffectedCount);
        var savedContract = await context.Contracts.SingleAsync();
        var savedMilestone = await context.Milestones.SingleAsync();
        Assert.Equal(ContractStatus.Terminated, savedContract.Status);
        Assert.NotNull(savedContract.TerminatedAt);
        Assert.Equal(MilestoneStatus.Cancelled, savedMilestone.Status);
        Assert.Single(
            await context.OutboxMessages.Where(item =>
                    item.EventType
                        == ContractPaymentEventTypes.ContractTerminated)
                .ToListAsync());
    }

    [Fact]
    public async Task TerminateAsync_RefundsEligibleHoldBeforeTerminating()
    {
        await using var context = CreateContext();
        var contract = CreateContract();
        contract.Status = ContractStatus.Active;
        contract.ActivatedAt = _utcNow.AddDays(-1);
        var milestone = CreateMilestone(contract.Id, 1, 1_000m);
        milestone.Status = MilestoneStatus.FundedInProgress;
        milestone.FundedAt = _utcNow.AddHours(-1);
        var account = new EscrowAccount(
            Guid.NewGuid(),
            contract.Id,
            _utcNow.AddHours(-1))
        {
            TotalDeposited = 1_000m
        };
        var hold = new EscrowHold(
            Guid.NewGuid(),
            account.Id,
            contract.Id,
            milestone.Id,
            1_000m,
            50m,
            950m,
            Guid.NewGuid(),
            _utcNow.AddHours(-1),
            _utcNow.AddHours(-1));
        var wallet = new LawyerWallet(
            Guid.NewGuid(),
            _lawyerUserId,
            _utcNow.AddHours(-1))
        {
            PendingBalance = 950m
        };
        await AddContractPrerequisitesAsync(
            context,
            contract.ProposalId,
            contract.LegalCaseId);
        context.AddRange(contract, milestone, account, hold, wallet);
        await context.SaveChangesAsync();
        var timeProvider = new FixedTimeProvider(_utcNow);
        var outboxWriter = new OutboxWriter(context, timeProvider);
        var settlementService =
            new ContractTerminationSettlementService(
                context,
                new SuccessfulRefundProvider(),
                outboxWriter,
                timeProvider,
                NullLogger<
                    ContractTerminationSettlementService>.Instance);
        var service = CreateService(
            context,
            new MutableCurrentUserService(_clientUserId),
            terminationSettlementServices: [settlementService]);

        var result = await service.TerminateAsync(
            contract.Id,
            new TerminateContractRequest(
                "اتفق الطرفان على إنهاء العقد ورد تمويل المرحلة غير المنفذة."),
            ToETag(contract.RowVersion),
            CancellationToken.None);

        Assert.Equal(ContractStatus.Terminated, result.Status);
        Assert.Equal(
            MilestoneStatus.Refunded,
            (await context.Milestones.SingleAsync()).Status);
        Assert.Equal(
            EscrowHoldStatus.Refunded,
            (await context.EscrowHolds.SingleAsync()).Status);
        Assert.Equal(
            1_000m,
            (await context.EscrowAccounts.SingleAsync()).TotalRefunded);
        Assert.Equal(
            0m,
            (await context.LawyerWallets.SingleAsync()).PendingBalance);
        Assert.Equal(
            2,
            await context.OutboxMessages.CountAsync(item =>
                item.EventType
                    == ContractPaymentEventTypes.FundsRefunded
                || item.EventType
                    == ContractPaymentEventTypes.ContractTerminated));
    }

    private ContractService CreateService(
        ApplicationDbContext context,
        MutableCurrentUserService currentUser,
        IContractCreationDependencyGate? creationGate = null,
        IContractUserEligibilityService? eligibilityService = null,
        IEnumerable<IContractTerminationSettlementService>?
            terminationSettlementServices = null,
        IContractCaseAssignmentService? caseAssignmentService = null)
    {
        var timeProvider = new FixedTimeProvider(_utcNow);
        var eligibility = eligibilityService ?? new StubEligibilityService();
        var queryService = new ContractQueryService(
            context,
            currentUser,
            eligibility);
        return new ContractService(
            context,
            currentUser,
            creationGate ?? new StubCreationGate(
                new ContractCreationFacts(
                    Guid.NewGuid(),
                    Guid.NewGuid(),
                    _clientUserId,
                    _lawyerUserId)),
            eligibility,
            queryService,
            new OutboxWriter(context, timeProvider),
            caseAssignmentService ?? new NoOpCaseAssignmentService(),
            terminationSettlementServices
                ?? Array.Empty<IContractTerminationSettlementService>(),
            timeProvider);
    }

    private IContractCaseAssignmentService CreateCaseAssignmentService(
        ApplicationDbContext context)
    {
        var timeProvider = new FixedTimeProvider(_utcNow);
        return new ContractCaseAssignmentService(
            context,
            new ChatConversationService(context, timeProvider),
            new OutboxWriter(context, timeProvider));
    }

    private ContractQueryService CreateQueryService(
        ApplicationDbContext context,
        MutableCurrentUserService currentUser,
        IContractUserEligibilityService? eligibilityService = null)
    {
        return new ContractQueryService(
            context,
            currentUser,
            eligibilityService ?? new StubEligibilityService());
    }

    private Contract CreateContract()
    {
        return new Contract(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            _clientUserId,
            _lawyerUserId,
            "عقد تمثيل قانوني",
            "الشروط والأحكام.",
            _utcNow.AddHours(-1));
    }

    private async Task AddContractPrerequisitesAsync(
        ApplicationDbContext context,
        Guid proposalId,
        Guid legalCaseId)
    {
        var caseEntity = new SmartCourt.Entities.Case { Id = legalCaseId, ClientId = _clientUserId, Title = "قضية اختبار العقد", Description = "قضية مؤهلة لاختبار دورة حياة العقد.", City = "القاهرة", SubmittedAt = _utcNow.AddDays(-2), Status = CaseStatus.Matched };
        var proposal = new Proposal(
            proposalId,
            legalCaseId,
            _clientUserId,
            _lawyerUserId,
            _utcNow.AddDays(-1))
        {
            Status = ProposalStatus.Accepted
        };
        context.AddRange(caseEntity, proposal);
        await context.SaveChangesAsync();
    }

    private async Task<ActivationScenario> AddActivationScenarioAsync(
        ApplicationDbContext context,
        bool closeWinningConversation)
    {
        var contract = CreateContract();
        await AddContractPrerequisitesAsync(
            context,
            contract.ProposalId,
            contract.LegalCaseId);

        var acceptedCompetitorLawyerId = Guid.NewGuid();
        var pendingCompetitorLawyerId = Guid.NewGuid();
        context.Users.AddRange(
            CreateUser(
                acceptedCompetitorLawyerId,
                "accepted-competitor-lawyer"),
            CreateUser(
                pendingCompetitorLawyerId,
                "pending-competitor-lawyer"));
        context.Set<SmartCourt.Common.Entities.LawyerProfile>().AddRange(
            new SmartCourt.Common.Entities.LawyerProfile
            {
                UserId = acceptedCompetitorLawyerId
            },
            new SmartCourt.Common.Entities.LawyerProfile
            {
                UserId = pendingCompetitorLawyerId
            });

        var winningConversation = new ChatConversation(
            Guid.NewGuid(),
            contract.ProposalId,
            contract.LegalCaseId,
            _clientUserId,
            _lawyerUserId,
            _utcNow.AddHours(-1));
        if (closeWinningConversation)
        {
            winningConversation.Close(_utcNow.AddMinutes(-45));
        }

        var acceptedCompetitor = new Proposal(
            Guid.NewGuid(),
            contract.LegalCaseId,
            _clientUserId,
            acceptedCompetitorLawyerId,
            _utcNow.AddHours(-2));
        acceptedCompetitor.Accept(_utcNow.AddHours(-1));
        var pendingCompetitor = new Proposal(
            Guid.NewGuid(),
            contract.LegalCaseId,
            _clientUserId,
            pendingCompetitorLawyerId,
            _utcNow.AddHours(-2));
        var approvedMilestone = CreateMilestone(
            contract.Id,
            1,
            1_250m);
        approvedMilestone.AcceptedByClientAt = _utcNow.AddMinutes(-10);
        approvedMilestone.AcceptedByLawyerAt = _utcNow.AddMinutes(-5);

        context.AddRange(
            contract,
            winningConversation,
            acceptedCompetitor,
            pendingCompetitor,
            approvedMilestone);
        await context.SaveChangesAsync();

        return new ActivationScenario(
            contract,
            winningConversation.Id,
            contract.ProposalId,
            acceptedCompetitor.Id,
            pendingCompetitor.Id);
    }

    private Milestone CreateMilestone(
        Guid contractId,
        int orderNumber,
        decimal amount)
    {
        return new Milestone(
            Guid.NewGuid(),
            contractId,
            $"المرحلة {orderNumber}",
            null,
            orderNumber,
            amount,
            null,
            null,
            _utcNow.AddMinutes(-30));
    }

    private ApplicationDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlServer(ConnectionString)
            .Options;
        return new ApplicationDbContext(
            options,
            new FixedTimeProvider(_utcNow));
    }

    private static ApplicationUser CreateUser(
        Guid id,
        string userName)
    {
        return new ApplicationUser
        {
            Id = id,
            UserName = userName,
            NormalizedUserName = userName.ToUpperInvariant(),
            Email = $"{userName}@example.test",
            NormalizedEmail = $"{userName}@example.test"
                .ToUpperInvariant(),
            FullName = userName,
            NationalNumber = id.ToString("N")[..14]
        };
    }

    private static string ToETag(byte[] rowVersion)
    {
        return $"\"{Convert.ToBase64String(rowVersion)}\"";
    }

    private string ConnectionString =>
        Environment.GetEnvironmentVariable("TEST_DB_CONNECTION_STRING")
        ?? $"Server=(localdb)\\mssqllocaldb;Database={_databaseName};Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=True;";

    private sealed class MutableCurrentUserService(Guid userId)
        : ICurrentUserService
    {
        public Guid? UserId { get; set; } = userId;
        public bool IsAuthenticated => UserId.HasValue;
    }

    private sealed class StubCreationGate(ContractCreationFacts facts)
        : IContractCreationDependencyGate
    {
        public Task<ContractCreationFacts> VerifyAsync(
            Guid proposalId,
            Guid actorUserId,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult(facts);
        }
    }

    private sealed class StubEligibilityService
        : IContractUserEligibilityService
    {
        public Dictionary<Guid, ContractUserEligibilityFacts> Results { get; }
            = [];

        public Task<ContractUserEligibilityFacts?> FindEligibilityAsync(
            Guid userId,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            Results.TryGetValue(userId, out var result);
            return Task.FromResult(result);
        }
    }

    private sealed class NoOpCaseAssignmentService
        : IContractCaseAssignmentService
    {
        public Task AssignAsync(
            ContractCaseAssignment assignment,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.CompletedTask;
        }
    }

    private sealed record ActivationScenario(
        Contract Contract,
        Guid WinningConversationId,
        Guid SelectedProposalId,
        Guid AcceptedCompetitorId,
        Guid PendingCompetitorId);

    private sealed class SuccessfulRefundProvider : IPaymentProvider
    {
        public Task<ProviderResult> RefundAsync(
            ProviderRefundRequest request,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult(
                new ProviderResult(
                    request.Amount,
                    request.Currency,
                    request.BusinessId,
                    request.ProviderIdempotencyKey,
                    request.CorrelationId,
                    ProviderOperationOutcome.Succeeded,
                    $"refund-{Guid.NewGuid():N}",
                    null));
        }

        public Task<ProviderResult> DepositAsync(
            ProviderDepositRequest request,
            CancellationToken cancellationToken)
            => throw new NotSupportedException();

        public Task<ProviderResult> RetryDepositAsync(
            ProviderDepositRetryRequest request,
            CancellationToken cancellationToken)
            => throw new NotSupportedException();

        public Task<ProviderResult> ReleaseAsync(
            ProviderReleaseRequest request,
            CancellationToken cancellationToken)
            => throw new NotSupportedException();

        public Task<ProviderResult> WithdrawAsync(
            ProviderWithdrawalRequest request,
            CancellationToken cancellationToken)
            => throw new NotSupportedException();
    }

    private sealed class PendingTerminationSettlementService
        : IContractTerminationSettlementService
    {
        public Task<ContractTerminationSettlement>
            SettleForTerminationAsync(
                Guid contractId,
                Guid actorUserId,
                string reason,
                Guid correlationId,
                CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult(
                new ContractTerminationSettlement(
                    false,
                    0m,
                    0m,
                    0m,
                    0m));
        }
    }

    private sealed class FixedTimeProvider(DateTime utcNow)
        : TimeProvider
    {
        public override DateTimeOffset GetUtcNow()
        {
            return new DateTimeOffset(utcNow);
        }
    }
}

