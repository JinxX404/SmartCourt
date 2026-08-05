using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using SmartCourt.Common.Exceptions;
using SmartCourt.Features.Contracts.Entities;
using SmartCourt.Features.Payments;
using SmartCourt.Features.Payments.DTOs;
using SmartCourt.Features.Payments.Entities;
using SmartCourt.Features.Payments.Enums;
using SmartCourt.Features.Users.Integration;
using SmartCourt.Infrastructure.Idempotency;
using SmartCourt.Infrastructure.Persistence.Enums;
using SmartCourt.Interfaces;
using SmartCourt.Persistence;
using Xunit;

namespace SmartCourt.Tests.Features.Payments;

public sealed class AdminWalletAdjustmentServiceTests
{
    private static readonly DateTime UtcNow =
        new(2026, 8, 1, 12, 0, 0, DateTimeKind.Utc);

    [Fact]
    public async Task AdjustAsync_AppendsAuditAndLedgerAndUpdatesProjection()
    {
        await using var context = CreateContext();
        var state = AddFinancialState(context);
        await context.SaveChangesAsync();
        var administratorUserId = Guid.NewGuid();
        var idempotency = new RecordingIdempotencyService();
        var service = CreateService(
            context,
            administratorUserId,
            isSuperAdministrator: true,
            idempotency);
        var request = new AdminWalletAdjustmentRequest(
            state.Contract.Id,
            PendingBalanceDelta: -100m,
            AvailableBalanceDelta: 75m,
            "تصحيح إسقاط المحفظة بعد مراجعة مستندات التسوية المالية.");

        var result = await service.AdjustAsync(
            state.Contract.LawyerUserId,
            request,
            "admin-adjustment-1",
            CancellationToken.None);

        Assert.Equal(400m, state.Wallet.PendingBalance);
        Assert.Equal(275m, state.Wallet.AvailableBalance);
        Assert.Equal(400m, result.PendingBalance);
        Assert.Equal(275m, result.AvailableBalance);
        Assert.Equal(administratorUserId, result.CreatedByUserId);
        var adjustment = await context.WalletAdjustments.SingleAsync();
        Assert.Equal(500m, adjustment.PendingBalanceBefore);
        Assert.Equal(400m, adjustment.PendingBalanceAfter);
        Assert.Equal(200m, adjustment.AvailableBalanceBefore);
        Assert.Equal(275m, adjustment.AvailableBalanceAfter);
        Assert.Equal(administratorUserId, adjustment.CreatedByUserId);
        var ledger = await context.EscrowLedgerEntries.SingleAsync();
        Assert.Equal(LedgerTransactionType.Adjustment, ledger.TransactionType);
        Assert.Equal(175m, ledger.Amount);
        Assert.Equal(1_000m, ledger.RunningBalance);
        Assert.Equal(adjustment.Id, ledger.ReferenceId);
        Assert.Equal(adjustment.LedgerEntryId, ledger.Id);
        Assert.True(idempotency.Completed);
    }

    [Fact]
    public async Task AdjustAsync_ReplayDoesNotCreateAnotherFinancialRecord()
    {
        await using var context = CreateContext();
        var state = AddFinancialState(context);
        await context.SaveChangesAsync();
        var administratorUserId = Guid.NewGuid();
        var idempotency = new RecordingIdempotencyService();
        var service = CreateService(
            context,
            administratorUserId,
            isSuperAdministrator: true,
            idempotency);
        var request = new AdminWalletAdjustmentRequest(
            state.Contract.Id,
            0m,
            25m,
            "تعويض إسقاط الرصيد المتاح بعد مراجعة عملية الإفراج السابقة.");

        var first = await service.AdjustAsync(
            state.Contract.LawyerUserId,
            request,
            "admin-adjustment-replay",
            CancellationToken.None);
        var replay = await service.AdjustAsync(
            state.Contract.LawyerUserId,
            request,
            "admin-adjustment-replay",
            CancellationToken.None);

        Assert.Equal(first, replay);
        Assert.Single(await context.WalletAdjustments.ToListAsync());
        Assert.Single(await context.EscrowLedgerEntries.ToListAsync());
        Assert.Equal(225m, state.Wallet.AvailableBalance);
    }

    [Fact]
    public async Task AdjustAsync_RejectsNonSuperAdministrator()
    {
        await using var context = CreateContext();
        var state = AddFinancialState(context);
        await context.SaveChangesAsync();
        var service = CreateService(
            context,
            Guid.NewGuid(),
            isSuperAdministrator: false,
            new RecordingIdempotencyService());

        await Assert.ThrowsAsync<ForbiddenAccessException>(() =>
            service.AdjustAsync(
                state.Contract.LawyerUserId,
                new AdminWalletAdjustmentRequest(
                    state.Contract.Id,
                    0m,
                    10m,
                    "محاولة تصحيح إداري دون امتلاك صلاحية المشرف العام."),
                "unauthorized-adjustment",
                CancellationToken.None));
        Assert.Empty(await context.WalletAdjustments.ToListAsync());
        Assert.Empty(await context.EscrowLedgerEntries.ToListAsync());
    }

    [Fact]
    public async Task AdjustAsync_RejectsNegativeResultWithoutWriting()
    {
        await using var context = CreateContext();
        var state = AddFinancialState(context);
        await context.SaveChangesAsync();
        var service = CreateService(
            context,
            Guid.NewGuid(),
            isSuperAdministrator: true,
            new RecordingIdempotencyService());

        await Assert.ThrowsAsync<BusinessException>(() =>
            service.AdjustAsync(
                state.Contract.LawyerUserId,
                new AdminWalletAdjustmentRequest(
                    state.Contract.Id,
                    -501m,
                    0m,
                    "تصحيح مرفوض لأنه يتجاوز كامل الرصيد المعلق في المحفظة."),
                "negative-adjustment",
                CancellationToken.None));
        Assert.Equal(500m, state.Wallet.PendingBalance);
        Assert.Empty(await context.WalletAdjustments.ToListAsync());
        Assert.Empty(await context.EscrowLedgerEntries.ToListAsync());
    }

    private static AdminWalletAdjustmentService CreateService(
        ApplicationDbContext context,
        Guid administratorUserId,
        bool isSuperAdministrator,
        IIdempotencyService idempotencyService)
    {
        return new AdminWalletAdjustmentService(
            context,
            new StubCurrentUserService(administratorUserId),
            new StubEligibilityService(
                new ContractUserEligibilityFacts(
                    administratorUserId,
                    IsActive: true,
                    CanActAsClient: false,
                    CanActAsLawyer: false,
                    CanActAsModerator: false,
                    CanActAsFinanceAdministrator: true,
                    CanActAsSuperAdministrator: isSuperAdministrator)),
            idempotencyService,
            new FixedTimeProvider());
    }

    private static FinancialState AddFinancialState(
        ApplicationDbContext context)
    {
        var lawyerUserId = Guid.NewGuid();
        var contract = new Contract(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            lawyerUserId,
            "عقد اختبار التصحيح المالي",
            "شروط عقد صالحة لاختبار التصحيح المالي للمحفظة.",
            UtcNow);
        var account = new EscrowAccount(
            Guid.NewGuid(),
            contract.Id,
            UtcNow)
        {
            TotalDeposited = 1_000m
        };
        var wallet = new LawyerWallet(
            Guid.NewGuid(),
            lawyerUserId,
            UtcNow)
        {
            PendingBalance = 500m,
            AvailableBalance = 200m
        };
        context.AddRange(contract, account, wallet);
        return new FinancialState(contract, account, wallet);
    }

    private static ApplicationDbContext CreateContext()
    {
        return new ApplicationDbContext(
            new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(
                    $"admin-wallet-adjustment-{Guid.NewGuid():N}")
                .Options,
            new FixedTimeProvider());
    }

    private sealed record FinancialState(
        Contract Contract,
        EscrowAccount Account,
        LawyerWallet Wallet);

    private sealed class StubCurrentUserService(Guid userId)
        : ICurrentUserService
    {
        public Guid? UserId => userId;
        public bool IsAuthenticated => true;
    }

    private sealed class StubEligibilityService(
        ContractUserEligibilityFacts facts)
        : IContractUserEligibilityService
    {
        public Task<ContractUserEligibilityFacts?> FindEligibilityAsync(
            Guid userId,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult<ContractUserEligibilityFacts?>(facts);
        }
    }

    private sealed class RecordingIdempotencyService : IIdempotencyService
    {
        private readonly Guid _recordId = Guid.NewGuid();
        private string? _responseBody;
        private Guid? _resultReferenceId;

        public bool Completed { get; private set; }

        public Task<IdempotencyReservation> ReserveAsync<TRequest>(
            IdempotencyScope scope,
            string? idempotencyKey,
            TRequest request,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult(
                new IdempotencyReservation(
                    _recordId,
                    Completed
                        ? IdempotencyReservationState.Replay
                        : IdempotencyReservationState.Acquired,
                    "hash",
                    Completed
                        ? IdempotencyStatus.Completed
                        : IdempotencyStatus.Processing,
                    Completed ? 200 : null,
                    _responseBody,
                    _resultReferenceId));
        }

        public Task CompleteAsync<TResponse>(
            Guid recordId,
            int responseStatusCode,
            TResponse response,
            Guid? resultReferenceId,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            Completed = true;
            _responseBody = JsonSerializer.Serialize(
                response,
                new JsonSerializerOptions(JsonSerializerDefaults.Web));
            _resultReferenceId = resultReferenceId;
            return Task.CompletedTask;
        }

        public Task FailAsync<TResponse>(
            Guid recordId,
            int responseStatusCode,
            TResponse response,
            Guid? resultReferenceId,
            CancellationToken cancellationToken)
            => Task.CompletedTask;

        public Task<int> PurgeExpiredResponseBodiesAsync(
            CancellationToken cancellationToken)
            => Task.FromResult(0);
    }

    private sealed class FixedTimeProvider : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => new(UtcNow);
    }
}
