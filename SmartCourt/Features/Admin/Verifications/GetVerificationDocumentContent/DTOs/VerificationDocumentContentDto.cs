namespace SmartCourt.Features.Admin.Verifications.GetVerificationDocumentContent.DTOs;

public sealed class VerificationDocumentContentDto
{
    /// <summary>URL the admin can use to download/view the document.</summary>
    public string DownloadUrl { get; init; } = string.Empty;
    public string ContentType { get; init; } = string.Empty;
    public string FileName { get; init; } = string.Empty;
}
