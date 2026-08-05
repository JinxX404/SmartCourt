namespace SmartCourt.Infrastructure.Providers.Jobs;

public interface IContractRecurringJobRegistrar
{
    Task RegisterAsync(CancellationToken cancellationToken);
}
