using MediatR;
using SmartCourt.Common.Enums;
using SmartCourt.Common.Models;
using SmartCourt.Features.Admin.Verifications.GetPendingVerifications.DTOs;

namespace SmartCourt.Features.Admin.Verifications.GetPendingVerifications;

public sealed class GetPendingVerificationsQuery : PagedRequest,
    IRequest<PagedResponse<IReadOnlyList<PendingVerificationListItemDto>>>
{
    public string? Search { get; init; }
    public VerificationDocumentStatus? Status { get; init; }
}
