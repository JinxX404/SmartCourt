namespace SmartCourt.Features.Admin.Verifications.ReviewVerificationDocument.DTOs;

public sealed class ReviewVerificationDocumentRequest
{
    public VerificationReviewDecision Decision { get; init; }
    public string? RejectionReason { get; init; }
}
