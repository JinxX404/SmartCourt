using SmartCourt.Common.Enums;

namespace SmartCourt.Entities
{
    public class ReviewPoint
    {
        public Guid Id { get; set; }
        public Guid CaseReviewReportId { get; set; }
        public CaseReviewReport CaseReviewReport { get; set; } = null!;
        public string Description { get; set; } = null!;
        public ReviewPointType Type { get; set; }
    }
}
