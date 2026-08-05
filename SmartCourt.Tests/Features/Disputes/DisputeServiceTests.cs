using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging.Abstractions;
using SmartCourt.Features.Contracts.Entities;
using SmartCourt.Features.Contracts.Enums;
using SmartCourt.Features.Contracts;
using SmartCourt.Features.Contracts.DTOs;
using SmartCourt.Features.Disputes;
using SmartCourt.Features.Disputes.DTOs;
using SmartCourt.Features.Disputes.Enums;
using SmartCourt.Features.Files.Integration;
using SmartCourt.Features.Milestones.Entities;
using SmartCourt.Features.Milestones.Enums;
using SmartCourt.Features.Payments.Entities;
using SmartCourt.Features.Payments.Enums;
using SmartCourt.Features.Payments.FundingVerification;
using SmartCourt.Features.Users.Integration;
using SmartCourt.Infrastructure.Idempotency;
using SmartCourt.Infrastructure.Providers.Events;
using SmartCourt.Infrastructure.Providers.Jobs;
using SmartCourt.Infrastructure.Providers.Payments;
using SmartCourt.Interfaces;
using SmartCourt.Persistence;
using Xunit;

namespace SmartCourt.Tests.Features.Disputes;

public sealed class DisputeServiceTests
{
    private static readonly DateTime Now =
        new(2026, 8, 1, 8, 0, 0, DateTimeKind.Utc);

    [Fact]
    public async Task CreateAsync_FreezesExactHoldAndSuspendsContractAtomically()
    {
        var state = await CreateFundedStateAsync(MilestoneStatus.AcceptedHold);
        await using var context = state.Context;
        var service = CreateService(context, state.ClientUserId);

        var result = await service.CreateAsync(
            new CreateDisputeRequest(
                state.Milestone.Id,
                DisputeCategory.DeliverableQuality,
                "مستند التسليم غير صالح",
                "المستند المسلم لا يحقق المتطلبات المتفق عليها.",
                DisputeRequestedOutcome.Refund,
                []),
            CancellationToken.None);

        Assert.Equal(DisputeStatus.Open, result.Status);
        Assert.Equal(EscrowHoldStatus.Frozen, state.Hold.Status);
        Assert.Equal(MilestoneStatus.Disputed, state.Milestone.Status);
        Assert.Equal(ContractStatus.SuspendedByDispute, state.Contract.Status);
        Assert.Equal(
            ContractPaymentEventTypes.DisputeOpened,
            (await context.OutboxMessages.SingleAsync()).EventType);
    }

    [Fact]
    public async Task ResolveAsync_FullRelease_ReconcilesLedgerWalletAndImmutableResolution()
    {
        await using var connection =
            new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        var sqliteContext = await CreateSqliteContextAsync(connection);
        var state = await CreateFundedStateAsync(
            MilestoneStatus.Disputed,
            sqliteContext);
        await using var context = state.Context;
        state.Contract.Status = ContractStatus.SuspendedByDispute;
        state.Hold.Status = EscrowHoldStatus.Frozen;
        state.Hold.FrozenAt = Now;
        var moderatorId = Guid.NewGuid();
        var dispute = new SmartCourt.Features.Disputes.Entities.Dispute(
            Guid.NewGuid(),
            state.Contract.Id,
            state.Milestone.Id,
            state.ClientUserId,
            DisputeCategory.DeliverableQuality,
            "مستند التسليم غير صالح",
            "المستند المسلم لا يحقق المتطلبات المتفق عليها.",
            DisputeRequestedOutcome.Review,
            Now)
        {
            AssignedModeratorUserId = moderatorId,
            Status = DisputeStatus.UnderReview
        };
        context.Disputes.Add(dispute);
        await context.SaveChangesAsync();

        var eligibility = new TestEligibilityService();
        eligibility.Results[moderatorId] = new ContractUserEligibilityFacts(
            moderatorId,
            true,
            false,
            false,
            true,
            false,
            false);
        var provider = new SuccessfulProvider(context);
        var service = CreateService(
            context,
            moderatorId,
            eligibility,
            provider);

        var result = await service.ResolveAsync(
            dispute.Id,
            new ResolveDisputeRequest(
                DisputeResolutionType.FullRelease,
                0m,
                950m,
                "ثبت سلامة التسليم واستحقاق المحامي للمبلغ."),
            "resolution-key",
            CancellationToken.None);

        Assert.Equal(DisputeStatus.Resolved, result.Status);
        Assert.Equal("Completed", result.Settlement?.Status);
        Assert.Equal(EscrowHoldStatus.Released, state.Hold.Status);
        Assert.Equal(MilestoneStatus.Released, state.Milestone.Status);
        Assert.Equal(ContractStatus.Active, state.Contract.Status);
        Assert.Equal(0m, state.Wallet.PendingBalance);
        Assert.Equal(950m, state.Wallet.AvailableBalance);
        Assert.Single(await context.DisputeResolutions.ToListAsync());
        Assert.Equal(2, await context.EscrowLedgerEntries.CountAsync());
        Assert.True(provider.CallsObservedWithoutTransaction);
        Assert.True(provider.AttemptsObservedBeforeCall);
    }

    [Fact]
    public async Task RecoverPendingSettlementsAsync_RetriesFailedAttemptAndFinalizesOnce()
    {
        await using var connection =
            new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        var sqliteContext = await CreateSqliteContextAsync(connection);
        var state = await CreateFundedStateAsync(
            MilestoneStatus.Disputed,
            sqliteContext);
        await using var context = state.Context;
        state.Contract.Status = ContractStatus.SuspendedByDispute;
        state.Hold.Status = EscrowHoldStatus.Frozen;
        state.Hold.FrozenAt = Now;
        var moderatorId = Guid.NewGuid();
        var dispute = new SmartCourt.Features.Disputes.Entities.Dispute(
            Guid.NewGuid(),
            state.Contract.Id,
            state.Milestone.Id,
            state.ClientUserId,
            DisputeCategory.DeliverableQuality,
            "مستند التسليم غير صالح",
            "المستند المسلم لا يحقق المتطلبات المتفق عليها.",
            DisputeRequestedOutcome.Review,
            Now)
        {
            AssignedModeratorUserId = moderatorId,
            Status = DisputeStatus.UnderReview
        };
        context.Disputes.Add(dispute);
        await context.SaveChangesAsync();

        var eligibility = new TestEligibilityService();
        eligibility.Results[moderatorId] = new ContractUserEligibilityFacts(
            moderatorId,
            true,
            false,
            false,
            true,
            false,
            false);
        var provider = new FailThenSucceedReleaseProvider(context);
        var evaluator = new RecordingCompletionEvaluator();
        var service = CreateService(
            context,
            moderatorId,
            eligibility,
            provider,
            evaluator);

        var resolved = await service.ResolveAsync(
            dispute.Id,
            new ResolveDisputeRequest(
                DisputeResolutionType.FullRelease,
                0m,
                950m,
                "ثبت سلامة التسليم واستحقاق المحامي للمبلغ."),
            "resolution-recovery-key",
            CancellationToken.None);

        Assert.Equal("Failed", resolved.Settlement?.Status);
        Assert.Equal(EscrowHoldStatus.Frozen, state.Hold.Status);

        var recovered = await service.RecoverPendingSettlementsAsync(
            CancellationToken.None);
        var repeated = await service.RecoverPendingSettlementsAsync(
            CancellationToken.None);

        Assert.Equal(JobExecutionOutcome.Completed, recovered.Outcome);
        Assert.Equal(JobExecutionOutcome.NoOp, repeated.Outcome);
        Assert.Equal(EscrowHoldStatus.Released, state.Hold.Status);
        Assert.Equal(MilestoneStatus.Released, state.Milestone.Status);
        Assert.Equal(ContractStatus.Active, state.Contract.Status);
        Assert.Equal(0m, state.Wallet.PendingBalance);
        Assert.Equal(950m, state.Wallet.AvailableBalance);
        Assert.Equal(2, await context.PaymentTransactions.CountAsync(item =>
            item.OperationType == PaymentOperationType.Release));
        Assert.Equal(2, await context.EscrowLedgerEntries.CountAsync());
        Assert.Equal(1, evaluator.CallCount);
        Assert.True(provider.CallsObservedWithoutTransaction);
        Assert.True(provider.AttemptsObservedBeforeCall);
    }

    private static DisputeService CreateService(
        ApplicationDbContext context,
        Guid actorUserId,
        TestEligibilityService? eligibility = null,
        IPaymentProvider? paymentProvider = null,
        RecordingCompletionEvaluator? completionEvaluator = null)
    {
        var timeProvider = new FixedTimeProvider(Now);
        return new DisputeService(
            context,
            new TestCurrentUserService(actorUserId),
            eligibility ?? new TestEligibilityService(),
            new TestFileAccessService(),
            new MilestoneFundingVerifier(context),
            new IdempotencyService(
                context,
                new CanonicalIdempotencyRequestHasher(),
                timeProvider),
            paymentProvider ?? new SuccessfulProvider(),
            new UnusedScheduler(),
            completionEvaluator ?? new RecordingCompletionEvaluator(),
            new OutboxWriter(context, timeProvider),
            timeProvider,
            NullLogger<DisputeService>.Instance);
    }

    private static async Task<TestState> CreateFundedStateAsync(
        MilestoneStatus milestoneStatus,
        ApplicationDbContext? context = null)
    {
        if (context is null)
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .ConfigureWarnings(warnings => warnings.Ignore(
                    InMemoryEventId.TransactionIgnoredWarning))
                .Options;
            context = new ApplicationDbContext(
                options,
                new FixedTimeProvider(Now));
        }
        var clientUserId = Guid.NewGuid();
        var lawyerUserId = Guid.NewGuid();
        var contract = new Contract(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            clientUserId,
            lawyerUserId,
            "عقد تمثيل قانوني",
            "شروط قانونية واضحة",
            Now)
        {
            Status = ContractStatus.Active,
            ActivatedAt = Now
        };
        var milestone = new Milestone(
            Guid.NewGuid(),
            contract.Id,
            "إعداد المذكرة",
            null,
            1,
            1_000m,
            10,
            null,
            Now)
        {
            Status = milestoneStatus,
            FundedAt = Now,
            AcceptedAt = Now,
            HoldStartsAt = Now,
            HoldExpiresAt = Now.AddDays(14),
            AcceptedByClientAt = Now,
            AcceptedByLawyerAt = Now
        };
        var account = new EscrowAccount(Guid.NewGuid(), contract.Id, Now)
        {
            TotalDeposited = 1_000m
        };
        var deposit = new PaymentTransaction(
            Guid.NewGuid(),
            contract.Id,
            milestone.Id,
            PaymentOperationType.Deposit,
            "TestProvider",
            $"deposit-{Guid.NewGuid():N}",
            1_000m,
            Now)
        {
            Status = PaymentTransactionStatus.Completed,
            ProcessedAt = Now,
            ProviderTransactionId = $"provider-{Guid.NewGuid():N}"
        };
        var hold = new EscrowHold(
            Guid.NewGuid(),
            account.Id,
            contract.Id,
            milestone.Id,
            1_000m,
            50m,
            950m,
            deposit.Id,
            Now,
            Now)
        {
            HoldStartsAt = Now,
            HoldExpiresAt = Now.AddDays(14)
        };
        deposit.EscrowHoldId = hold.Id;
        var wallet = new LawyerWallet(Guid.NewGuid(), lawyerUserId, Now)
        {
            PendingBalance = 950m
        };
        context.AddRange(contract, milestone, account, deposit, hold, wallet);
        await context.SaveChangesAsync();
        return new TestState(
            context,
            clientUserId,
            contract,
            milestone,
            hold,
            wallet);
    }

    private static async Task<ApplicationDbContext> CreateSqliteContextAsync(
        SqliteConnection connection)
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlite(connection)
            .Options;
        var context = new ApplicationDbContext(
            options,
            new FixedTimeProvider(Now));
        var createScript = context.Database.GenerateCreateScript()
            .Replace(
                "\"RowVersion\" BLOB NOT NULL",
                "\"RowVersion\" BLOB NOT NULL DEFAULT (randomblob(8))",
                StringComparison.Ordinal);
        await context.Database.ExecuteSqlRawAsync(createScript);
        await context.Database.ExecuteSqlRawAsync(
            "PRAGMA foreign_keys = OFF;");
        return context;
    }

    private sealed record TestState(
        ApplicationDbContext Context,
        Guid ClientUserId,
        Contract Contract,
        Milestone Milestone,
        EscrowHold Hold,
        LawyerWallet Wallet);

    private sealed class FixedTimeProvider(DateTime value) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => new(value);
    }

    private sealed class TestCurrentUserService(Guid userId) : ICurrentUserService
    {
        public Guid? UserId => userId;
        public bool IsAuthenticated => true;
    }

    private sealed class TestEligibilityService : IContractUserEligibilityService
    {
        public Dictionary<Guid, ContractUserEligibilityFacts> Results { get; } = [];

        public Task<ContractUserEligibilityFacts?> FindEligibilityAsync(
            Guid userId,
            CancellationToken cancellationToken)
        {
            Results.TryGetValue(userId, out var result);
            return Task.FromResult(result);
        }
    }

    private sealed class TestFileAccessService : IContractFileAccessService
    {
        public Task<IReadOnlyList<AuthorizedContractFile>> AuthorizeForUseAsync(
            Guid actorUserId,
            IReadOnlyCollection<Guid> storedFileIds,
            ContractFilePurpose purpose,
            Guid relatedEntityId,
            CancellationToken cancellationToken)
            => Task.FromResult<IReadOnlyList<AuthorizedContractFile>>(
                storedFileIds.Select(id => new AuthorizedContractFile(id, actorUserId)).ToArray());

        public Task<ContractFileReadAccess?> GetAuthorizedReadAccessAsync(
            Guid actorUserId,
            Guid storedFileId,
            ContractFilePurpose purpose,
            Guid relatedEntityId,
            CancellationToken cancellationToken)
            => Task.FromResult<ContractFileReadAccess?>(null);
    }

    private sealed class SuccessfulProvider(
        ApplicationDbContext? context = null) : IPaymentProvider
    {
        public bool CallsObservedWithoutTransaction { get; private set; } = true;
        public bool AttemptsObservedBeforeCall { get; private set; } = true;

        public Task<ProviderResult> DepositAsync(ProviderDepositRequest request, CancellationToken cancellationToken)
            => throw new NotSupportedException();

        public Task<ProviderResult> RetryDepositAsync(ProviderDepositRetryRequest request, CancellationToken cancellationToken)
            => throw new NotSupportedException();

        public async Task<ProviderResult> ReleaseAsync(ProviderReleaseRequest request, CancellationToken cancellationToken)
        {
            await ObserveCallAsync(request.CorrelationId, cancellationToken);
            return Success(request, "release");
        }

        public async Task<ProviderResult> RefundAsync(ProviderRefundRequest request, CancellationToken cancellationToken)
        {
            await ObserveCallAsync(request.CorrelationId, cancellationToken);
            return Success(request, "refund");
        }

        public Task<ProviderResult> WithdrawAsync(ProviderWithdrawalRequest request, CancellationToken cancellationToken)
            => throw new NotSupportedException();

        private static ProviderResult Success(PaymentProviderRequest request, string operation)
            => new(
                request.Amount,
                request.Currency,
                request.BusinessId,
                request.ProviderIdempotencyKey,
                request.CorrelationId,
                ProviderOperationOutcome.Succeeded,
                $"provider-{operation}-{Guid.NewGuid():N}",
                null);

        private async Task ObserveCallAsync(
            Guid paymentTransactionId,
            CancellationToken cancellationToken)
        {
            if (context is null)
            {
                return;
            }

            CallsObservedWithoutTransaction &=
                context.Database.CurrentTransaction is null;
            AttemptsObservedBeforeCall &=
                await context.PaymentTransactions
                    .AsNoTracking()
                    .AnyAsync(
                        item => item.Id == paymentTransactionId,
                        cancellationToken);
        }
    }

    private sealed class FailThenSucceedReleaseProvider(
        ApplicationDbContext? context = null) : IPaymentProvider
    {
        private int _releaseCalls;
        public bool CallsObservedWithoutTransaction { get; private set; } = true;
        public bool AttemptsObservedBeforeCall { get; private set; } = true;

        public Task<ProviderResult> DepositAsync(ProviderDepositRequest request, CancellationToken cancellationToken)
            => throw new NotSupportedException();

        public Task<ProviderResult> RetryDepositAsync(ProviderDepositRetryRequest request, CancellationToken cancellationToken)
            => throw new NotSupportedException();

        public async Task<ProviderResult> ReleaseAsync(ProviderReleaseRequest request, CancellationToken cancellationToken)
        {
            _releaseCalls++;
            if (context is not null)
            {
                CallsObservedWithoutTransaction &=
                    context.Database.CurrentTransaction is null;
                AttemptsObservedBeforeCall &=
                    await context.PaymentTransactions
                        .AsNoTracking()
                        .AnyAsync(
                            item => item.Id == request.CorrelationId,
                            cancellationToken);
            }

            return new ProviderResult(
                request.Amount,
                request.Currency,
                request.BusinessId,
                request.ProviderIdempotencyKey,
                request.CorrelationId,
                _releaseCalls == 1
                    ? ProviderOperationOutcome.Failed
                    : ProviderOperationOutcome.Succeeded,
                _releaseCalls == 1 ? null : $"provider-release-{Guid.NewGuid():N}",
                _releaseCalls == 1 ? "رفض مزود الدفع محاولة التحرير الأولى." : null);
        }

        public Task<ProviderResult> RefundAsync(ProviderRefundRequest request, CancellationToken cancellationToken)
            => throw new NotSupportedException();

        public Task<ProviderResult> WithdrawAsync(ProviderWithdrawalRequest request, CancellationToken cancellationToken)
            => throw new NotSupportedException();
    }

    private sealed class UnusedScheduler : IContractJobScheduler
    {
        public Task<string> ScheduleAutoAcceptAsync(Guid milestoneId, Guid escrowHoldId, int submissionVersion, DateTime runAtUtc, CancellationToken cancellationToken) => Task.FromResult("job");
        public Task<string> ScheduleReleaseExpiredHoldAsync(Guid escrowHoldId, DateTime runAtUtc, CancellationToken cancellationToken) => Task.FromResult("job");
        public Task<string> ScheduleProviderReconciliationAsync(Guid paymentTransactionId, DateTime runAtUtc, CancellationToken cancellationToken) => Task.FromResult("job");
        public Task<string> ScheduleProviderRetryAsync(Guid paymentTransactionId, DateTime runAtUtc, CancellationToken cancellationToken) => Task.FromResult("job");
        public Task<string> ScheduleSchedulingReconciliationAsync(DateTime runAtUtc, CancellationToken cancellationToken) => Task.FromResult("job");
        public Task<string> SchedulePendingWalletProjectionReconciliationAsync(DateTime runAtUtc, CancellationToken cancellationToken) => Task.FromResult("job");
        public Task<string> ScheduleOutboxDispatchAsync(int batchSize, DateTime runAtUtc, CancellationToken cancellationToken) => Task.FromResult("job");
    }

    private sealed class RecordingCompletionEvaluator
        : IContractCompletionEvaluator
    {
        public int CallCount { get; private set; }

        public Task<ContractActionResultDto> EvaluateCompletionAsync(
            Guid contractId,
            CancellationToken cancellationToken)
        {
            CallCount++;
            return Task.FromResult(new ContractActionResultDto(
                contractId,
                ContractStatus.Active.ToString(),
                Now));
        }
    }
}
