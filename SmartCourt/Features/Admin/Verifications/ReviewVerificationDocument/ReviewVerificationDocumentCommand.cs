using MediatR;
using SmartCourt.Common.Models;
using SmartCourt.Features.Admin.Verifications.ReviewVerificationDocument.DTOs;

namespace SmartCourt.Features.Admin.Verifications.ReviewVerificationDocument;

public enum VerificationReviewDecision : byte
{
    Approve = 1,
    Reject = 2
}

public sealed record ReviewVerificationDocumentCommand(
    Guid DocumentId,
    VerificationReviewDecision Decision,
    string? RejectionReason)
    : IRequest<ApiResponse<ReviewVerificationDocumentResponse>>;
