using SmartCourt.Common.Entities;
using SmartCourt.Common.Enums;

namespace SmartCourt.Entities
{
    public class Case : AuditableEntity
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = null!;
        public string Description { get; set; } = null!;
        public string? Governorate { get; set; }
        public string? City { get; set; }
        public Guid ClientId { get; set; }
        public ClientProfile ClientProfile { get; set; } = null!;
        public CaseStatus Status { get; set; }
        public DateTime? SubmittedAt { get; set; }
        public Guid? LawyerId { get; set; }
        public LawyerProfile? LawyerProfile { get; set; }
        public ICollection<CaseDocument> Documents { get; set; } = new List<CaseDocument>();
        public CaseProfile? CaseProfile { get; set; }
        public ICollection<CaseReviewReport> ReviewReports { get; set; } = new List<CaseReviewReport>();
        public ICollection<CaseRecommendation> Recommendations { get; set; } = new List<CaseRecommendation>();
    }
}
