namespace SmartCourt.Features.Users.Integration;

public interface IContractUserEligibilityService
{
    Task<ContractUserEligibilityFacts?> FindEligibilityAsync(
        Guid userId,
        CancellationToken cancellationToken);
}

public sealed record ContractUserEligibilityFacts(
    Guid UserId,
    bool IsActive,
    bool CanActAsClient,
    bool CanActAsLawyer,
    bool CanActAsModerator,
    bool CanActAsFinanceAdministrator,
    bool CanActAsSuperAdministrator);
