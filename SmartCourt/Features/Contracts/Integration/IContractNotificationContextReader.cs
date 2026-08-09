namespace SmartCourt.Features.Contracts.Integration;

public interface IContractNotificationContextReader
{
    Task<ContractNotificationContext> GetAsync(
        Guid contractId,
        CancellationToken cancellationToken);
}

public sealed record ContractNotificationContext(
    Guid ContractId,
    Guid ProposalId,
    Guid LegalCaseId,
    Guid ClientUserId,
    Guid LawyerUserId,
    bool IsTerminated);
