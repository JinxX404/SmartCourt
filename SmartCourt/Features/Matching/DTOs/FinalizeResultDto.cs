using System;
using System.Collections.Generic;

namespace SmartCourt.Features.Matching.DTOs;

public class FinalizeResultDto
{
    public Guid CaseId { get; set; }
    public int TotalEligibleLawyers { get; set; }
    public List<CaseRecommendationDto> Recommendations { get; set; } = [];
}
