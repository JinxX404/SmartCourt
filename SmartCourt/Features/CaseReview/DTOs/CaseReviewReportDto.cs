using System;
using System.Collections.Generic;

namespace SmartCourt.Features.CaseReview.DTOs;

public class CaseReviewReportDto
{
    public Guid Id { get; set; }
    public Guid CaseId { get; set; }
    public bool IsLatest { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public List<ReviewPointDto> ReviewPoints { get; set; } = [];
}
