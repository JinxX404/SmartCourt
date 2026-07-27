using System.Linq.Expressions;
using SmartCourt.Common.Exceptions;
using SmartCourt.Features.Milestones;
using SmartCourt.Features.Payments;
using SmartCourt.Infrastructure.Providers.Events;
using SmartCourt.Infrastructure.Providers.Jobs;
using SmartCourt.Interfaces.Providers;
using SmartCourt.Providers.Jobs;
using Xunit;

namespace SmartCourt.Tests.Infrastructure.Providers;

public sealed class ContractJobInfrastructureTests
{
    private static readonly DateTime RunAtUtc =
        new(2026, 8, 10, 9, 30, 0, DateTimeKind.Utc);

    [Fact]
    public async Task Scheduler_RoutesEveryJobThroughDelayedAsyncProvider()
    {
        var backgroundJobs = new RecordingBackgroundJobProvider();
        var scheduler = new HangfireContractJobScheduler(backgroundJobs);
        var milestoneId = Guid.NewGuid();
        var holdId = Guid.NewGuid();
        var transactionId = Guid.NewGuid();

        await scheduler.ScheduleAutoAcceptAsync(
            milestoneId,
            holdId,
            3,
            RunAtUtc,
            CancellationToken.None);
        await scheduler.ScheduleReleaseExpiredHoldAsync(
            holdId,
            RunAtUtc,
            CancellationToken.None);
        await scheduler.ScheduleProviderReconciliationAsync(
            transactionId,
            RunAtUtc,
            CancellationToken.None);
        await scheduler.ScheduleProviderRetryAsync(
            transactionId,
            RunAtUtc,
            CancellationToken.None);
        await scheduler.ScheduleSchedulingReconciliationAsync(
            RunAtUtc,
            CancellationToken.None);
        await scheduler.SchedulePendingWalletProjectionReconciliationAsync(
            RunAtUtc,
            CancellationToken.None);
        await scheduler.ScheduleOutboxDispatchAsync(
            25,
            RunAtUtc,
            CancellationToken.None);

        Assert.Equal(
            new[]
            {
                nameof(IContractJobService.AutoAcceptMilestoneAsync),
                nameof(IContractJobService.ReleaseExpiredHoldAsync),
                nameof(IContractJobService.ReconcileProviderTransactionAsync),
                nameof(IContractJobService.RetryProviderTransactionAsync),
                nameof(IContractJobService.ReconcileMissingSchedulesAsync),
                nameof(IContractJobService.ReconcilePendingWalletProjectionsAsync),
                nameof(IContractJobService.DispatchOutboxAsync)
            },
            backgroundJobs.ScheduledMethods);
        Assert.All(
            backgroundJobs.RunAtValues,
            runAt => Assert.Equal(new DateTimeOffset(RunAtUtc), runAt));
    }

    [Fact]
    public async Task Scheduler_RejectsInvalidArgumentsBeforeCreatingJob()
    {
        var backgroundJobs = new RecordingBackgroundJobProvider();
        var scheduler = new HangfireContractJobScheduler(backgroundJobs);

        await Assert.ThrowsAsync<BusinessException>(
            () => scheduler.ScheduleAutoAcceptAsync(
                Guid.NewGuid(),
                Guid.NewGuid(),
                0,
                RunAtUtc,
                CancellationToken.None));
        await Assert.ThrowsAsync<BusinessException>(
            () => scheduler.ScheduleOutboxDispatchAsync(
                0,
                RunAtUtc,
                CancellationToken.None));
        await Assert.ThrowsAsync<BusinessException>(
            () => scheduler.ScheduleSchedulingReconciliationAsync(
                DateTime.SpecifyKind(RunAtUtc, DateTimeKind.Local),
                CancellationToken.None));

        Assert.Empty(backgroundJobs.ScheduledMethods);
    }

    [Fact]
    public async Task Scheduler_PropagatesCancellation()
    {
        var scheduler = new HangfireContractJobScheduler(
            new RecordingBackgroundJobProvider());
        using var source = new CancellationTokenSource();
        source.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => scheduler.ScheduleReleaseExpiredHoldAsync(
                Guid.NewGuid(),
                RunAtUtc,
                source.Token));
    }

    [Fact]
    public async Task JobFacade_ForwardsStructuredNoOpOnRepeatedStaleDelivery()
    {
        var operations = new RecordingJobOperations(
            JobExecutionResult.NoOp("SubmissionVersionIsStale"));
        var service = new ContractJobService(
            [operations],
            new RecordingOutboxDispatcher(),
            new RecordingScheduleReconciliation());
        var milestoneId = Guid.NewGuid();
        var holdId = Guid.NewGuid();

        var results = await Task.WhenAll(
            service.AutoAcceptMilestoneAsync(
                milestoneId,
                holdId,
                2,
                CancellationToken.None),
            service.AutoAcceptMilestoneAsync(
                milestoneId,
                holdId,
                2,
                CancellationToken.None));

        Assert.All(
            results,
            result =>
            {
                Assert.Equal(JobExecutionOutcome.NoOp, result.Outcome);
                Assert.Equal("SubmissionVersionIsStale", result.Reason);
            });
        Assert.Equal(2, operations.AutoAcceptCalls);
    }

    [Fact]
    public async Task OutboxJob_ReturnsStructuredNoOpOrCompletedCount()
    {
        var emptyDispatcher = new RecordingOutboxDispatcher();
        var emptyService = new ContractJobService(
            [],
            emptyDispatcher,
            new RecordingScheduleReconciliation());

        var emptyResult = await emptyService.DispatchOutboxAsync(
            20,
            CancellationToken.None);

        Assert.Equal(JobExecutionOutcome.NoOp, emptyResult.Outcome);
        Assert.Equal("NoOutboxMessagesAvailable", emptyResult.Reason);

        var populatedService = new ContractJobService(
            [],
            new RecordingOutboxDispatcher(4),
            new RecordingScheduleReconciliation());
        var populatedResult = await populatedService.DispatchOutboxAsync(
            20,
            CancellationToken.None);

        Assert.Equal(JobExecutionOutcome.Completed, populatedResult.Outcome);
        Assert.Equal(4, populatedResult.AffectedCount);
    }

    [Fact]
    public async Task MissingDomainOperations_FailsForRetryInsteadOfSilentNoOp()
    {
        var service = new ContractJobService(
            [],
            new RecordingOutboxDispatcher(),
            new RecordingScheduleReconciliation());

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.ReleaseExpiredHoldAsync(
                Guid.NewGuid(),
                CancellationToken.None));
    }

    private sealed class RecordingBackgroundJobProvider
        : IBackgroundJobProvider
    {
        public List<string> ScheduledMethods { get; } = [];
        public List<DateTimeOffset> RunAtValues { get; } = [];

        public string Enqueue(Expression<Action> methodCall)
        {
            throw new NotSupportedException();
        }

        public string Enqueue<T>(Expression<Action<T>> methodCall)
        {
            throw new NotSupportedException();
        }

        public string Enqueue<T>(Expression<Func<T, Task>> methodCall)
        {
            throw new NotSupportedException();
        }

        public Task<string> EnqueueAsync<T>(
            Expression<Func<T, Task>> methodCall,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult("enqueued");
        }

        public Task<string> ScheduleAsync<T>(
            Expression<Func<T, Task>> methodCall,
            DateTimeOffset runAt,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ScheduledMethods.Add(GetMethodCall(methodCall).Method.Name);
            RunAtValues.Add(runAt);
            return Task.FromResult($"scheduled-{ScheduledMethods.Count}");
        }

        private static MethodCallExpression GetMethodCall<T>(
            Expression<Func<T, Task>> methodCall)
        {
            return methodCall.Body switch
            {
                MethodCallExpression call => call,
                UnaryExpression { Operand: MethodCallExpression call } => call,
                _ => throw new InvalidOperationException(
                    "Expected a scheduled service method call.")
            };
        }
    }

    private sealed class RecordingJobOperations(JobExecutionResult result)
        : IContractJobOperations
    {
        private int _autoAcceptCalls;

        public int AutoAcceptCalls => _autoAcceptCalls;

        public Task<JobExecutionResult> AutoAcceptMilestoneAsync(
            Guid milestoneId,
            Guid escrowHoldId,
            int submissionVersion,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            Interlocked.Increment(ref _autoAcceptCalls);
            return Task.FromResult(result);
        }

        public Task<JobExecutionResult> ReleaseExpiredHoldAsync(
            Guid escrowHoldId,
            CancellationToken cancellationToken)
        {
            return Task.FromResult(result);
        }

        public Task<JobExecutionResult> ReconcileProviderTransactionAsync(
            Guid paymentTransactionId,
            CancellationToken cancellationToken)
        {
            return Task.FromResult(result);
        }

        public Task<JobExecutionResult> RetryProviderTransactionAsync(
            Guid paymentTransactionId,
            CancellationToken cancellationToken)
        {
            return Task.FromResult(result);
        }

        public Task<JobExecutionResult> ReconcilePendingWalletProjectionsAsync(
            CancellationToken cancellationToken)
        {
            return Task.FromResult(result);
        }
    }

    private sealed class RecordingOutboxDispatcher(int dispatched = 0)
        : IOutboxDispatcher
    {
        public Task<int> DispatchAvailableAsync(
            int batchSize,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult(dispatched);
        }

        public Task DispatchAsync(
            Guid outboxMessageId,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.CompletedTask;
        }
    }

    private sealed class RecordingScheduleReconciliation
        : IMilestoneSchedulingReconciliationService
    {
        public Task<JobExecutionResult> ReconcileAsync(
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult(
                JobExecutionResult.NoOp("NoMissingSchedulesFound"));
        }
    }
}
