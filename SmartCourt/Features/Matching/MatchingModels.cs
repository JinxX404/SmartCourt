using System;
using SmartCourt.Common.Enums;

namespace SmartCourt.Features.Matching;

public class LawyerCandidate
{
    public Guid LawyerId { get; set; }
    public string LawyerName { get; set; } = string.Empty;
    public string? Governorate { get; set; }
    public LawyerLevel Level { get; set; }
    public bool IsAvailable { get; set; }
    public decimal AverageRating { get; set; }
    public decimal AverageResponseTimeHours { get; set; }

    public int SpecializationYearsOfExperience { get; set; }
    public int SpecializationCasesHandled { get; set; }
}

public class ScoredLawyerCandidate
{
    public LawyerCandidate Candidate { get; set; } = null!;
    public double TotalScore { get; set; }
    public double LocationScore { get; set; }
    public double ExperienceScore { get; set; }
    public double RatingScore { get; set; }
    public double ResponseTimeScore { get; set; }
    public int Rank { get; set; }
}
