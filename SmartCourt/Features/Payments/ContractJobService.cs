using SmartCourt.Infrastructure.Providers.Events;
using SmartCourt.Infrastructure.Providers.Jobs;
using SmartCourt.Features.Milestones;

namespace SmartCourt.Features.Payments;

public sealed class ContractJobService : IContractJobService
{
    private readonly IReadOnlyCollection<IContractJobOperations> _operations;
    private readonly IOutboxDispatcher _outboxDispatcher;
    private readonly IMilestoneSchedulingReconciliationService
        _schedulingReconciliation;

    public ContractJobService(
        IEnumerable<IContractJobOperations> operations,
        IOutboxDispatcher outboxDispatcher,
        IMilestoneSchedulingReconciliationService schedulingReconciliation)
    {
        _operations = operations.ToArray();
        _outboxDispatcher = outboxDispatcher;
        _schedulingReconciliation = schedulingReconciliation;
    }

    public async Task<JobExecutionResult> AutoAcceptMilestoneAsync(
        Guid milestoneId,
        Guid escrowHoldId,
        int submissionVersion,
        CancellationToken cancellationToken)
    {
        return await GetOperations().AutoAcceptMilestoneAsync(
            milestoneId,
            escrowHoldId,
            submissionVersion,
            cancellationToken);
    }

    public async Task<JobExecutionResult> ReleaseExpiredHoldAsync(
        Guid escrowHoldId,
        CancellationToken cancellationToken)
    {
        return await GetOperations().ReleaseExpiredHoldAsync(
            escrowHoldId,
            cancellationToken);
    }

    public async Task<JobExecutionResult> ReconcileProviderTransactionAsync(
        Guid paymentTransactionId,
        CancellationToken cancellationToken)
    {
        return await GetOperations().ReconcileProviderTransactionAsync(
            paymentTransactionId,
            cancellationToken);
    }

    public async Task<JobExecutionResult> RetryProviderTransactionAsync(
        Guid paymentTransactionId,
        CancellationToken cancellationToken)
    {
        return await GetOperations().RetryProviderTransactionAsync(
            paymentTransactionId,
            cancellationToken);
    }

    public async Task<JobExecutionResult> ReconcileMissingSchedulesAsync(
        CancellationToken cancellationToken)
    {
        return await _schedulingReconciliation.ReconcileAsync(
            cancellationToken);
    }

    public async Task<JobExecutionResult>
        ReconcilePendingProviderTransactionsAsync(
            CancellationToken cancellationToken)
    {
        return await GetOperations()
            .ReconcilePendingProviderTransactionsAsync(cancellationToken);
    }

    public async Task<JobExecutionResult> ReconcilePendingWalletProjectionsAsync(
        CancellationToken cancellationToken)
    {
        return await GetOperations().ReconcilePendingWalletProjectionsAsync(
            cancellationToken);
    }

    public async Task<JobExecutionResult> DispatchOutboxAsync(
        int batchSize,
        CancellationToken cancellationToken)
    {
        var dispatched = await _outboxDispatcher.DispatchAvailableAsync(
            batchSize,
            cancellationToken);
        return dispatched == 0
            ? JobExecutionResult.NoOp("NoOutboxMessagesAvailable")
            : JobExecutionResult.Completed(
                "OutboxMessagesDispatched",
                dispatched);
    }

    private IContractJobOperations GetOperations()
    {
        if (_operations.Count == 1)
        {
            return _operations.Single();
        }

        throw new InvalidOperationException(
            "Exactly one contract job operations service must be registered.");
    }
}
