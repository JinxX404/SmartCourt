namespace SmartCourt.Features.Proposals.DTOs;

public sealed record CreateProposalRequest(
    Guid LegalCaseId,
    Guid LawyerUserId,
    string Message);

public sealed record RejectProposalRequest(string Reason);

public sealed record CancelProposalRequest(string Reason);

public sealed record TerminateProposalRequest(string Reason);
