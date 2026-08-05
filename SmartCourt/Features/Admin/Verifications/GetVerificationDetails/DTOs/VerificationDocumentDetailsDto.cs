namespace SmartCourt.Features.Admin.Verifications.GetVerificationDetails.DTOs;

public sealed class VerificationDocumentDetailsDto
{
    public Guid DocumentId { get; init; }
    public string DocumentType { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public string FileName { get; init; } = string.Empty;
    public string ContentType { get; init; } = string.Empty;
    public DateOnly ExpirationDate { get; init; }
    public DateTime? ReviewedAt { get; init; }
    public string? RejectionReason { get; init; }
    public string ContentUrl { get; init; } = string.Empty;
}
