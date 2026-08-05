using System.Linq.Expressions;
using SmartCourt.Common.Entities;
using SmartCourt.Common.Enums;

namespace SmartCourt.Features.Admin.Verifications.Shared;

internal static class VerificationQueueFilter
{
    /// <summary>
    /// Returns an expression that filters lawyers by the given document status.
    /// When no status is specified the default queue only surfaces lawyers who
    /// have at least one current <see cref="VerificationDocumentStatus.Pending"/>
    /// document — i.e. lawyers that genuinely need an admin decision.
    /// Previously the null branch returned every lawyer who had any current
    /// document, including fully-verified lawyers, which is incorrect.
    /// </summary>
    public static Expression<Func<ApplicationUser, bool>> HasCurrentDocumentWithStatus(
        VerificationDocumentStatus? status)
    {
        // Default to Pending so the admin queue only shows actionable cases.
        var effectiveStatus = status ?? VerificationDocumentStatus.Pending;

        return user => user.VerificationDocuments.Any(document =>
            document.IsCurrent && document.Status == effectiveStatus);
    }
}
