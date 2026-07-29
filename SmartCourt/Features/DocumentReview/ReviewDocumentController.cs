using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SmartCourt.Common.Models;
using SmartCourt.Features.DocumentReview.DTOs;

using Microsoft.AspNetCore.Authorization;

namespace SmartCourt.Features.DocumentReview;

[Route("api/document-review")]
[ApiController]
[AllowAnonymous]
// TODO: Re-enable after testing: [Authorize(Roles = "Admin,Lawyer")]
public class ReviewDocumentController : ControllerBase
{
    private readonly IDocumentReviewService _service;

    public ReviewDocumentController(IDocumentReviewService service)
    {
        _service = service;
    }

    [HttpPost("review-document")]
    public async Task<ActionResult<ApiResponse<AnalyzeResponse>>> ReviewDocument([FromForm] ReviewDocumentRequest request, CancellationToken cancellationToken)
    {
        var result = await _service.ReviewDocumentAsync(request, cancellationToken);
        return Ok(ApiResponse<AnalyzeResponse>.Ok(result));
    }
}
