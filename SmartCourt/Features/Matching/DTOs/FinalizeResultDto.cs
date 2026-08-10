using System;
using System.Collections.Generic;

namespace SmartCourt.Features.Matching.DTOs;

public class FinalizeResultDto
{
    public Guid CaseId { get; set; }
    public int TotalEligibleLawyers { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public int TotalPages { get; set; }
    public bool HasNextPage => PageNumber < TotalPages;
    public bool HasPreviousPage => PageNumber > 1;
    public List<CaseRecommendationDto> Recommendations { get; set; } = [];
}
