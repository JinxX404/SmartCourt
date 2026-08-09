namespace SmartCourt.Features.Proposals.Expiration;

public interface IProposalRecurringJobRegistrar
{
    Task RegisterAsync(CancellationToken cancellationToken);
}
