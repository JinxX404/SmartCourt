using SmartCourt.Common.Entities;

namespace SmartCourt.Entities
{
    public class CaseReviewReport : AuditableEntity
    {
        public Guid Id { get; set; }
        public bool IsLatest { get; set; }
        public Guid CaseId { get; set; }
        public Case Case { get; set; } = null!;
        public ICollection<ReviewPoint> ReviewPoints { get; set; } = new List<ReviewPoint>();
    }
}
