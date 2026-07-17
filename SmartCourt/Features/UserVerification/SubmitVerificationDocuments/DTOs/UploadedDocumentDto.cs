using SmartCourt.Common.Enums;

namespace SmartCourt.Features.UserVerification.SubmitVerificationDocuments.DTOs
{
    public class UploadedDocumentDto
    {
        public string FileName { get; set; }
        public VerificationDocumentType Type { get; set; }
    }
}
