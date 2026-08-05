using System.Linq.Expressions;
using SmartCourt.Features.Payments;
using SmartCourt.Infrastructure.Providers.Jobs;
using SmartCourt.Interfaces.Providers;
using SmartCourt.Providers.Jobs;
using Xunit;

namespace SmartCourt.Tests.Infrastructure.Providers;

public sealed class ContractRecurringJobRegistrarTests
{
    [Fact]
    public async Task RegisterAsync_RegistersOutboxAndReconciliationJobs()
    {
        var provider = new RecordingRecurringJobProvider();
        var registrar = new ContractRecurringJobRegistrar(provider);

        await registrar.RegisterAsync(CancellationToken.None);

        Assert.Equal(
            [
                "contract-payment-outbox-dispatch",
                "contract-payment-schedule-reconciliation",
                "contract-payment-wallet-reconciliation",
                "contract-payment-provider-reconciliation",
                "contract-termination-recovery",
                "contract-dispute-settlement-recovery"
            ],
            provider.JobIds);
        Assert.Equal(
            [
                "*/1 * * * *",
                "*/5 * * * *",
                "*/5 * * * *",
                "*/5 * * * *",
                "*/5 * * * *",
                "*/5 * * * *"
            ],
            provider.CronExpressions);
        Assert.Equal(
            [
                nameof(IContractJobService.DispatchOutboxAsync),
                nameof(IContractJobService.ReconcileMissingSchedulesAsync),
                nameof(IContractJobService.ReconcilePendingWalletProjectionsAsync),
                nameof(IContractJobService.ReconcilePendingProviderTransactionsAsync),
                nameof(IContractJobService.RecoverPendingContractTerminationsAsync),
                nameof(IContractJobService.RecoverPendingDisputeSettlementsAsync)
            ],
            provider.MethodNames);
    }

    [Fact]
    public async Task RegisterAsync_PropagatesCancellationBeforeRegistration()
    {
        var provider = new RecordingRecurringJobProvider();
        var registrar = new ContractRecurringJobRegistrar(provider);
        using var source = new CancellationTokenSource();
        source.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => registrar.RegisterAsync(source.Token));
        Assert.Empty(provider.JobIds);
    }

    private sealed class RecordingRecurringJobProvider
        : IRecurringBackgroundJobProvider
    {
        public List<string> JobIds { get; } = [];
        public List<string> CronExpressions { get; } = [];
        public List<string> MethodNames { get; } = [];

        public Task RegisterOrUpdateAsync<T>(
            string recurringJobId,
            Expression<Func<T, Task>> methodCall,
            string cronExpression,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            JobIds.Add(recurringJobId);
            CronExpressions.Add(cronExpression);
            MethodNames.Add(GetMethodCall(methodCall).Method.Name);
            return Task.CompletedTask;
        }

        private static MethodCallExpression GetMethodCall<T>(
            Expression<Func<T, Task>> methodCall)
        {
            return methodCall.Body switch
            {
                MethodCallExpression call => call,
                UnaryExpression { Operand: MethodCallExpression call } => call,
                _ => throw new InvalidOperationException(
                    "Expected a recurring service method call.")
            };
        }
    }
}
