using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartCourt.Common.Models;
using SmartCourt.Features.DocumentReview.DTOs;

namespace SmartCourt.Features.DocumentReview;

[Route("api/document-review")]
[ApiController]
// TODO: Re-enable after testing: [Authorize(Roles = "Admin,Lawyer")]
public class DocumentReviewController : ControllerBase
{
    private readonly IDocumentReviewService _service;

    public DocumentReviewController(IDocumentReviewService service)
    {
        _service = service;
    }

    [HttpPost("analyze-text")]
    public async Task<ActionResult<ApiResponse<AnalyzeResponse>>> AnalyzeText([FromBody] AnalyzeTextRequest request, CancellationToken cancellationToken)
    {
        var result = await _service.AnalyzeTextAsync(request, cancellationToken);
        return Ok(ApiResponse<AnalyzeResponse>.Ok(result));
    }

    [HttpPost("analyze-document")]
    public async Task<ActionResult<ApiResponse<AnalyzeResponse>>> AnalyzeDocument([FromForm] AnalyzeDocumentRequest request, CancellationToken cancellationToken)
    {
        var result = await _service.AnalyzeDocumentAsync(request, cancellationToken);
        return Ok(ApiResponse<AnalyzeResponse>.Ok(result));
    }

    [HttpPost("ask-law")]
    public async Task<ActionResult<ApiResponse<AnalyzeResponse>>> AskLaw([FromBody] AskLawRequest request, CancellationToken cancellationToken)
    {
        var result = await _service.AskLawAsync(request, cancellationToken);
        return Ok(ApiResponse<AnalyzeResponse>.Ok(result));
    }
}
