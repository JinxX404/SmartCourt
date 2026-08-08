using SmartCourt.Common.Enums;

namespace SmartCourt.Features.UserVerification.GetUserVerificationDocuments.DTOs
{
    public sealed class UserVerificationDocumentDto
    {
        public Guid DocumentId { get; init; }
        public VerificationDocumentType DocumentType { get; init; }
        public VerificationDocumentStatus Status { get; init; }
        public DateOnly ExpirationDate { get; init; }
        public bool IsCurrent { get; init; }
        public string FileName { get; init; } = string.Empty;
        public string? RejectionReason { get; init; }
    }
}
