namespace SmartCourt.Features.Contracts.Integration;

public interface IContractCaseAssignmentService
{
    Task AssignAsync(
        ContractCaseAssignment assignment,
        CancellationToken cancellationToken);
}

public sealed record ContractCaseAssignment(
    Guid ContractId,
    Guid ProposalId,
    Guid LegalCaseId,
    Guid ClientUserId,
    Guid LawyerUserId,
    DateTimeOffset OccurredAt);
