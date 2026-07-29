using Microsoft.EntityFrameworkCore;
using SmartCourt.Common.Exceptions;
using SmartCourt.Features.Cases.Entities;
using SmartCourt.Features.Cases.Enums;
using SmartCourt.Features.Contracts;
using SmartCourt.Features.Contracts.Dependencies;
using SmartCourt.Features.Contracts.DTOs;
using SmartCourt.Features.Contracts.Entities;
using SmartCourt.Features.Contracts.Enums;
using SmartCourt.Features.Milestones.Entities;
using SmartCourt.Features.Milestones.Enums;
using SmartCourt.Features.Payments.Integration;
using SmartCourt.Features.Proposals.Entities;
using SmartCourt.Features.Proposals.Enums;
using SmartCourt.Features.Users.Integration;
using SmartCourt.Infrastructure.Providers.Events;
using SmartCourt.Interfaces;
using SmartCourt.Persistence;
using Xunit;

namespace SmartCourt.Tests.Features.Contracts;

public sealed class ContractServiceIntegrationTests : IAsyncLifetime
{
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

        var activation = await service.EvaluateActivationAsync(
            contract.Id,
            CancellationToken.None);

        Assert.Equal(
            ContractStatus.Active.ToString(),
            activation.Status);
        Assert.NotNull(contract.ActivatedAt);
        Assert.Single(
            await context.ContractStateHistories
                .Where(item =>
                    item.Trigger
                    == ContractPaymentEventTypes.ContractActivated)
                .ToListAsync());
        Assert.Single(
            await context.OutboxMessages
                .Where(item =>
                    item.EventType
                    == ContractPaymentEventTypes.ContractActivated)
                .ToListAsync());
        Assert.Empty(await context.PaymentTransactions.ToListAsync());
        Assert.Empty(await context.EscrowHolds.ToListAsync());

        var repeatedEvaluation = await service.EvaluateActivationAsync(
            contract.Id,
            CancellationToken.None);

        Assert.Equal(
            ContractStatus.Active.ToString(),
            repeatedEvaluation.Status);
        Assert.Single(
            await context.ContractStateHistories
                .Where(item =>
                    item.Trigger
                    == ContractPaymentEventTypes.ContractActivated)
                .ToListAsync());
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
        var service = CreateService(
            context,
            currentUser,
            eligibilityService: eligibility);

        var exception =
            await Assert.ThrowsAsync<ForbiddenAccessException>(() =>
                service.GetAsync(contract.Id, CancellationToken.None));

        Assert.Equal(
            "غير مصرح لك بالاطلاع على هذا العقد.",
            exception.Message);
        var outsiderList = await service.ListAsync(
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

        var result = await service.GetAsync(
            contract.Id,
            CancellationToken.None);
        var moderatorList = await service.ListAsync(
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
        Assert.Single(
            await context.OutboxMessages
                .Where(item =>
                    item.EventType
                    == ContractPaymentEventTypes.ContractTerminated)
                .ToListAsync());
    }

    private ContractService CreateService(
        ApplicationDbContext context,
        MutableCurrentUserService currentUser,
        IContractCreationDependencyGate? creationGate = null,
        IContractUserEligibilityService? eligibilityService = null)
    {
        var timeProvider = new FixedTimeProvider(_utcNow);
        return new ContractService(
            context,
            currentUser,
            creationGate ?? new StubCreationGate(
                new ContractCreationFacts(
                    Guid.NewGuid(),
                    Guid.NewGuid(),
                    _clientUserId,
                    _lawyerUserId)),
            eligibilityService ?? new StubEligibilityService(),
            new OutboxWriter(context, timeProvider),
            Array.Empty<IContractTerminationSettlementService>(),
            timeProvider);
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
        var legalCase = new LegalCase(
            legalCaseId,
            _clientUserId,
            "قضية اختبار العقد",
            "قضية مؤهلة لاختبار دورة حياة العقد.",
            "القاهرة",
            _utcNow.AddDays(-2))
        {
            Status = CaseStatus.Matched,
            FinalSubmittedAt = _utcNow.AddDays(-2)
        };
        var proposal = new Proposal(
            proposalId,
            legalCaseId,
            _clientUserId,
            _lawyerUserId,
            _utcNow.AddDays(-1))
        {
            Status = ProposalStatus.Accepted
        };
        context.AddRange(legalCase, proposal);
        await context.SaveChangesAsync();
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
        $"Server=localhost;Database={_databaseName};Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=True;";

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

    private sealed class FixedTimeProvider(DateTime utcNow)
        : TimeProvider
    {
        public override DateTimeOffset GetUtcNow()
        {
            return new DateTimeOffset(utcNow);
        }
    }
}
