using Microsoft.EntityFrameworkCore;
using SmartCourt.Common.Exceptions;
using SmartCourt.Infrastructure.Persistence.Enums;
using SmartCourt.Infrastructure.Providers.Events;
using SmartCourt.Persistence;
using Xunit;

namespace SmartCourt.Tests.Infrastructure.Providers;

public sealed class OutboxIntegrationTests : IAsyncLifetime
{
    private readonly string _databaseName =
        $"SmartCourtOutboxTests_{Guid.NewGuid():N}";
    private readonly DateTime _initialUtc =
        new(2026, 8, 2, 12, 0, 0, DateTimeKind.Utc);

    public async Task InitializeAsync()
    {
        await using var context = CreateContext(_initialUtc);
        await context.Database.EnsureCreatedAsync();
    }

    public async Task DisposeAsync()
    {
        await using var context = CreateContext(_initialUtc);
        await context.Database.EnsureDeletedAsync();
    }

    [Fact]
    public async Task RolledBackDomainTransaction_LeavesNoOutboxMessage()
    {
        await using var context = CreateContext(_initialUtc);
        await using var transaction =
            await context.Database.BeginTransactionAsync();
        var writer = new OutboxWriter(
            context,
            new FixedTimeProvider(_initialUtc));

        await writer.EnqueueAsync(
            CreateEvent(),
            CancellationToken.None);
        await context.SaveChangesAsync();
        await transaction.RollbackAsync();

        await using var verifyContext = CreateContext(_initialUtc);
        Assert.Empty(await verifyContext.OutboxMessages.ToListAsync());
    }

    [Fact]
    public async Task DuplicateDispatch_DoesNotInvokeHandlerTwice()
    {
        var messageId = await EnqueueAsync();
        var handler = new RecordingHandler(
            ContractPaymentEventTypes.ContractCreated);
        await using var context = CreateContext(_initialUtc);
        var dispatcher = new OutboxDispatcher(
            context,
            [handler],
            new FixedTimeProvider(_initialUtc));

        await dispatcher.DispatchAsync(
            messageId,
            CancellationToken.None);
        await dispatcher.DispatchAsync(
            messageId,
            CancellationToken.None);

        Assert.Equal(1, handler.CallCount);
        var message = await context.OutboxMessages
            .SingleAsync(item => item.Id == messageId);
        Assert.Equal(OutboxStatus.Processed, message.Status);
        Assert.NotNull(message.ProcessedAt);
    }

    [Fact]
    public async Task MultipleHandlers_ReceiveOneDeliveryEach()
    {
        var messageId = await EnqueueAsync();
        var firstHandler = new RecordingHandler(
            ContractPaymentEventTypes.ContractCreated);
        var secondHandler = new RecordingHandler(
            ContractPaymentEventTypes.ContractCreated);
        await using var context = CreateContext(_initialUtc);
        var dispatcher = new OutboxDispatcher(
            context,
            [firstHandler, secondHandler],
            new FixedTimeProvider(_initialUtc));

        await dispatcher.DispatchAsync(
            messageId,
            CancellationToken.None);
        await dispatcher.DispatchAsync(
            messageId,
            CancellationToken.None);

        Assert.Equal(1, firstHandler.CallCount);
        Assert.Equal(1, secondHandler.CallCount);
    }

    [Fact]
    public async Task FailedDelivery_RemainsRetryableWithBackoff()
    {
        var messageId = await EnqueueAsync();
        var failingHandler = new RecordingHandler(
            ContractPaymentEventTypes.ContractCreated)
        {
            FailFirstCall = true
        };
        await using (var context = CreateContext(_initialUtc))
        {
            var dispatcher = new OutboxDispatcher(
                context,
                [failingHandler],
                new FixedTimeProvider(_initialUtc));
            await dispatcher.DispatchAsync(
                messageId,
                CancellationToken.None);

            var failed = await context.OutboxMessages
                .SingleAsync(item => item.Id == messageId);
            Assert.Equal(OutboxStatus.Failed, failed.Status);
            Assert.Equal(1, failed.Attempts);
            Assert.Contains("simulated", failed.LastError);
            Assert.True(failed.AvailableAt > _initialUtc);
        }

        var retryUtc = _initialUtc.AddMinutes(1);
        await using var retryContext = CreateContext(retryUtc);
        var retryDispatcher = new OutboxDispatcher(
            retryContext,
            [failingHandler],
            new FixedTimeProvider(retryUtc));
        await retryDispatcher.DispatchAsync(
            messageId,
            CancellationToken.None);

        var processed = await retryContext.OutboxMessages
            .SingleAsync(item => item.Id == messageId);
        Assert.Equal(OutboxStatus.Processed, processed.Status);
        Assert.Equal(2, processed.Attempts);
        Assert.Equal(2, failingHandler.CallCount);
    }

    [Fact]
    public async Task SensitivePaymentAndEvidenceFields_AreRejected()
    {
        await using var context = CreateContext(_initialUtc);
        var writer = new OutboxWriter(
            context,
            new FixedTimeProvider(_initialUtc));

        await Assert.ThrowsAsync<BusinessException>(
            () => writer.EnqueueAsync(
                CreateEvent(payload: new
                {
                    paymentMethodReference = "card-token"
                }),
                CancellationToken.None));
        await Assert.ThrowsAsync<BusinessException>(
            () => writer.EnqueueAsync(
                CreateEvent(payload: new
                {
                    evidenceFileId = Guid.NewGuid()
                }),
                CancellationToken.None));
    }

    [Fact]
    public async Task SubmissionEventPayload_PreservesHoldAndVersionContext()
    {
        await using var context = CreateContext(_initialUtc);
        var writer = new OutboxWriter(
            context,
            new FixedTimeProvider(_initialUtc));
        var payload = new MilestoneSubmissionEventPayload(
            Guid.NewGuid(),
            Guid.NewGuid(),
            4);

        var message = await writer.EnqueueAsync(
            CreateEvent(
                ContractPaymentEventTypes.MilestoneSubmitted,
                payload),
            CancellationToken.None);

        Assert.Contains("\"submissionVersion\":4", message.Payload);
        Assert.Contains(
            $"\"escrowHoldId\":\"{payload.EscrowHoldId}\"",
            message.Payload);
    }

    private async Task<Guid> EnqueueAsync()
    {
        await using var context = CreateContext(_initialUtc);
        var writer = new OutboxWriter(
            context,
            new FixedTimeProvider(_initialUtc));
        var message = await writer.EnqueueAsync(
            CreateEvent(),
            CancellationToken.None);
        await context.SaveChangesAsync();
        return message.Id;
    }

    private OutboxEvent CreateEvent(
        string eventType = ContractPaymentEventTypes.ContractCreated,
        object? payload = null)
    {
        return new OutboxEvent(
            eventType,
            1,
            payload ?? new ContractPaymentAggregateEventPayload(Guid.NewGuid()),
            "Contract",
            Guid.NewGuid(),
            Guid.NewGuid());
    }

    private ApplicationDbContext CreateContext(DateTime utcNow)
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlServer(ConnectionString)
            .Options;
        return new ApplicationDbContext(
            options,
            new FixedTimeProvider(utcNow));
    }

    private string ConnectionString =>
        $"Server=localhost;Database={_databaseName};Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=True;";

    private sealed class RecordingHandler(string eventType)
        : IOutboxEventHandler
    {
        public IReadOnlyCollection<string> EventTypes => [eventType];
        public bool FailFirstCall { get; set; }
        public int CallCount { get; private set; }

        public Task HandleAsync(
            SmartCourt.Infrastructure.Persistence.Entities.OutboxMessage message,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            CallCount++;
            if (FailFirstCall && CallCount == 1)
            {
                throw new InvalidOperationException("simulated handler failure");
            }

            return Task.CompletedTask;
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
