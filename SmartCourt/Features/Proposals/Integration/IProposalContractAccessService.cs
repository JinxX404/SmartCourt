namespace SmartCourt.Features.Proposals.Integration;

/// <summary>
/// Provides proposal-owned facts required to create a contract.
/// A result is returned only when the proposal exists and is accepted.
/// </summary>
public interface IProposalContractAccessService
{
    Task<AcceptedProposalContractFacts?> FindAcceptedForContractAsync(
        Guid proposalId,
        CancellationToken cancellationToken);
}

public sealed record AcceptedProposalContractFacts(
    Guid ProposalId,
    Guid LegalCaseId,
    Guid ClientUserId,
    Guid LawyerUserId);
