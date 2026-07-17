using SmartCourt.Common.Enums;

namespace SmartCourt.Features.UserVerification.SubmitVerificationDocuments.DTOs
{
    public class VerificationDocumentDto
    {
        public IFormFile File { get; set; }
        public DateOnly ExpirationDate { get; set; }
        public VerificationDocumentType Type { get; set; }
    }
}
