namespace SmartCourt.Features.Admin.Verifications.GetVerificationDocumentContent.DTOs;

public sealed class VerificationDocumentContentDto
{
    public byte[] Content { get; init; } = [];
    public string ContentType { get; init; } = string.Empty;
    public string FileName { get; init; } = string.Empty;
}
