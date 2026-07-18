using System.Linq.Expressions;
using SmartCourt.Common.Entities;
using SmartCourt.Common.Enums;

namespace SmartCourt.Features.Admin.Verifications.Shared;

internal static class VerificationQueueFilter
{
    public static Expression<Func<ApplicationUser, bool>> HasCurrentDocumentWithStatus(
        VerificationDocumentStatus? status)
    {
        return status is null
            ? user => user.VerificationDocuments.Any(document => document.IsCurrent)
            : user => user.VerificationDocuments.Any(document =>
                document.IsCurrent && document.Status == status.Value);
    }
}
