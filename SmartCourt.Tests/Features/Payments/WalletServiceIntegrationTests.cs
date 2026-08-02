using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using SmartCourt.Common.Exceptions;
using SmartCourt.Entities;
using SmartCourt.Features.Payments;
using SmartCourt.Features.Payments.DTOs;
using SmartCourt.Features.Payments.Entities;
using SmartCourt.Features.Payments.Enums;
using SmartCourt.Infrastructure.Idempotency;
using SmartCourt.Infrastructure.Persistence.Enums;
using SmartCourt.Infrastructure.Providers.Jobs;
using SmartCourt.Infrastructure.Providers.Payments;
using SmartCourt.Interfaces;
using SmartCourt.Persistence;
using Xunit;

namespace SmartCourt.Tests.Features.Payments;

public sealed class WalletServiceIntegrationTests : IAsyncLifetime
{
    private readonly string _databaseName =
        $"SmartCourtWalletServiceTests_{Guid.NewGuid():N}";
    private readonly Guid _lawyerUserId = Guid.NewGuid();
    private readonly DateTime _utcNow =
        new(2026, 8, 12, 10, 0, 0, DateTimeKind.Utc);

    public async Task InitializeAsync()
    {
        await using var context = CreateContext();
        await context.Database.EnsureCreatedAsync();
        context.Users.Add(CreateUser(_lawyerUserId));
        context.LawyerWallets.Add(
            new LawyerWallet(
                Guid.NewGuid(),
                _lawyerUserId,
                _utcNow.AddDays(-1))
            {
                PendingBalance = 200m,
                AvailableBalance = 1_000m
            });
        await context.SaveChangesAsync();
    }

    public async Task DisposeAsync()
    {
        await using var context = CreateContext();
        await context.Database.EnsureDeletedAsync();
    }

    [Fact]
    public async Task GetAsync_ReturnsOnlyAuthenticatedLawyerWallet()
    {
        await using var context = CreateContext();
        var service = CreateService(
            context,
            new TestPaymentProvider(
                ProviderOperationOutcome.Succeeded));

        var result = await service.GetAsync(CancellationToken.None);

        Assert.Equal(_lawyerUserId, result.LawyerUserId);
        Assert.Equal("EGP", result.Currency);
        Assert.Equal(200m, result.PendingBalance);
        Assert.Equal(1_000m, result.AvailableBalance);
        Assert.Equal(0m, result.TotalReleased);
    }

    [Fact]
    public async Task WithdrawAsync_SuccessReducesAvailableBalanceExactlyOnce()
    {
        await using var context = CreateContext();
        var provider = new TestPaymentProvider(
            ProviderOperationOutcome.Succeeded);
        var service = CreateService(context, provider);
        var request = new CreateWithdrawalRequest(
            400m,
            "bank-account-token");

        var result = await service.WithdrawAsync(
            request,
            "withdraw-success",
            CancellationToken.None);
        var replay = await service.WithdrawAsync(
            request,
            "withdraw-success",
            CancellationToken.None);

        Assert.Equal(result, replay);
        Assert.Equal(WithdrawalStatus.Completed.ToString(), result.Status);
        Assert.Equal(
            600m,
            (await context.LawyerWallets.SingleAsync())
                .AvailableBalance);
        var withdrawal =
            await context.WithdrawalRequests.SingleAsync();
        Assert.Equal(WithdrawalStatus.Completed, withdrawal.Status);
        Assert.NotNull(withdrawal.ProviderTransactionId);
        Assert.Equal(1, provider.WithdrawCalls);
        Assert.Equal(
            "bank-account-token",
            provider.LastRequest?.DestinationReference);
        Assert.Equal(
            IdempotencyStatus.Completed,
            (await context.IdempotencyRecords.SingleAsync()).Status);
    }

    [Fact]
    public async Task WithdrawAsync_ConfirmedFailureReleasesReservedBalance()
    {
        await using var context = CreateContext();
        var provider = new TestPaymentProvider(
            ProviderOperationOutcome.Failed);
        var service = CreateService(context, provider);
        var request = new CreateWithdrawalRequest(
            400m,
            "bank-account-token");

        var exception = await Assert.ThrowsAsync<BusinessException>(() =>
            service.WithdrawAsync(
                request,
                "withdraw-failure",
                CancellationToken.None));

        Assert.Contains("إعادة المبلغ", exception.Message);
        Assert.Equal(
            1_000m,
            (await context.LawyerWallets.SingleAsync())
                .AvailableBalance);
        Assert.Equal(
            WithdrawalStatus.Failed,
            (await context.WithdrawalRequests.SingleAsync()).Status);
        Assert.Equal(
            IdempotencyStatus.Failed,
            (await context.IdempotencyRecords.SingleAsync()).Status);

        await Assert.ThrowsAsync<BusinessException>(() =>
            service.WithdrawAsync(
                request,
                "withdraw-failure",
                CancellationToken.None));
        Assert.Equal(1, provider.WithdrawCalls);
    }

    [Fact]
    public async Task WithdrawAsync_UnknownOutcomeKeepsFundsReserved()
    {
        await using var context = CreateContext();
        var provider = new TestPaymentProvider(
            ProviderOperationOutcome.Unknown);
        var service = CreateService(context, provider);
        var request = new CreateWithdrawalRequest(
            400m,
            "bank-account-token");

        var exception = await Assert.ThrowsAsync<BusinessException>(() =>
            service.WithdrawAsync(
                request,
                "withdraw-unknown",
                CancellationToken.None));

        Assert.Contains("غير مؤكدة", exception.Message);
        Assert.Equal(
            600m,
            (await context.LawyerWallets.SingleAsync())
                .AvailableBalance);
        Assert.Equal(
            WithdrawalStatus.Processing,
            (await context.WithdrawalRequests.SingleAsync()).Status);
        Assert.Equal(
            IdempotencyStatus.Processing,
            (await context.IdempotencyRecords.SingleAsync()).Status);

        var replayException =
            await Assert.ThrowsAsync<BusinessException>(() =>
                service.WithdrawAsync(
                    request,
                    "withdraw-unknown",
                    CancellationToken.None));
        Assert.Matches(
            "[\\u0600-\\u06FF]",
            replayException.Message);
        Assert.Equal(1, provider.WithdrawCalls);

        var reconciliationProvider = new TestPaymentProvider(
            ProviderOperationOutcome.Succeeded);
        var reconciliationService = CreateService(
            context,
            reconciliationProvider);
        var reconciliation =
            await reconciliationService
                .ReconcilePendingWithdrawalsAsync(
                    CancellationToken.None);

        Assert.Equal(
            JobExecutionOutcome.Completed,
            reconciliation.Outcome);
        Assert.Equal(
            WithdrawalStatus.Completed,
            (await context.WithdrawalRequests.SingleAsync()).Status);
        Assert.Equal(
            IdempotencyStatus.Completed,
            (await context.IdempotencyRecords.SingleAsync()).Status);
        Assert.Equal(
            600m,
            (await context.LawyerWallets.SingleAsync())
                .AvailableBalance);
    }

    [Fact]
    public async Task WithdrawAsync_CannotUsePendingOrInsufficientFunds()
    {
        await using var context = CreateContext();
        var wallet = await context.LawyerWallets.SingleAsync();
        wallet.PendingBalance = 1_200m;
        wallet.AvailableBalance = 0m;
        await context.SaveChangesAsync();
        var provider = new TestPaymentProvider(
            ProviderOperationOutcome.Succeeded);
        var service = CreateService(context, provider);

        var exception = await Assert.ThrowsAsync<BusinessException>(() =>
            service.WithdrawAsync(
                new CreateWithdrawalRequest(
                    100m,
                    "bank-account-token"),
                "withdraw-pending-funds",
                CancellationToken.None));

        Assert.Contains("الرصيد المتاح", exception.Message);
        Assert.Equal(1_200m, wallet.PendingBalance);
        Assert.Equal(0m, wallet.AvailableBalance);
        Assert.Empty(await context.WithdrawalRequests.ToListAsync());
        Assert.Equal(0, provider.WithdrawCalls);
    }

    [Fact]
    public async Task ConcurrentWithdrawals_CannotOverdrawAvailableBalance()
    {
        await using var firstContext = CreateContext();
        await using var secondContext = CreateContext();
        var firstProvider = new TestPaymentProvider(
            ProviderOperationOutcome.Succeeded);
        var secondProvider = new TestPaymentProvider(
            ProviderOperationOutcome.Succeeded);
        var firstService = CreateService(
            firstContext,
            firstProvider);
        var secondService = CreateService(
            secondContext,
            secondProvider);
        var request = new CreateWithdrawalRequest(
            700m,
            "bank-account-token");

        var attempts = await Task.WhenAll(
            TryWithdrawAsync(
                firstService,
                request,
                "concurrent-withdrawal-1"),
            TryWithdrawAsync(
                secondService,
                request,
                "concurrent-withdrawal-2"));

        Assert.Single(attempts, exception => exception is null);
        Assert.Single(
            attempts,
            exception => exception is BusinessException);
        await using var assertionContext = CreateContext();
        Assert.Equal(
            300m,
            (await assertionContext.LawyerWallets.SingleAsync())
                .AvailableBalance);
        Assert.Single(
            await assertionContext.WithdrawalRequests
                .Where(item =>
                    item.Status == WithdrawalStatus.Completed)
                .ToListAsync());
        Assert.Equal(
            1,
            firstProvider.WithdrawCalls
            + secondProvider.WithdrawCalls);
    }

    private static async Task<Exception?> TryWithdrawAsync(
        IWalletService service,
        CreateWithdrawalRequest request,
        string idempotencyKey)
    {
        try
        {
            await service.WithdrawAsync(
                request,
                idempotencyKey,
                CancellationToken.None);
            return null;
        }
        catch (Exception exception)
        {
            return exception;
        }
    }

    private WalletService CreateService(
        ApplicationDbContext context,
        IPaymentProvider paymentProvider)
    {
        var timeProvider = new FixedTimeProvider(_utcNow);
        return new WalletService(
            context,
            new CurrentUserStub(_lawyerUserId),
            paymentProvider,
            (IPaymentReconciliationProvider)paymentProvider,
            new IdempotencyService(
                context,
                new CanonicalIdempotencyRequestHasher(),
                timeProvider),
            timeProvider,
            NullLogger<WalletService>.Instance);
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

    private static ApplicationUser CreateUser(Guid userId)
    {
        var userName = $"wallet-lawyer-{userId:N}";
        return new ApplicationUser
        {
            Id = userId,
            UserName = userName,
            NormalizedUserName = userName.ToUpperInvariant(),
            Email = $"{userName}@example.test",
            NormalizedEmail = $"{userName}@example.test"
                .ToUpperInvariant(),
            FullName = "Wallet Lawyer",
            NationalNumber = userId.ToString("N")[..14]
        };
    }

    private string ConnectionString =>
        $"Server=localhost;Database={_databaseName};Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=True;";

    private sealed class CurrentUserStub(Guid userId)
        : ICurrentUserService
    {
        public Guid? UserId { get; } = userId;
        public bool IsAuthenticated => true;
    }

    private sealed class TestPaymentProvider(
        ProviderOperationOutcome outcome)
        : IPaymentProvider, IPaymentReconciliationProvider
    {
        public int WithdrawCalls { get; private set; }
        public ProviderWithdrawalRequest? LastRequest { get; private set; }

        public Task<ProviderResult> WithdrawAsync(
            ProviderWithdrawalRequest request,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            WithdrawCalls++;
            LastRequest = request;
            return Task.FromResult(
                new ProviderResult(
                    request.Amount,
                    request.Currency,
                    request.BusinessId,
                    request.ProviderIdempotencyKey,
                    request.CorrelationId,
                    outcome,
                    outcome == ProviderOperationOutcome.Succeeded
                        ? $"withdrawal-{Guid.NewGuid():N}"
                        : null,
                    outcome == ProviderOperationOutcome.Succeeded
                        ? null
                        : "تعذر تنفيذ طلب السحب لدى مزود الدفع."));
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

        public Task<ProviderResult> RefundAsync(
            ProviderRefundRequest request,
            CancellationToken cancellationToken)
            => throw new NotSupportedException();

        public Task<ProviderResult?> GetDepositStatusAsync(
            ProviderDepositStatusRequest request,
            CancellationToken cancellationToken)
            => Task.FromResult<ProviderResult?>(null);

        public Task<ProviderResult?> GetReleaseStatusAsync(
            ProviderReleaseStatusRequest request,
            CancellationToken cancellationToken)
            => Task.FromResult<ProviderResult?>(null);

        public Task<ProviderResult?> GetRefundStatusAsync(
            ProviderRefundStatusRequest request,
            CancellationToken cancellationToken)
            => Task.FromResult<ProviderResult?>(null);

        public Task<ProviderResult?> GetWithdrawalStatusAsync(
            ProviderWithdrawalStatusRequest request,
            CancellationToken cancellationToken)
            => Task.FromResult<ProviderResult?>(new ProviderResult(
                request.Amount,
                request.Currency,
                request.BusinessId,
                request.ProviderIdempotencyKey,
                request.CorrelationId,
                outcome,
                outcome == ProviderOperationOutcome.Succeeded
                    ? $"withdrawal-{Guid.NewGuid():N}"
                    : null,
                outcome == ProviderOperationOutcome.Succeeded
                    ? null
                    : "تعذر تنفيذ طلب السحب لدى مزود الدفع."));
    }

    private sealed class FixedTimeProvider(DateTime utcNow)
        : TimeProvider
    {
        public override DateTimeOffset GetUtcNow()
            => new(utcNow);
    }
}
