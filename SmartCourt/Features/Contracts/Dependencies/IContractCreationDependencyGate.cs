namespace SmartCourt.Features.Contracts.Dependencies;

public interface IContractCreationDependencyGate
{
    Task<ContractCreationFacts> VerifyAsync(
        Guid proposalId,
        Guid actorUserId,
        CancellationToken cancellationToken);
}

public sealed record ContractCreationFacts(
    Guid ProposalId,
    Guid LegalCaseId,
    Guid ClientUserId,
    Guid LawyerUserId);
