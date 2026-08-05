namespace SmartCourt.Features.UserVerification.GetUserVerificationDocuments.DTOs
{
    public sealed class GetUserVerificationDocumentsResponseDto
    {
        public List<UserVerificationDocumentDto> Documents { get; init; } = [];
    }
}
