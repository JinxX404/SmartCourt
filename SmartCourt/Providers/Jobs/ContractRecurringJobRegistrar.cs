using SmartCourt.Features.Payments;
using SmartCourt.Infrastructure.Providers.Jobs;
using SmartCourt.Interfaces.Providers;

namespace SmartCourt.Providers.Jobs;

public sealed class ContractRecurringJobRegistrar(
    IRecurringBackgroundJobProvider recurringJobs)
    : IContractRecurringJobRegistrar
{
    private const int OutboxBatchSize = 100;
    private const string EveryMinute = "*/1 * * * *";
    private const string EveryFiveMinutes = "*/5 * * * *";

    public async Task RegisterAsync(
        CancellationToken cancellationToken)
    {
        await recurringJobs.RegisterOrUpdateAsync<IContractJobService>(
            "contract-payment-outbox-dispatch",
            service => service.DispatchOutboxAsync(
                OutboxBatchSize,
                CancellationToken.None),
            EveryMinute,
            cancellationToken);

        await recurringJobs.RegisterOrUpdateAsync<IContractJobService>(
            "contract-payment-schedule-reconciliation",
            service => service.ReconcileMissingSchedulesAsync(
                CancellationToken.None),
            EveryFiveMinutes,
            cancellationToken);

        await recurringJobs.RegisterOrUpdateAsync<IContractJobService>(
            "contract-payment-wallet-reconciliation",
            service => service.ReconcilePendingWalletProjectionsAsync(
                CancellationToken.None),
            EveryFiveMinutes,
            cancellationToken);

        await recurringJobs.RegisterOrUpdateAsync<IContractJobService>(
            "contract-payment-provider-reconciliation",
            service => service.ReconcilePendingProviderTransactionsAsync(
                CancellationToken.None),
            EveryFiveMinutes,
            cancellationToken);
    }
}
