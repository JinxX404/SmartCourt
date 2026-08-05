namespace SmartCourt.Features.Contracts.Integration;

/// <summary>
/// Outbound contract lifecycle port. The Cases slice supplies the implementation
/// so Contracts never mutates case state directly.
/// </summary>
public interface IContractCaseLifecycleService
{
    Task ApplyAsync(
        ContractCaseLifecycleUpdate update,
        CancellationToken cancellationToken);
}

public sealed record ContractCaseLifecycleUpdate(
    Guid EventId,
    Guid LegalCaseId,
    Guid ContractId,
    ContractCaseLifecycleTransition Transition,
    DateTimeOffset OccurredAt);

public enum ContractCaseLifecycleTransition
{
    ContractCompleted = 1,
    ContractTerminated = 2
}
