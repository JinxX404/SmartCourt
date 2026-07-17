using SmartCourt.Common;
using SmartCourt.Common.Enums;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartCourt.Entities
{
    public class UserVerificationDocument : BaseEntity
    {
        public Guid Id { get; set; }

        [ForeignKey("User")]
        public string UserId { get; set; }
        public ApplicationUser User { get; set; }

        [ForeignKey("StoredFile")]
        public Guid StoredFileId { get; set; }
        public StoredFile StoredFile { get; set; }

        public VerificationDocumentType DocumentType { get; set; }
        public VerificationDocumentStatus Status { get; set; }
        public DateOnly ExpirationDate { get; set; }
        public DateTime? VerifiedAt { get; set; }
        public string? VerifiedByAdminId { get; set; }
        public string? RejectionReason { get; set; }
        public bool IsCurrent { get; set; }
    }
}
