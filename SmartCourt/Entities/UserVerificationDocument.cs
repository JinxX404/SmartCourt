using SmartCourt.Common.Entities;
using SmartCourt.Common;
using SmartCourt.Common.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartCourt.Entities
{
    public class UserVerificationDocument : BaseEntity
    {
        public Guid Id { get; set; }

        [ForeignKey("User")]
        public Guid UserId { get; set; }
        public ApplicationUser User { get; set; } = null!;

        [ForeignKey("StoredFile")]
        public Guid StoredFileId { get; set; }
        public StoredFile StoredFile { get; set; } = null!;

        public VerificationDocumentType DocumentType { get; set; }
        public VerificationDocumentStatus Status { get; set; }
        public DateOnly ExpirationDate { get; set; }
        public DateTimeOffset? VerifiedAt { get; set; }
        public string? VerifiedByAdminId { get; set; }
        public string? RejectionReason { get; set; }
        public bool IsCurrent { get; set; }

        /// <summary>
        /// SQL Server rowversion column used as an optimistic concurrency token.
        /// When two admins load the same document and both call SaveChangesAsync,
        /// the second call throws <see cref="Microsoft.EntityFrameworkCore.DbUpdateConcurrencyException"/>
        /// because the row version no longer matches.
        /// </summary>
        [Timestamp]
        public byte[] RowVersion { get; set; } = [];
    }
}
