namespace SmartCourt.Features.Admin.Verifications.GetPendingVerifications.DTOs;

public sealed class PendingVerificationListItemDto
{
    public Guid LawyerId { get; init; }
    public string FullName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string? PhoneNumber { get; init; }
    public int PendingDocumentCount { get; init; }
    public int VerifiedDocumentCount { get; init; }
    public int RejectedDocumentCount { get; init; }
}
