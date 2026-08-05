using System;

namespace SmartCourt.Features.Matching.DTOs;

public class CaseRecommendationDto
{
    public Guid LawyerId { get; set; }
    public string LawyerName { get; set; } = string.Empty;
    public double TotalScore { get; set; }
    public double LocationScore { get; set; }
    public double ExperienceScore { get; set; }
    public double RatingScore { get; set; }
    public double ResponseTimeScore { get; set; }
    public string Explanation { get; set; } = string.Empty;
    public int Rank { get; set; }
}
