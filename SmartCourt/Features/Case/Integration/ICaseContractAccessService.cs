namespace SmartCourt.Features.Case.Integration;

/// <summary>
/// Provides case-owned facts required to create a contract.
/// A result is returned only when the case is eligible for contract creation.
/// </summary>
public interface ICaseContractAccessService
{
    Task<CaseContractEligibilityFacts?> FindEligibleForContractAsync(
        Guid legalCaseId,
        CancellationToken cancellationToken);
}

public sealed record CaseContractEligibilityFacts(
    Guid LegalCaseId,
    Guid ClientUserId);
