using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using SmartCourt.Common.Exceptions;
using SmartCourt.Features.Contracts;
using SmartCourt.Features.Contracts.DTOs;
using SmartCourt.Features.Contracts.Entities;
using SmartCourt.Features.Contracts.Enums;
using SmartCourt.Features.Milestones.Entities;
using SmartCourt.Features.Milestones.Enums;
using SmartCourt.Features.Payments;
using SmartCourt.Features.Payments.DTOs;
using SmartCourt.Features.Payments.Entities;
using SmartCourt.Features.Payments.Enums;
using SmartCourt.Features.Users.Integration;
using SmartCourt.Infrastructure.Idempotency;
using SmartCourt.Infrastructure.Persistence.Enums;
using SmartCourt.Infrastructure.Providers.Events;
using SmartCourt.Infrastructure.Providers.Jobs;
using SmartCourt.Infrastructure.Providers.Payments;
using SmartCourt.Interfaces;
using SmartCourt.Persistence;
using SmartCourt.Providers.Payments;
using Xunit;

namespace SmartCourt.Tests.Features.Payments;

public sealed class PaymentEscrowServiceTests
{
    private static readonly DateTime Now =
        new(2026, 8, 15, 10, 0, 0, DateTimeKind.Utc);
    private readonly Guid _clientUserId = Guid.NewGuid();
    private readonly Guid _lawyerUserId = Guid.NewGuid();
    private readonly Guid _contractId = Guid.NewGuid();

    [Fact]
    public async Task FundAsync_SuccessCreatesHoldLedgerWalletAndHistory()
    {
        await using var context = CreateContext();
        var milestone = CreateMilestone();
        context.Milestones.Add(milestone);
        await context.SaveChangesAsync();
        var provider = new TestPaymentProvider(
            ProviderOperationOutcome.Succeeded);
        var idempotency = new TestIdempotencyService();
        var service = CreateService(
            context,
            provider,
            idempotency,
            new MutableCurrentUser(_clientUserId));

        var result = await service.FundAsync(
            milestone.Id,
            new FundMilestoneRequest("mock-card-success"),
            "fund-success-1",
            CancellationToken.None);

        Assert.Equal(EscrowHoldStatus.Funded, result.Status);
        Assert.Equal(1_000m, result.GrossAmount);
        Assert.Equal(50m, result.PlatformFee);
        Assert.Equal(950m, result.NetAmount);
        Assert.Equal(1, provider.DepositCalls);
        Assert.Equal(MilestoneStatus.FundedInProgress, milestone.Status);
        Assert.NotNull(milestone.FundedAt);

        var transaction = await context.PaymentTransactions.SingleAsync();
        var hold = await context.EscrowHolds.SingleAsync();
        var account = await context.EscrowAccounts.SingleAsync();
        var wallet = await context.LawyerWallets.SingleAsync();
        var ledger = await context.EscrowLedgerEntries.SingleAsync();
        var histories = await context.MilestoneStateHistories
            .OrderBy(item => item.CreatedAt)
            .ToListAsync();

        Assert.Equal(PaymentTransactionStatus.Completed, transaction.Status);
        Assert.Equal(hold.Id, transaction.EscrowHoldId);
        Assert.Equal(hold.Id, result.Id);
        Assert.Equal(1_000m, account.TotalDeposited);
        Assert.Equal(950m, wallet.PendingBalance);
        Assert.Equal(1_000m, ledger.Amount);
        Assert.Equal(1_000m, ledger.RunningBalance);
        Assert.Equal(
            [
                MilestoneStatus.FundingProcessing,
                MilestoneStatus.FundedInProgress
            ],
            histories.Select(item => item.NewStatus).ToArray());
        Assert.Equal(
            [
                ContractPaymentEventTypes.MilestoneFundingStarted,
                ContractPaymentEventTypes.MilestoneFunded
            ],
            await context.OutboxMessages
                .OrderBy(item => item.CreatedAt)
                .Select(item => item.EventType)
                .ToArrayAsync());
        Assert.Equal(IdempotencyStatus.Completed, idempotency.Status);
    }

    [Fact]
    public async Task FundAsync_ConfirmedFailureLeavesNoHoldAndReturnsToAwaitingFunding()
    {
        await using var context = CreateContext();
        var milestone = CreateMilestone();
        context.Milestones.Add(milestone);
        await context.SaveChangesAsync();
        var provider = new TestPaymentProvider(
            ProviderOperationOutcome.Failed);
        var idempotency = new TestIdempotencyService();
        var service = CreateService(
            context,
            provider,
            idempotency,
            new MutableCurrentUser(_clientUserId));

        var exception = await Assert.ThrowsAsync<BusinessException>(() =>
            service.FundAsync(
                milestone.Id,
                new FundMilestoneRequest("mock-fail-card"),
                "fund-failure-1",
                CancellationToken.None));

        Assert.Contains("رفض مزود الدفع", exception.Message);
        Assert.Matches("[\\u0600-\\u06FF]", exception.Message);
        Assert.Equal(MilestoneStatus.AwaitingFunding, milestone.Status);
        Assert.Null(milestone.FundedAt);
        Assert.Empty(await context.EscrowHolds.ToListAsync());
        Assert.Empty(await context.EscrowLedgerEntries.ToListAsync());
        Assert.Equal(
            PaymentTransactionStatus.Failed,
            (await context.PaymentTransactions.SingleAsync()).Status);
        Assert.Equal(IdempotencyStatus.Failed, idempotency.Status);
    }

    [Fact]
    public async Task FundAsync_UnknownProviderOutcomeRemainsProcessingWithoutHold()
    {
        await using var context = CreateContext();
        var milestone = CreateMilestone();
        context.Milestones.Add(milestone);
        await context.SaveChangesAsync();
        var provider = new TestPaymentProvider(
            ProviderOperationOutcome.Unknown);
        var idempotency = new TestIdempotencyService();
        var service = CreateService(
            context,
            provider,
            idempotency,
            new MutableCurrentUser(_clientUserId));

        var exception = await Assert.ThrowsAsync<BusinessException>(() =>
            service.FundAsync(
                milestone.Id,
                new FundMilestoneRequest("mock-timeout-card"),
                "fund-unknown-1",
                CancellationToken.None));

        Assert.Contains("غير مؤكدة", exception.Message);
        Assert.Matches("[\\u0600-\\u06FF]", exception.Message);
        Assert.Equal(MilestoneStatus.FundingProcessing, milestone.Status);
        Assert.Null(milestone.FundedAt);
        Assert.Empty(await context.EscrowHolds.ToListAsync());
        Assert.Empty(await context.EscrowLedgerEntries.ToListAsync());
        Assert.Equal(
            PaymentTransactionStatus.Processing,
            (await context.PaymentTransactions.SingleAsync()).Status);
        Assert.Equal(IdempotencyStatus.Processing, idempotency.Status);
    }

    [Fact]
    public async Task FundAsync_RequiresClientAndIdempotencyKeyBeforeProviderCall()
    {
        await using var context = CreateContext();
        var milestone = CreateMilestone();
        context.Milestones.Add(milestone);
        await context.SaveChangesAsync();
        var provider = new TestPaymentProvider(
            ProviderOperationOutcome.Succeeded);
        var idempotency = new TestIdempotencyService();
        var service = CreateService(
            context,
            provider,
            idempotency,
            new MutableCurrentUser(_lawyerUserId));

        var forbidden = await Assert.ThrowsAsync<ForbiddenAccessException>(() =>
            service.FundAsync(
                milestone.Id,
                new FundMilestoneRequest("mock-card-success"),
                "fund-forbidden-1",
                CancellationToken.None));
        Assert.Matches("[\\u0600-\\u06FF]", forbidden.Message);
        Assert.Equal(0, provider.DepositCalls);

        var clientService = CreateService(
            context,
            provider,
            idempotency,
            new MutableCurrentUser(_clientUserId));
        var missingKey = await Assert.ThrowsAsync<BusinessException>(() =>
            clientService.FundAsync(
                milestone.Id,
                new FundMilestoneRequest("mock-card-success"),
                null,
                CancellationToken.None));
        Assert.Contains("Idempotency-Key", missingKey.Message);
        Assert.Matches("[\\u0600-\\u06FF]", missingKey.Message);
        Assert.Equal(0, provider.DepositCalls);
    }

    [Fact]
    public async Task FundAsync_RejectsUnsettledEarlierOrActiveOtherMilestones()
    {
        await using var context = CreateContext();
        var earlier = CreateMilestone(1, MilestoneStatus.AwaitingFunding);
        var current = CreateMilestone(2, MilestoneStatus.AwaitingFunding);
        context.Milestones.AddRange(earlier, current);
        await context.SaveChangesAsync();
        var provider = new TestPaymentProvider(
            ProviderOperationOutcome.Succeeded);
        var idempotency = new TestIdempotencyService();
        var service = CreateService(
            context,
            provider,
            idempotency,
            new MutableCurrentUser(_clientUserId));

        var earlierException = await Assert.ThrowsAsync<BusinessException>(() =>
            service.FundAsync(
                current.Id,
                new FundMilestoneRequest("mock-card-success"),
                "fund-sequence-1",
                CancellationToken.None));
        Assert.Contains("المراحل السابقة", earlierException.Message);
        Assert.Equal(IdempotencyStatus.Failed, idempotency.Status);

        earlier.Status = MilestoneStatus.Released;
        var otherActive = CreateMilestone(3, MilestoneStatus.FundedInProgress);
        context.Milestones.Add(otherActive);
        await context.SaveChangesAsync();
        var otherActiveException = await Assert.ThrowsAsync<BusinessException>(() =>
            service.FundAsync(
                current.Id,
                new FundMilestoneRequest("mock-card-success"),
                "fund-sequence-2",
                CancellationToken.None));
        Assert.Contains("مرحلة", otherActiveException.Message);
        Assert.Equal(0, provider.DepositCalls);
    }

    [Fact]
    public async Task FundAsync_ReplaysCompletedIdempotentResponseWithoutCallingProvider()
    {
        await using var context = CreateContext();
        var milestone = CreateMilestone();
        context.Milestones.Add(milestone);
        await context.SaveChangesAsync();
        var provider = new TestPaymentProvider(
            ProviderOperationOutcome.Succeeded);
        var idempotency = new TestIdempotencyService();
        var service = CreateService(
            context,
            provider,
            idempotency,
            new MutableCurrentUser(_clientUserId));
        var request = new FundMilestoneRequest("mock-card-success");

        var first = await service.FundAsync(
            milestone.Id,
            request,
            "fund-replay-1",
            CancellationToken.None);
        var second = await service.FundAsync(
            milestone.Id,
            request,
            "fund-replay-1",
            CancellationToken.None);

        Assert.Equal(first, second);
        Assert.Equal(1, provider.DepositCalls);
        Assert.Single(await context.EscrowHolds.ToListAsync());
        Assert.Single(await context.PaymentTransactions.ToListAsync());
    }

    [Fact]
    public async Task HandleWebhookAsync_ValidSuccessCompletesUnknownFundingOnce()
    {
        await using var context = CreateContext();
        var (milestone, transaction) =
            await SeedProcessingFundingAsync(context);
        var service = CreateService(
            context,
            new TestPaymentProvider(ProviderOperationOutcome.Unknown),
            new TestIdempotencyService(),
            new MutableCurrentUser(_clientUserId));
        var request = CreateWebhookRequest(
            transaction.Id,
            PaymentTransactionStatus.Completed);
        var rawBody = JsonSerializer.Serialize(request);

        var first = await service.HandleWebhookAsync(
            request,
            request.EventId,
            TimestampHeader(),
            Signature(TimestampHeader(), rawBody),
            rawBody,
            CancellationToken.None);
        var duplicate = await service.HandleWebhookAsync(
            request,
            request.EventId,
            TimestampHeader(),
            Signature(TimestampHeader(), rawBody),
            rawBody,
            CancellationToken.None);

        Assert.Equal(
            PaymentTransactionStatus.Completed.ToString(),
            first.Status);
        Assert.Equal("Duplicate", duplicate.Status);
        Assert.Equal(MilestoneStatus.FundedInProgress, milestone.Status);
        Assert.Single(await context.EscrowHolds.ToListAsync());
        Assert.Single(await context.EscrowLedgerEntries.ToListAsync());
        Assert.Single(await context.PaymentWebhookEvents.ToListAsync());
    }

    [Fact]
    public async Task HandleWebhookAsync_ValidFailureReturnsMilestoneWithoutMoneyMovement()
    {
        await using var context = CreateContext();
        var (milestone, transaction) =
            await SeedProcessingFundingAsync(context);
        var service = CreateService(
            context,
            new TestPaymentProvider(ProviderOperationOutcome.Unknown),
            new TestIdempotencyService(),
            new MutableCurrentUser(_clientUserId));
        var request = CreateWebhookRequest(
            transaction.Id,
            PaymentTransactionStatus.Failed);
        var rawBody = JsonSerializer.Serialize(request);

        var result = await service.HandleWebhookAsync(
            request,
            request.EventId,
            TimestampHeader(),
            Signature(TimestampHeader(), rawBody),
            rawBody,
            CancellationToken.None);

        Assert.Equal(
            PaymentTransactionStatus.Failed.ToString(),
            result.Status);
        Assert.Equal(MilestoneStatus.AwaitingFunding, milestone.Status);
        Assert.Equal(PaymentTransactionStatus.Failed, transaction.Status);
        Assert.Empty(await context.EscrowHolds.ToListAsync());
        Assert.Empty(await context.EscrowLedgerEntries.ToListAsync());
        Assert.Single(await context.PaymentWebhookEvents.ToListAsync());
    }

    [Fact]
    public async Task HandleWebhookAsync_RejectsInvalidSignatureWithoutMutation()
    {
        await using var context = CreateContext();
        var (milestone, transaction) =
            await SeedProcessingFundingAsync(context);
        var service = CreateService(
            context,
            new TestPaymentProvider(ProviderOperationOutcome.Unknown),
            new TestIdempotencyService(),
            new MutableCurrentUser(_clientUserId));
        var request = CreateWebhookRequest(
            transaction.Id,
            PaymentTransactionStatus.Completed);
        var rawBody = JsonSerializer.Serialize(request);

        var exception = await Assert.ThrowsAsync<BusinessException>(() =>
            service.HandleWebhookAsync(
                request,
                request.EventId,
                TimestampHeader(),
                "v1=AAAA",
                rawBody,
                CancellationToken.None));

        Assert.Contains("توقيع", exception.Message);
        Assert.Equal(MilestoneStatus.FundingProcessing, milestone.Status);
        Assert.Equal(
            PaymentTransactionStatus.Processing,
            transaction.Status);
        Assert.Empty(await context.PaymentWebhookEvents.ToListAsync());
        Assert.Empty(await context.EscrowHolds.ToListAsync());
    }

    [Fact]
    public async Task HandleWebhookAsync_RejectsAlteredAmountWithoutMutation()
    {
        await using var context = CreateContext();
        var (milestone, transaction) =
            await SeedProcessingFundingAsync(context);
        var service = CreateService(
            context,
            new TestPaymentProvider(ProviderOperationOutcome.Unknown),
            new TestIdempotencyService(),
            new MutableCurrentUser(_clientUserId));
        var request = CreateWebhookRequest(
            transaction.Id,
            PaymentTransactionStatus.Completed) with
        {
            Amount = 999m
        };
        var rawBody = JsonSerializer.Serialize(request);

        var exception = await Assert.ThrowsAsync<BusinessException>(() =>
            service.HandleWebhookAsync(
                request,
                request.EventId,
                TimestampHeader(),
                Signature(TimestampHeader(), rawBody),
                rawBody,
                CancellationToken.None));

        Assert.Contains("قيمة", exception.Message);
        Assert.Equal(MilestoneStatus.FundingProcessing, milestone.Status);
        Assert.Equal(
            PaymentTransactionStatus.Processing,
            transaction.Status);
        Assert.Empty(await context.PaymentWebhookEvents.ToListAsync());
        Assert.Empty(await context.EscrowHolds.ToListAsync());
    }

    [Fact]
    public async Task ReconcileProviderTransactionAsync_FinalizesConfirmedSuccess()
    {
        await using var context = CreateContext();
        var (milestone, transaction) =
            await SeedProcessingFundingAsync(context);
        var service = CreateService(
            context,
            new TestPaymentProvider(
                ProviderOperationOutcome.Succeeded),
            new TestIdempotencyService(),
            new MutableCurrentUser(_clientUserId));

        var result = await service.ReconcileProviderTransactionAsync(
            transaction.Id,
            CancellationToken.None);

        Assert.Equal(
            JobExecutionOutcome.Completed,
            result.Outcome);
        Assert.Equal(MilestoneStatus.FundedInProgress, milestone.Status);
        Assert.Equal(
            PaymentTransactionStatus.Completed,
            transaction.Status);
        Assert.Single(await context.EscrowHolds.ToListAsync());
        Assert.Single(await context.EscrowLedgerEntries.ToListAsync());
    }

    [Fact]
    public async Task ReconcileProviderTransactionAsync_LeavesUnknownOutcomeProcessing()
    {
        await using var context = CreateContext();
        var (milestone, transaction) =
            await SeedProcessingFundingAsync(context);
        var service = CreateService(
            context,
            new TestPaymentProvider(
                ProviderOperationOutcome.Unknown),
            new TestIdempotencyService(),
            new MutableCurrentUser(_clientUserId));

        var result = await service.ReconcileProviderTransactionAsync(
            transaction.Id,
            CancellationToken.None);

        Assert.Equal(JobExecutionOutcome.NoOp, result.Outcome);
        Assert.Equal(
            "ProviderOutcomeStillUnknown",
            result.Reason);
        Assert.Equal(MilestoneStatus.FundingProcessing, milestone.Status);
        Assert.Equal(
            PaymentTransactionStatus.Processing,
            transaction.Status);
        Assert.Empty(await context.EscrowHolds.ToListAsync());
    }

    [Fact]
    public async Task GetContractPaymentsAsync_ParticipantSeesOnlyOwnContractHistory()
    {
        await using var context = CreateContext();
        var (_, transaction) =
            await SeedProcessingFundingAsync(context);
        var service = CreateService(
            context,
            new TestPaymentProvider(
                ProviderOperationOutcome.Unknown),
            new TestIdempotencyService(),
            new MutableCurrentUser(_clientUserId));

        var history = await service.GetContractPaymentsAsync(
            _contractId,
            CancellationToken.None);

        var attempt = Assert.Single(history.Attempts);
        Assert.Equal(transaction.Id, attempt.Id);
        Assert.Empty(history.Payments);
        Assert.Empty(history.LedgerEntries);

        var outsiderService = CreateService(
            context,
            new TestPaymentProvider(
                ProviderOperationOutcome.Unknown),
            new TestIdempotencyService(),
            new MutableCurrentUser(Guid.NewGuid()));
        await Assert.ThrowsAsync<ForbiddenAccessException>(() =>
            outsiderService.GetContractPaymentsAsync(
                _contractId,
                CancellationToken.None));
    }

    [Fact]
    public async Task GetContractPaymentsAsync_FinanceOperatorCanInspectContract()
    {
        await using var context = CreateContext();
        await SeedProcessingFundingAsync(context);
        var financeUserId = Guid.NewGuid();
        var service = CreateService(
            context,
            new TestPaymentProvider(
                ProviderOperationOutcome.Unknown),
            new TestIdempotencyService(),
            new MutableCurrentUser(financeUserId),
            financeAccess: true);

        var history = await service.GetContractPaymentsAsync(
            _contractId,
            CancellationToken.None);

        Assert.Single(history.Attempts);
    }

    [Fact]
    public async Task RetryAsync_SuccessCreatesNewAttemptAndPreservesFailedAttempt()
    {
        await using var context = CreateContext();
        var (milestone, originalTransaction) =
            await SeedFailedFundingAsync(context);
        var financeUserId = Guid.NewGuid();
        var provider = new TestPaymentProvider(
            ProviderOperationOutcome.Succeeded);
        var service = CreateService(
            context,
            provider,
            new TestIdempotencyService(),
            new MutableCurrentUser(financeUserId),
            financeAccess: true);

        var result = await service.RetryAsync(
            originalTransaction.Id,
            "retry-payment-1",
            CancellationToken.None);

        Assert.Equal(EscrowHoldStatus.Funded, result.Status);
        Assert.Equal(MilestoneStatus.FundedInProgress, milestone.Status);
        Assert.Equal(1, provider.DepositCalls);
        var attempts = await context.PaymentTransactions
            .OrderBy(transaction => transaction.CreatedAt)
            .ToListAsync();
        Assert.Equal(2, attempts.Count);
        Assert.Equal(originalTransaction.Id, attempts[0].Id);
        Assert.Equal(
            PaymentTransactionStatus.Failed,
            attempts[0].Status);
        Assert.Equal(
            PaymentTransactionStatus.Completed,
            attempts[1].Status);
        Assert.NotEqual(
            attempts[0].IdempotencyKey,
            attempts[1].IdempotencyKey);

        var milestonePayment =
            await service.GetMilestonePaymentAsync(
                milestone.Id,
                CancellationToken.None);
        Assert.Equal(result, milestonePayment);
    }

    [Fact]
    public async Task RetryAsync_NonFinanceUserIsRejectedWithoutNewAttempt()
    {
        await using var context = CreateContext();
        var (_, originalTransaction) =
            await SeedFailedFundingAsync(context);
        var service = CreateService(
            context,
            new TestPaymentProvider(
                ProviderOperationOutcome.Succeeded),
            new TestIdempotencyService(),
            new MutableCurrentUser(_clientUserId));

        await Assert.ThrowsAsync<ForbiddenAccessException>(() =>
            service.RetryAsync(
                originalTransaction.Id,
                "retry-payment-unauthorized",
                CancellationToken.None));

        Assert.Single(
            await context.PaymentTransactions.ToListAsync());
        Assert.Empty(await context.EscrowHolds.ToListAsync());
    }

    private async Task<(Milestone Milestone, PaymentTransaction Transaction)>
        SeedProcessingFundingAsync(ApplicationDbContext context)
    {
        var contract = new Contract(
            _contractId,
            Guid.NewGuid(),
            Guid.NewGuid(),
            _clientUserId,
            _lawyerUserId,
            "عقد تمثيل قانوني",
            "الشروط والأحكام المعتمدة.",
            Now.AddDays(-2))
        {
            Status = ContractStatus.Active,
            ActivatedAt = Now.AddDays(-1)
        };
        var milestone = CreateMilestone(
            status: MilestoneStatus.FundingProcessing);
        var transaction = new PaymentTransaction(
            Guid.NewGuid(),
            _contractId,
            milestone.Id,
            PaymentOperationType.Deposit,
            "TestPaymentProvider",
            "provider-idempotency-key",
            milestone.Amount,
            Now.AddMinutes(-10));
        context.Contracts.Add(contract);
        context.Milestones.Add(milestone);
        context.PaymentTransactions.Add(transaction);
        await context.SaveChangesAsync();
        return (milestone, transaction);
    }

    private async Task<(Milestone Milestone, PaymentTransaction Transaction)>
        SeedFailedFundingAsync(ApplicationDbContext context)
    {
        var (milestone, transaction) =
            await SeedProcessingFundingAsync(context);
        milestone.Status = MilestoneStatus.AwaitingFunding;
        transaction.Status = PaymentTransactionStatus.Failed;
        transaction.FailureReason = "declined";
        transaction.ProcessedAt = Now.AddMinutes(-5);
        transaction.UpdatedAt = transaction.ProcessedAt.Value;
        await context.SaveChangesAsync();
        return (milestone, transaction);
    }

    private static PaymentWebhookRequest CreateWebhookRequest(
        Guid paymentTransactionId,
        PaymentTransactionStatus status)
        => new(
            $"event-{Guid.NewGuid():N}",
            paymentTransactionId,
            $"provider-{Guid.NewGuid():N}",
            status,
            1_000m,
            "EGP",
            Now,
            status == PaymentTransactionStatus.Failed
                ? "declined"
                : null);

    private static string TimestampHeader()
        => new DateTimeOffset(Now).ToUnixTimeSeconds().ToString();

    private static string Signature(
        string timestamp,
        string rawBody)
        => $"v1={Convert.ToBase64String(
            HMACSHA256.HashData(
                Encoding.UTF8.GetBytes(
                    "local-mock-payment-webhook-secret"),
                Encoding.UTF8.GetBytes(
                    $"{timestamp}.{rawBody}")))}";

    private PaymentEscrowService CreateService(
        ApplicationDbContext context,
        TestPaymentProvider provider,
        TestIdempotencyService idempotency,
        MutableCurrentUser currentUser,
        bool financeAccess = false)
    {
        var timeProvider = new FixedTimeProvider(Now);
        return new PaymentEscrowService(
            context,
            currentUser,
            new ContractServiceStub(CreateContract()),
            new TestUserEligibilityService(
                currentUser.UserId,
                financeAccess),
            provider,
            provider,
            idempotency,
            new OutboxWriter(context, timeProvider),
            Options.Create(new PaymentProviderOptions()),
            NullLogger<PaymentEscrowService>.Instance,
            timeProvider);
    }

    private ContractDetailDto CreateContract()
        => new(
            _contractId,
            Guid.NewGuid(),
            Guid.NewGuid(),
            _clientUserId,
            _lawyerUserId,
            "عقد تمثيل قانوني",
            "الشروط والأحكام المعتمدة.",
            "EGP",
            ContractStatus.Active,
            null,
            null,
            Now,
            null,
            null,
            1_000m,
            [],
            [],
            []);

    private Milestone CreateMilestone(
        int orderNumber = 1,
        MilestoneStatus status = MilestoneStatus.AwaitingFunding)
        => new(
            Guid.NewGuid(),
            _contractId,
            $"المرحلة {orderNumber}",
            null,
            orderNumber,
            1_000m,
            14,
            Now.AddDays(14),
            Now.AddHours(-1))
        {
            Status = status,
            AcceptedByClientAt = Now.AddHours(-3),
            AcceptedByLawyerAt = Now.AddHours(-2),
            ReadyForFundingAt = Now.AddHours(-1),
            RowVersion = [1, 2, 3, (byte)orderNumber]
        };

    private ApplicationDbContext CreateContext()
        => new(
            new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase($"payment-escrow-{Guid.NewGuid():N}")
                .Options,
            new FixedTimeProvider(Now));

    private sealed class MutableCurrentUser(Guid userId)
        : ICurrentUserService
    {
        public Guid? UserId { get; set; } = userId;
        public bool IsAuthenticated => UserId.HasValue;
    }

    private sealed class TestPaymentProvider(
        ProviderOperationOutcome outcome)
        : IPaymentProvider, IPaymentReconciliationProvider
    {
        public string Name => "TestPaymentProvider";
        public int DepositCalls { get; private set; }
        public bool ThrowOnDeposit { get; set; }

        public Task<ProviderResult> DepositAsync(
            ProviderDepositRequest request,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            DepositCalls++;
            if (ThrowOnDeposit)
            {
                throw new InvalidOperationException("provider timeout");
            }

            return Task.FromResult(
                new ProviderResult(
                    request.Amount,
                    request.Currency,
                    request.BusinessId,
                    request.ProviderIdempotencyKey,
                    request.CorrelationId,
                    outcome,
                    outcome == ProviderOperationOutcome.Succeeded
                        ? "provider-transaction-1"
                        : null,
                    outcome == ProviderOperationOutcome.Failed
                        ? "declined"
                        : null));
        }

        public Task<ProviderResult> RetryDepositAsync(
            ProviderDepositRetryRequest request,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            DepositCalls++;
            return Task.FromResult(
                new ProviderResult(
                    request.Amount,
                    request.Currency,
                    request.BusinessId,
                    request.ProviderIdempotencyKey,
                    request.CorrelationId,
                    outcome,
                    outcome == ProviderOperationOutcome.Succeeded
                        ? "provider-retry-transaction-1"
                        : null,
                    outcome == ProviderOperationOutcome.Failed
                        ? "declined"
                        : null));
        }

        public Task<ProviderResult> ReleaseAsync(
            ProviderReleaseRequest request,
            CancellationToken cancellationToken)
            => throw new NotSupportedException();

        public Task<ProviderResult> RefundAsync(
            ProviderRefundRequest request,
            CancellationToken cancellationToken)
            => throw new NotSupportedException();

        public Task<ProviderResult> WithdrawAsync(
            ProviderWithdrawalRequest request,
            CancellationToken cancellationToken)
            => throw new NotSupportedException();

        public Task<ProviderResult?> GetDepositStatusAsync(
            ProviderDepositStatusRequest request,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult<ProviderResult?>(
                new ProviderResult(
                    request.Amount,
                    request.Currency,
                    request.BusinessId,
                    request.ProviderIdempotencyKey,
                    request.CorrelationId,
                    outcome,
                    outcome == ProviderOperationOutcome.Succeeded
                        ? "provider-reconciled-transaction"
                        : null,
                    null));
        }
    }

    private sealed class TestUserEligibilityService(
        Guid? userId,
        bool financeAccess)
        : IContractUserEligibilityService
    {
        public Task<ContractUserEligibilityFacts?> FindEligibilityAsync(
            Guid requestedUserId,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (!userId.HasValue
                || requestedUserId != userId.Value)
            {
                return Task.FromResult<
                    ContractUserEligibilityFacts?>(null);
            }

            return Task.FromResult<ContractUserEligibilityFacts?>(
                new ContractUserEligibilityFacts(
                    requestedUserId,
                    true,
                    false,
                    false,
                    false,
                    financeAccess,
                    false));
        }
    }

    private sealed class TestIdempotencyService : IIdempotencyService
    {
        private IdempotencyReservation? _reservation;
        private string? _responseBody;
        private Guid? _resultReferenceId;

        public IdempotencyStatus Status =>
            _reservation?.Status ?? IdempotencyStatus.Processing;

        public Task<IdempotencyReservation> ReserveAsync<TRequest>(
            IdempotencyScope scope,
            string? idempotencyKey,
            TRequest request,
            CancellationToken cancellationToken)
        {
            if (_reservation is not null)
            {
                if (_reservation.Status == IdempotencyStatus.Processing)
                {
                    throw new BusinessException(
                        "الطلب السابق قيد المعالجة.");
                }

                return Task.FromResult(
                    _reservation with
                    {
                        State = IdempotencyReservationState.Replay,
                        ResponseBody = _responseBody,
                        ResultReferenceId = _resultReferenceId
                    });
            }

            _reservation = new IdempotencyReservation(
                Guid.NewGuid(),
                IdempotencyReservationState.Acquired,
                "hash",
                IdempotencyStatus.Processing,
                null,
                null,
                null);
            return Task.FromResult(_reservation);
        }

        public Task CompleteAsync<TResponse>(
            Guid recordId,
            int responseStatusCode,
            TResponse response,
            Guid? resultReferenceId,
            CancellationToken cancellationToken)
        {
            _responseBody = JsonSerializer.Serialize(response);
            _resultReferenceId = resultReferenceId;
            _reservation = _reservation! with
            {
                Status = IdempotencyStatus.Completed,
                ResponseStatusCode = responseStatusCode,
                ResponseBody = _responseBody,
                ResultReferenceId = resultReferenceId
            };
            return Task.CompletedTask;
        }

        public Task FailAsync<TResponse>(
            Guid recordId,
            int responseStatusCode,
            TResponse response,
            Guid? resultReferenceId,
            CancellationToken cancellationToken)
        {
            _responseBody = JsonSerializer.Serialize(response);
            _resultReferenceId = resultReferenceId;
            _reservation = _reservation! with
            {
                Status = IdempotencyStatus.Failed,
                ResponseStatusCode = responseStatusCode,
                ResponseBody = _responseBody,
                ResultReferenceId = resultReferenceId
            };
            return Task.CompletedTask;
        }

        public Task<int> PurgeExpiredResponseBodiesAsync(
            CancellationToken cancellationToken)
            => Task.FromResult(0);
    }

    private sealed class ContractServiceStub(
        ContractDetailDto contract) : IContractService
    {
        public Task<ContractDetailDto> GetAsync(
            Guid contractId,
            CancellationToken cancellationToken)
            => Task.FromResult(contract);

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

        public Task<ContractActionResultDto> EvaluateActivationAsync(
            Guid contractId,
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
