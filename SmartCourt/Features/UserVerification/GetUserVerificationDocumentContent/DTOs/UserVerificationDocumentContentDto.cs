namespace SmartCourt.Features.UserVerification.GetUserVerificationDocumentContent.DTOs;

public sealed class UserVerificationDocumentContentDto
{
    public string DownloadUrl { get; init; } = string.Empty;
    public string ContentType { get; init; } = string.Empty;
    public string FileName { get; init; } = string.Empty;
}
