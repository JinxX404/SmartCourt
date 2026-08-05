namespace SmartCourt.Features.Contracts.DTOs;

public sealed record CreateContractRequest(
    Guid ProposalId,
    string Title,
    string TermsAndConditions);

public sealed record UpdateContractRequest(
    string Title,
    string TermsAndConditions);

public sealed record TerminateContractRequest(string Reason);

