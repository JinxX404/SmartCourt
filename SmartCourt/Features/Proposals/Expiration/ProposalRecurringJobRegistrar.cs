using SmartCourt.Interfaces.Providers;

namespace SmartCourt.Features.Proposals.Expiration;

public sealed class ProposalRecurringJobRegistrar(
    IRecurringBackgroundJobProvider recurringJobs)
    : IProposalRecurringJobRegistrar
{
    private const string EveryMinute = "*/1 * * * *";

    public Task RegisterAsync(CancellationToken cancellationToken)
    {
        return recurringJobs.RegisterOrUpdateAsync<IProposalExpirationService>(
            "proposal-expiration",
            service => service.ExpireDueAsync(CancellationToken.None),
            EveryMinute,
            cancellationToken);
    }
}
