using SmartCourt.Common.Enums;
using SmartCourt.Features.Auth.Enums;

namespace SmartCourt.Features.Admin.Verifications.Integration;

public interface IVerificationNotificationContextReader
{
    Task<VerificationDocumentNotificationContext> GetDocumentAsync(
        Guid documentId,
        CancellationToken cancellationToken);

    Task<VerificationAccountNotificationContext> GetAccountAsync(
        Guid userId,
        CancellationToken cancellationToken);
}

public sealed record VerificationDocumentNotificationContext(
    Guid DocumentId,
    Guid UserId,
    VerificationDocumentType DocumentType,
    VerificationDocumentStatus Status);

public sealed record VerificationAccountNotificationContext(
    Guid UserId,
    UserStatus Status);
