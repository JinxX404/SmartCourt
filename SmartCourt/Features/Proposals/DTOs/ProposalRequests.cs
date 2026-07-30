namespace SmartCourt.Features.Proposals.DTOs;

public sealed record CreateProposalRequest(
    Guid LegalCaseId,
    Guid LawyerUserId,
    string Message);

public sealed record RejectProposalRequest(string Reason);
