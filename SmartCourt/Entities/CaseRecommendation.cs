using SmartCourt.Common.Entities;

namespace SmartCourt.Entities;

public class CaseRecommendation : AuditableEntity
{
    public Guid Id { get; set; }
    public Guid CaseId { get; set; }
    public Case Case { get; set; } = null!;
    public Guid LawyerId { get; set; }
    public LawyerProfile LawyerProfile { get; set; } = null!;
    public decimal TotalScore { get; set; }
    public decimal LocationScore { get; set; }
    public decimal ExperienceScore { get; set; }
    public decimal RatingScore { get; set; }
    public decimal ResponseTimeScore { get; set; }
    public string Explanation { get; set; } = null!;
    public int Rank { get; set; }
}
