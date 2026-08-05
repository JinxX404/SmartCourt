namespace SmartCourt.Features.UserVerification.SubmitVerificationDocuments.DTOs
{
    public class SubmitVerificationDocumentResponseDto
    {
        public List<UploadedDocumentDto> UploadedDocuments { get; set; } = [];
        public List<DocumentUploadErrorDto> FailedDocuments { get; set; } = [];
    }
}
