using System.Linq.Expressions;
using SmartCourt.Common.Entities;
using SmartCourt.Common.Enums;
using SmartCourt.Features.Auth.Enums;

namespace SmartCourt.Features.Admin.Verifications.Shared;

internal static class VerificationQueueFilter
{
    /// <summary>
    /// Returns an expression that filters users for the verification queue.
    /// When no status is specified, the default queue surfaces any user whose account
    /// is <see cref="UserStatus.PendingReview"/> OR who has at least one current 
    /// <see cref="VerificationDocumentStatus.Pending"/> document.
    /// </summary>
    public static Expression<Func<ApplicationUser, bool>> HasCurrentDocumentWithStatus(
        VerificationDocumentStatus? status)
    {
        if (status.HasValue)
        {
            var effectiveStatus = status.Value;
            return user => user.VerificationDocuments.Any(document =>
                document.IsCurrent && document.Status == effectiveStatus);
        }

        // Default Pending Queue: include any user in PendingReview status OR with pending documents
        return user => user.Status == UserStatus.PendingReview ||
                       user.VerificationDocuments.Any(document =>
                           document.IsCurrent && document.Status == VerificationDocumentStatus.Pending);
    }
}
