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
public class AskLawController : ControllerBase
{
    private readonly IDocumentReviewService _service;

    public AskLawController(IDocumentReviewService service)
    {
        _service = service;
    }

    [HttpPost("ask-law")]
    public async Task<ActionResult<ApiResponse<AnalyzeResponse>>> AskLaw([FromBody] AskLawRequest request, CancellationToken cancellationToken)
    {
        var result = await _service.AskLawAsync(request, cancellationToken);
        return Ok(ApiResponse<AnalyzeResponse>.Ok(result));
    }
}
