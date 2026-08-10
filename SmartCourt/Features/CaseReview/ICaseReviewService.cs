using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SmartCourt.Features.CaseReview.DTOs;

namespace SmartCourt.Features.CaseReview;

public interface ICaseReviewService
{
    Task<CaseReviewReportDto> CreateReviewReportAsync(Guid caseId, CancellationToken cancellationToken = default);
    Task<CaseReviewReportDto> GetLatestReviewReportAsync(Guid caseId, CancellationToken cancellationToken = default);
}
