using SmartCourt.Common.Entities;

namespace SmartCourt.Entities
{
    public class CaseDocument : AuditableEntity
    { 
        public Guid Id { get; set; } 
        public Guid CaseId { get; set; }
        public Case Case { get; set; } = null!; 
        public Guid StoredFileId { get; set; }
        public StoredFile StoredFile { get; set; } = null!;
    }
}
