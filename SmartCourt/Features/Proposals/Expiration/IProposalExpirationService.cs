namespace SmartCourt.Features.Proposals.Expiration;

public interface IProposalExpirationService
{
    Task<int> ExpireDueAsync(CancellationToken cancellationToken);

    Task<int> ExpireDueForCaseAsync(
        Guid legalCaseId,
        CancellationToken cancellationToken);
}
