using SmartCourt.Common.Entities;
using SmartCourt.Common.Enums;
using SmartCourt.Entities;
using SmartCourt.Features.Auth.Enums;

namespace SmartCourt.Features.Admin.Verifications.Shared;

internal static class VerificationStatusEvaluator
{
    private static readonly VerificationDocumentType[] RequiredDocumentTypes =
    [
        VerificationDocumentType.NationalIdFront,
        VerificationDocumentType.NationalIdBack,
        VerificationDocumentType.BarAssociationCardFront,
        VerificationDocumentType.BarAssociationCardBack
    ];

    public static bool IsFullyVerified(IEnumerable<UserVerificationDocument> documents, DateOnly today)
    {
        var currentDocuments = documents.Where(d => d.IsCurrent).ToList();
        return RequiredDocumentTypes.All(requiredType => currentDocuments.Any(document =>
            document.DocumentType == requiredType &&
            document.Status == VerificationDocumentStatus.Verified &&
            document.ExpirationDate > today));
    }

    public static UserStatus ResolveAccountStatus(
        IEnumerable<UserVerificationDocument> documents,
        DateOnly today)
    {
        var currentDocuments = documents.Where(document => document.IsCurrent).ToList();

        var hasEveryRequiredDocument = RequiredDocumentTypes.All(requiredType =>
            currentDocuments.Any(document => document.DocumentType == requiredType));

        var allRequiredDocumentsAreVerified = hasEveryRequiredDocument &&
            RequiredDocumentTypes.All(requiredType => currentDocuments.Any(document =>
                document.DocumentType == requiredType &&
                document.Status == VerificationDocumentStatus.Verified &&
                document.ExpirationDate > today));

        if (allRequiredDocumentsAreVerified)
        {
            return UserStatus.Active;
        }

        if (currentDocuments.Any(document =>
                document.Status is VerificationDocumentStatus.Rejected or VerificationDocumentStatus.Expired ||
                document.ExpirationDate <= today))
        {
            return UserStatus.Rejected;
        }

        return currentDocuments.Any(document => document.Status == VerificationDocumentStatus.Pending)
            ? UserStatus.PendingReview
            : UserStatus.Unverified;
    }
}
