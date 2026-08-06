using SmartCourt.Common.Entities;
using SmartCourt.Common.Enums;
using SmartCourt.Entities;
using SmartCourt.Features.Auth.Enums;

namespace SmartCourt.Features.Admin.Verifications.Shared;

internal static class VerificationStatusEvaluator
{
    private static VerificationDocumentType[] GetRequiredDocumentTypes(bool isLawyer)
    {
        if (isLawyer)
        {
            return
            [
                VerificationDocumentType.NationalIdFront,
                VerificationDocumentType.NationalIdBack,
                VerificationDocumentType.BarAssociationCardFront,
                VerificationDocumentType.BarAssociationCardBack,
                VerificationDocumentType.SelfieWithId,
                VerificationDocumentType.OfficialProfilePicture
            ];
        }

        return
        [
            VerificationDocumentType.NationalIdFront,
            VerificationDocumentType.NationalIdBack,
            VerificationDocumentType.SelfieWithId
        ];
    }

    public static bool IsFullyVerified(IEnumerable<UserVerificationDocument> documents, DateOnly today, bool isLawyer)
    {
        var currentDocuments = documents.Where(d => d.IsCurrent).ToList();
        var requiredTypes = GetRequiredDocumentTypes(isLawyer);
        
        return requiredTypes.All(requiredType => currentDocuments.Any(document =>
            document.DocumentType == requiredType &&
            document.Status == VerificationDocumentStatus.Verified &&
            document.ExpirationDate > today));
    }

    public static UserStatus ResolveAccountStatus(
        IEnumerable<UserVerificationDocument> documents,
        DateOnly today,
        bool isLawyer)
    {
        var currentDocuments = documents.Where(document => document.IsCurrent).ToList();
        var requiredTypes = GetRequiredDocumentTypes(isLawyer);

        var hasEveryRequiredDocument = requiredTypes.All(requiredType =>
            currentDocuments.Any(document => document.DocumentType == requiredType));

        var allRequiredDocumentsAreVerified = hasEveryRequiredDocument &&
            requiredTypes.All(requiredType => currentDocuments.Any(document =>
                document.DocumentType == requiredType &&
                document.Status == VerificationDocumentStatus.Verified &&
                document.ExpirationDate > today));

        if (allRequiredDocumentsAreVerified)
        {
            return UserStatus.Active;
        }

        if (currentDocuments.Any(document => document.Status == VerificationDocumentStatus.Pending))
        {
            return UserStatus.PendingReview;
        }

        return UserStatus.Unverified;
    }
}
