using SmartCourt.Common.Entities;
using SmartCourt.Common.Enums;
using SmartCourt.Entities;
using SmartCourt.Features.Admin.Verifications.Shared;
using Xunit;

namespace SmartCourt.Tests.Features.Admin.Verifications;

public sealed class VerificationQueueFilterTests
{
    [Fact]
    public void HasCurrentDocumentWithStatus_DefaultsToOnlyPendingLawyers_WhenStatusIsNotSpecified()
    {
        // Previously the null branch matched ANY lawyer with a current document
        // (including fully verified ones). The fix: null defaults to Pending.
        var lawyerWithPendingDocument  = CreateLawyer(CreateDocument(VerificationDocumentStatus.Pending, true));
        var lawyerWithRejectedDocument = CreateLawyer(CreateDocument(VerificationDocumentStatus.Rejected, true));
        var lawyerWithVerifiedDocument = CreateLawyer(CreateDocument(VerificationDocumentStatus.Verified, true));
        var lawyerWithOnlyOldDocument  = CreateLawyer(CreateDocument(VerificationDocumentStatus.Pending, false));

        var filter = VerificationQueueFilter.HasCurrentDocumentWithStatus(null).Compile();

        Assert.True(filter(lawyerWithPendingDocument),   "Pending lawyer must appear in default queue.");
        Assert.False(filter(lawyerWithRejectedDocument), "Rejected lawyer must NOT appear in default Pending queue.");
        Assert.False(filter(lawyerWithVerifiedDocument), "Verified lawyer must NOT appear in default Pending queue.");
        Assert.False(filter(lawyerWithOnlyOldDocument),  "Lawyer with only non-current documents must NOT appear.");
    }

    [Fact]
    public void HasCurrentDocumentWithStatus_MatchesOnlyTheRequestedCurrentStatus()
    {
        var lawyer = CreateLawyer(
            CreateDocument(VerificationDocumentStatus.Pending, false),
            CreateDocument(VerificationDocumentStatus.Verified, true),
            CreateDocument(VerificationDocumentStatus.Rejected, true));

        Assert.False(VerificationQueueFilter.HasCurrentDocumentWithStatus(VerificationDocumentStatus.Pending).Compile()(lawyer));
        Assert.True(VerificationQueueFilter.HasCurrentDocumentWithStatus(VerificationDocumentStatus.Verified).Compile()(lawyer));
        Assert.True(VerificationQueueFilter.HasCurrentDocumentWithStatus(VerificationDocumentStatus.Rejected).Compile()(lawyer));
        Assert.False(VerificationQueueFilter.HasCurrentDocumentWithStatus(VerificationDocumentStatus.Expired).Compile()(lawyer));
    }

    private static ApplicationUser CreateLawyer(params UserVerificationDocument[] documents)
    {
        return new ApplicationUser
        {
            VerificationDocuments = documents.ToList()
        };
    }

    private static UserVerificationDocument CreateDocument(VerificationDocumentStatus status, bool isCurrent)
    {
        return new UserVerificationDocument
        {
            Status = status,
            IsCurrent = isCurrent
        };
    }
}
