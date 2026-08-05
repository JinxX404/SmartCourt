using SmartCourt.Common.Enums;
using SmartCourt.Entities;
using SmartCourt.Features.Admin.Verifications.Shared;
using SmartCourt.Features.Auth.Enums;
using Xunit;

namespace SmartCourt.Tests.Features.Admin.Verifications;

public sealed class VerificationStatusEvaluatorTests
{
    private static readonly DateOnly Today = new(2026, 7, 18);

    // ──────────────────────────────────────────
    // ResolveAccountStatus
    // ──────────────────────────────────────────

    [Fact]
    public void ResolveAccountStatus_ReturnsActive_WhenEveryCurrentRequirementIsVerifiedAndUnexpired()
    {
        var documents = RequiredDocuments(VerificationDocumentStatus.Verified);

        var status = VerificationStatusEvaluator.ResolveAccountStatus(documents, Today);

        Assert.Equal(UserStatus.Active, status);
    }

    [Fact]
    public void ResolveAccountStatus_ReturnsRejected_WhenAnyCurrentRequiredDocumentIsRejected()
    {
        var documents = RequiredDocuments(VerificationDocumentStatus.Verified);
        documents[2].Status = VerificationDocumentStatus.Rejected;

        var status = VerificationStatusEvaluator.ResolveAccountStatus(documents, Today);

        Assert.Equal(UserStatus.Rejected, status);
    }

    [Fact]
    public void ResolveAccountStatus_ReturnsPendingReview_WhenARequiredDocumentIsAwaitingReview()
    {
        var documents = RequiredDocuments(VerificationDocumentStatus.Verified);
        documents[1].Status = VerificationDocumentStatus.Pending;

        var status = VerificationStatusEvaluator.ResolveAccountStatus(documents, Today);

        Assert.Equal(UserStatus.PendingReview, status);
    }

    [Fact]
    public void ResolveAccountStatus_UsesTheCurrentReplacementInsteadOfAnOldRejectedVersion()
    {
        var documents = RequiredDocuments(VerificationDocumentStatus.Verified);
        documents[0].Status = VerificationDocumentStatus.Rejected;
        documents[0].IsCurrent = false;
        documents.Add(CreateDocument(VerificationDocumentType.NationalIdFront, VerificationDocumentStatus.Verified));

        var status = VerificationStatusEvaluator.ResolveAccountStatus(documents, Today);

        Assert.Equal(UserStatus.Active, status);
    }

    // ──────────────────────────────────────────
    // IsFullyVerified
    // ──────────────────────────────────────────

    [Fact]
    public void IsFullyVerified_ReturnsTrue_WhenAllRequiredDocumentsAreVerifiedAndUnexpired()
    {
        var documents = RequiredDocuments(VerificationDocumentStatus.Verified);

        var result = VerificationStatusEvaluator.IsFullyVerified(documents, Today);

        Assert.True(result);
    }

    [Fact]
    public void IsFullyVerified_ReturnsFalse_WhenNoDocumentsExist()
    {
        // This covers the original bug: an Active seeded lawyer with zero documents
        // was previously reported as IsFullyVerified = true because the code
        // derived it from UserStatus.Active instead of evaluating documents.
        var result = VerificationStatusEvaluator.IsFullyVerified([], Today);

        Assert.False(result);
    }

    [Fact]
    public void IsFullyVerified_ReturnsFalse_WhenARequiredDocumentIsExpired()
    {
        var documents = RequiredDocuments(VerificationDocumentStatus.Verified);
        // Make the first document expire before today
        documents[0].ExpirationDate = Today.AddDays(-1);

        var result = VerificationStatusEvaluator.IsFullyVerified(documents, Today);

        Assert.False(result);
    }

    [Fact]
    public void IsFullyVerified_ReturnsFalse_WhenARequiredDocumentIsPending()
    {
        var documents = RequiredDocuments(VerificationDocumentStatus.Verified);
        documents[3].Status = VerificationDocumentStatus.Pending;

        var result = VerificationStatusEvaluator.IsFullyVerified(documents, Today);

        Assert.False(result);
    }

    [Fact]
    public void IsFullyVerified_ReturnsFalse_WhenARequiredDocumentTypeIsMissing()
    {
        // Only 3 of the 4 required document types present
        var documents = new List<UserVerificationDocument>
        {
            CreateDocument(VerificationDocumentType.NationalIdFront, VerificationDocumentStatus.Verified),
            CreateDocument(VerificationDocumentType.NationalIdBack, VerificationDocumentStatus.Verified),
            CreateDocument(VerificationDocumentType.BarAssociationCardFront, VerificationDocumentStatus.Verified)
            // BarAssociationCardBack is missing
        };

        var result = VerificationStatusEvaluator.IsFullyVerified(documents, Today);

        Assert.False(result);
    }

    // ──────────────────────────────────────────
    // Helpers
    // ──────────────────────────────────────────

    private static List<UserVerificationDocument> RequiredDocuments(VerificationDocumentStatus status)
    {
        return
        [
            CreateDocument(VerificationDocumentType.NationalIdFront, status),
            CreateDocument(VerificationDocumentType.NationalIdBack, status),
            CreateDocument(VerificationDocumentType.BarAssociationCardFront, status),
            CreateDocument(VerificationDocumentType.BarAssociationCardBack, status)
        ];
    }

    private static UserVerificationDocument CreateDocument(
        VerificationDocumentType documentType,
        VerificationDocumentStatus status)
    {
        return new UserVerificationDocument
        {
            DocumentType = documentType,
            Status = status,
            ExpirationDate = Today.AddDays(30),
            IsCurrent = true
        };
    }
}
