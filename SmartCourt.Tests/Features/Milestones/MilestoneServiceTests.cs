using Microsoft.EntityFrameworkCore;
using SmartCourt.Common.Exceptions;
using SmartCourt.Features.Contracts;
using SmartCourt.Features.Contracts.DTOs;
using SmartCourt.Features.Contracts.Enums;
using SmartCourt.Features.Milestones;
using SmartCourt.Features.Milestones.DTOs;
using SmartCourt.Features.Milestones.Entities;
using SmartCourt.Features.Milestones.Enums;
using SmartCourt.Features.Payments.Entities;
using SmartCourt.Infrastructure.Providers.Events;
using SmartCourt.Interfaces;
using SmartCourt.Persistence;
using Xunit;

namespace SmartCourt.Tests.Features.Milestones;

public sealed class MilestoneServiceTests
{
    private readonly DateTime _utcNow =
        new(2026, 8, 12, 9, 0, 0, DateTimeKind.Utc);
    private readonly Guid _contractId = Guid.NewGuid();
    private readonly Guid _clientUserId = Guid.NewGuid();
    private readonly Guid _lawyerUserId = Guid.NewGuid();

    [Fact]
    public async Task AddAsync_RequiresTheNextSequentialOrder()
    {
        await using var context = CreateContext();
        var currentUser = new MutableCurrentUser(_clientUserId);
        var contracts = CreateContractStub(ContractStatus.Draft);
        var service = CreateService(context, currentUser, contracts);

        var created = await service.AddAsync(
            _contractId,
            ValidAddRequest(1),
            CancellationToken.None);

        var exception = await Assert.ThrowsAsync<BusinessException>(() =>
            service.AddAsync(
                _contractId,
                ValidAddRequest(3),
                CancellationToken.None));
        Assert.Equal(1, created.OrderNumber);
        Assert.Contains("2", exception.Message);
        Assert.Single(await context.Milestones.ToListAsync());
    }

    [Fact]
    public async Task UpdateDraftAsync_ResetsBothApprovals()
    {
        await using var context = CreateContext();
        var milestone = CreateMilestone(MilestoneStatus.Draft, 1);
        milestone.AcceptedByClientAt = _utcNow.AddMinutes(-2);
        milestone.AcceptedByLawyerAt = _utcNow.AddMinutes(-1);
        await AddMilestonesAsync(context, milestone);
        var service = CreateService(
            context,
            new MutableCurrentUser(_lawyerUserId),
            CreateContractStub(ContractStatus.Draft));

        var result = await service.UpdateDraftAsync(
            _contractId,
            milestone.Id,
            new UpdateMilestoneRequest(
                "Updated filing",
                "Updated description",
                21,
                _utcNow.AddDays(21)),
            ToETag(milestone.RowVersion),
            CancellationToken.None);

        Assert.Equal("Updated filing", result.Title);
        Assert.Null(milestone.AcceptedByClientAt);
        Assert.Null(milestone.AcceptedByLawyerAt);
        Assert.Equal(MilestoneStatus.Draft, milestone.Status);
    }

    [Fact]
    public async Task ApproveAsync_TransitionsOnlyAfterBothParticipantsApprove()
    {
        await using var context = CreateContext();
        var milestone = CreateMilestone(MilestoneStatus.Draft, 1);
        await AddMilestonesAsync(context, milestone);
        var currentUser = new MutableCurrentUser(_clientUserId);
        var contracts = CreateContractStub(ContractStatus.Draft);
        var service = CreateService(context, currentUser, contracts);

        var clientApproval = await service.ApproveAsync(
            milestone.Id,
            ToETag(milestone.RowVersion),
            CancellationToken.None);

        Assert.Equal(
            MilestoneStatus.Draft.ToString(),
            clientApproval.Status);
        Assert.NotNull(milestone.AcceptedByClientAt);
        Assert.Null(milestone.AcceptedByLawyerAt);
        Assert.Empty(await context.MilestoneStateHistories.ToListAsync());

        currentUser.UserId = _lawyerUserId;
        var lawyerApproval = await service.ApproveAsync(
            milestone.Id,
            ToETag(milestone.RowVersion),
            CancellationToken.None);

        Assert.Equal(
            MilestoneStatus.AwaitingFunding.ToString(),
            lawyerApproval.Status);
        Assert.Equal(MilestoneStatus.AwaitingFunding, milestone.Status);
        Assert.Single(await context.MilestoneStateHistories.ToListAsync());
        Assert.Equal(1, contracts.EvaluateActivationCallCount);
    }

    [Fact]
    public async Task ApproveAsync_RejectsDuplicateAcceptance()
    {
        await using var context = CreateContext();
        var milestone = CreateMilestone(MilestoneStatus.Draft, 1);
        milestone.AcceptedByClientAt = _utcNow.AddMinutes(-1);
        await AddMilestonesAsync(context, milestone);
        var service = CreateService(
            context,
            new MutableCurrentUser(_clientUserId),
            CreateContractStub(ContractStatus.Draft));

        var exception = await Assert.ThrowsAsync<ConflictException>(() =>
            service.ApproveAsync(
                milestone.Id,
                ToETag(milestone.RowVersion),
                CancellationToken.None));

        Assert.Contains("مسبقًا", exception.Message);
        Assert.Empty(await context.MilestoneStateHistories.ToListAsync());
    }

    [Fact]
    public async Task MarkReadyForFundingAsync_WritesOneOutboxEvent()
    {
        await using var context = CreateContext();
        var milestone = CreateMilestone(
            MilestoneStatus.AwaitingFunding,
            1);
        milestone.AcceptedByClientAt = _utcNow.AddMinutes(-2);
        milestone.AcceptedByLawyerAt = _utcNow.AddMinutes(-1);
        await AddMilestonesAsync(context, milestone);
        var service = CreateService(
            context,
            new MutableCurrentUser(_lawyerUserId),
            CreateContractStub(ContractStatus.Active));

        var result = await service.MarkReadyForFundingAsync(
            milestone.Id,
            ToETag(milestone.RowVersion),
            CancellationToken.None);

        Assert.Equal(
            MilestoneStatus.AwaitingFunding.ToString(),
            result.Status);
        Assert.Equal(_utcNow, milestone.ReadyForFundingAt);
        var outbox = await context.OutboxMessages.SingleAsync();
        Assert.Equal(
            ContractPaymentEventTypes.MilestoneReadyForFunding,
            outbox.EventType);
        Assert.Equal(milestone.Id, outbox.AggregateId);
    }

    [Fact]
    public async Task MarkReadyForFundingAsync_BlocksLaterUnsettledMilestone()
    {
        await using var context = CreateContext();
        var first = CreateMilestone(
            MilestoneStatus.AwaitingFunding,
            1);
        var second = CreateMilestone(
            MilestoneStatus.AwaitingFunding,
            2);
        await AddMilestonesAsync(context, first, second);
        var service = CreateService(
            context,
            new MutableCurrentUser(_lawyerUserId),
            CreateContractStub(ContractStatus.Active));

        var exception = await Assert.ThrowsAsync<BusinessException>(() =>
            service.MarkReadyForFundingAsync(
                second.Id,
                ToETag(second.RowVersion),
                CancellationToken.None));

        Assert.Contains("المراحل السابقة", exception.Message);
        Assert.Empty(await context.OutboxMessages.ToListAsync());
    }

    [Fact]
    public async Task ListAsync_DerivesFundingAndPermittedActions()
    {
        await using var context = CreateContext();
        var draft = CreateMilestone(MilestoneStatus.Draft, 1);
        var funded = CreateMilestone(
            MilestoneStatus.FundedInProgress,
            2);
        funded.FundedAt = _utcNow.AddHours(-1);
        var hold = new EscrowHold(
            Guid.NewGuid(),
            Guid.NewGuid(),
            _contractId,
            funded.Id,
            1_000m,
            50m,
            950m,
            Guid.NewGuid(),
            funded.FundedAt.Value,
            funded.FundedAt.Value);
        await AddMilestonesAsync(context, draft, funded);
        context.EscrowHolds.Add(hold);
        await context.SaveChangesAsync();
        var service = CreateService(
            context,
            new MutableCurrentUser(_clientUserId),
            CreateContractStub(ContractStatus.Active));

        var result = await service.ListAsync(
            _contractId,
            CancellationToken.None);

        Assert.Equal(2, result.Count);
        Assert.Contains("Update", result[0].PermittedActions);
        Assert.Contains("Approve", result[0].PermittedActions);
        Assert.Equal(
            MilestoneFundingStatus.Funded,
            result[1].FundingStatus);
        Assert.Equal(hold.Id, result[1].EscrowHoldId);
        Assert.Equal(950m, result[1].NetLawyerAmount);
    }

    [Fact]
    public async Task UpdateDraftAsync_RejectsNonDraftMilestone()
    {
        await using var context = CreateContext();
        var milestone = CreateMilestone(
            MilestoneStatus.FundedInProgress,
            1);
        await AddMilestonesAsync(context, milestone);
        var service = CreateService(
            context,
            new MutableCurrentUser(_lawyerUserId),
            CreateContractStub(ContractStatus.Active));

        await Assert.ThrowsAsync<BusinessException>(() =>
            service.UpdateDraftAsync(
                _contractId,
                milestone.Id,
                new UpdateMilestoneRequest(
                    "Updated filing",
                    null,
                    20,
                    null),
                ToETag(milestone.RowVersion),
                CancellationToken.None));
    }

    private MilestoneService CreateService(
        ApplicationDbContext context,
        MutableCurrentUser currentUser,
        ContractServiceStub contracts)
    {
        var timeProvider = new FixedTimeProvider(_utcNow);
        return new MilestoneService(
            context,
            currentUser,
            contracts,
            new OutboxWriter(context, timeProvider),
            timeProvider);
    }

    private ContractServiceStub CreateContractStub(
        ContractStatus status)
    {
        return new ContractServiceStub(
            new ContractDetailDto(
                _contractId,
                Guid.NewGuid(),
                Guid.NewGuid(),
                _clientUserId,
                _lawyerUserId,
                "Legal representation",
                "Contract terms",
                "EGP",
                status,
                null,
                null,
                status == ContractStatus.Active ? _utcNow : null,
                null,
                null,
                0m,
                [],
                [],
                []),
            _utcNow);
    }

    private AddMilestoneRequest ValidAddRequest(int orderNumber)
    {
        return new AddMilestoneRequest(
            $"Milestone {orderNumber}",
            null,
            orderNumber,
            1_000m,
            14,
            _utcNow.AddDays(14));
    }

    private Milestone CreateMilestone(
        MilestoneStatus status,
        int orderNumber)
    {
        return new Milestone(
            Guid.NewGuid(),
            _contractId,
            $"Milestone {orderNumber}",
            null,
            orderNumber,
            1_000m,
            14,
            _utcNow.AddDays(14),
            _utcNow.AddHours(-1))
        {
            Status = status,
            RowVersion = [1, 2, 3, (byte)orderNumber]
        };
    }

    private static async Task AddMilestonesAsync(
        ApplicationDbContext context,
        params Milestone[] milestones)
    {
        context.Milestones.AddRange(milestones);
        await context.SaveChangesAsync();
        foreach (var milestone in milestones)
        {
            if (milestone.RowVersion.Length == 0)
            {
                milestone.RowVersion = [1, 2, 3, 4];
            }
        }
    }

    private ApplicationDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(
                $"milestone-service-{Guid.NewGuid():N}")
            .Options;
        return new ApplicationDbContext(
            options,
            new FixedTimeProvider(_utcNow));
    }

    private static string ToETag(byte[] rowVersion)
        => $"\"{Convert.ToBase64String(rowVersion)}\"";

    private sealed class MutableCurrentUser(Guid userId)
        : ICurrentUserService
    {
        public Guid? UserId { get; set; } = userId;
        public bool IsAuthenticated => UserId.HasValue;
    }

    private sealed class ContractServiceStub(
        ContractDetailDto contract,
        DateTime utcNow) : IContractService
    {
        public int EvaluateActivationCallCount { get; private set; }

        public Task<ContractDetailDto> GetAsync(
            Guid contractId,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult(contract);
        }

        public Task<ContractActionResultDto> EvaluateActivationAsync(
            Guid contractId,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            EvaluateActivationCallCount++;
            return Task.FromResult(
                new ContractActionResultDto(
                    contractId,
                    contract.Status.ToString(),
                    utcNow));
        }

        public Task<ContractDetailDto> CreateAsync(
            CreateContractRequest request,
            CancellationToken cancellationToken)
            => throw new NotSupportedException();

        public Task<PagedResult<ContractSummaryDto>> ListAsync(
            ContractListQuery query,
            CancellationToken cancellationToken)
            => throw new NotSupportedException();

        public Task<ContractDetailDto> UpdateDraftAsync(
            Guid contractId,
            UpdateContractRequest request,
            string ifMatch,
            CancellationToken cancellationToken)
            => throw new NotSupportedException();

        public Task<ContractActionResultDto> AcceptAsync(
            Guid contractId,
            string ifMatch,
            CancellationToken cancellationToken)
            => throw new NotSupportedException();

        public Task<PagedResult<ContractStateHistoryDto>>
            GetStateHistoryAsync(
                Guid contractId,
                ContractStateHistoryQuery query,
                CancellationToken cancellationToken)
            => throw new NotSupportedException();

        public Task<ContractActionResultDto> EvaluateCompletionAsync(
            Guid contractId,
            CancellationToken cancellationToken)
            => throw new NotSupportedException();

        public Task<ContractDetailDto> TerminateAsync(
            Guid contractId,
            TerminateContractRequest request,
            string ifMatch,
            CancellationToken cancellationToken)
            => throw new NotSupportedException();
    }

    private sealed class FixedTimeProvider(DateTime utcNow)
        : TimeProvider
    {
        public override DateTimeOffset GetUtcNow()
            => new(utcNow);
    }
}
