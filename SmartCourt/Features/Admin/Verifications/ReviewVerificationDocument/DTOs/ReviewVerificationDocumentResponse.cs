namespace SmartCourt.Features.Admin.Verifications.ReviewVerificationDocument.DTOs;

public sealed class ReviewVerificationDocumentResponse
{
    public Guid DocumentId { get; init; }
    public string DocumentStatus { get; init; } = string.Empty;
    public string LawyerAccountStatus { get; init; } = string.Empty;
    public bool IsFullyVerified { get; init; }
}
