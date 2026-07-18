using SmartCourt.Common.Enums;
using SmartCourt.Entities;
using SmartCourt.Features.Admin.Verifications.Shared;
using SmartCourt.Features.Auth.Enums;
using Xunit;

namespace SmartCourt.Tests.Features.Admin.Verifications;

public sealed class VerificationStatusEvaluatorTests
{
    private static readonly DateOnly Today = new(2026, 7, 18);

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
