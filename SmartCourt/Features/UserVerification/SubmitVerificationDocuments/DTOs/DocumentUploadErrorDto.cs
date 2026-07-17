using SmartCourt.Common.Enums;

namespace SmartCourt.Features.UserVerification.SubmitVerificationDocuments.DTOs
{
    public class DocumentUploadErrorDto
    {
        public string FileName { get; set; }
        public VerificationDocumentType Type { get; set; }
        public string Error { get; set; }
    }
}
