namespace SmartCourt.Infrastructure.Providers.Events;

public sealed record VerificationDocumentEventPayload(
    Guid DocumentId,
    Guid UserId,
    string DocumentType,
    string Status);

public sealed record VerificationAccountEventPayload(
    Guid UserId,
    string Status);
