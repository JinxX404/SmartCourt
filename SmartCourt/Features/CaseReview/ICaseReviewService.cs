using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SmartCourt.Features.CaseReview.DTOs;

namespace SmartCourt.Features.CaseReview;

public interface ICaseReviewService
{
    Task<CaseReviewReportDto> CreateReviewReportAsync(Guid caseId, CancellationToken cancellationToken = default);
    Task<CaseReviewReportDto> GetReviewReportAsync(Guid caseId, Guid reviewId, CancellationToken cancellationToken = default);
}
