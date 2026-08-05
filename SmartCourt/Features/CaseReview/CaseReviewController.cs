using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartCourt.Common.Models;
using SmartCourt.Features.CaseReview.DTOs;

namespace SmartCourt.Features.CaseReview;

[ApiController]
[Route("api/cases")]
[Authorize]
public class CaseReviewController(ICaseReviewService caseReviewService) : ControllerBase
{
    private readonly ICaseReviewService _caseReviewService = caseReviewService;

    [HttpPost("{id}/review")]
    public async Task<ActionResult<ApiResponse<CaseReviewReportDto>>> CreateReview(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        var result = await _caseReviewService.CreateReviewReportAsync(id, cancellationToken);
        return Ok(ApiResponse<CaseReviewReportDto>.Ok(result));
    }

    [HttpGet("{id}/reviews")]
    public async Task<ActionResult<ApiResponse<List<CaseReviewReportDto>>>> GetReviews(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        var result = await _caseReviewService.GetReviewReportsAsync(id, cancellationToken);
        return Ok(ApiResponse<List<CaseReviewReportDto>>.Ok(result));
    }

    [HttpGet("{id}/reviews/latest")]
    public async Task<ActionResult<ApiResponse<CaseReviewReportDto>>> GetLatestReview(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        var result = await _caseReviewService.GetLatestReviewReportAsync(id, cancellationToken);
        return Ok(ApiResponse<CaseReviewReportDto>.Ok(result));
    }
}
