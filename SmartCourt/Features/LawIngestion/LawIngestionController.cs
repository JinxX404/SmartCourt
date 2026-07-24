using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartCourt.Common.Models;
using SmartCourt.Features.LawIngestion.DTOs;

namespace SmartCourt.Features.LawIngestion;

[Route("api/law-ingestion")]
[ApiController]
// TODO: Re-enable after testing: [Authorize(Roles = "Admin")]
public class LawIngestionController : ControllerBase
{
    private readonly ILawIngestionService _service;

    public LawIngestionController(ILawIngestionService service)
    {
        _service = service;
    }

    [HttpPost("ingest")]
    public async Task<ActionResult<ApiResponse<IngestLawDocumentResponse>>> Ingest([FromBody] IngestLawDocumentRequest request, CancellationToken cancellationToken)
    {
        var result = await _service.StartIngestionAsync(request, cancellationToken);
        return Ok(ApiResponse<IngestLawDocumentResponse>.Ok(result, "Ingestion started successfully."));
    }

    [HttpGet("{documentId}/status")]
    public async Task<ActionResult<ApiResponse<LawDocumentStatusResponse>>> GetStatus(Guid documentId, CancellationToken cancellationToken)
    {
        var result = await _service.GetStatusAsync(documentId, cancellationToken);
        return Ok(ApiResponse<LawDocumentStatusResponse>.Ok(result));
    }

    [HttpDelete("{documentId}")]
    public async Task<ActionResult<ApiResponse>> Delete(Guid documentId, CancellationToken cancellationToken)
    {
        await _service.DeleteDocumentAsync(documentId, cancellationToken);
        return Ok(ApiResponse.Ok("Document deleted successfully."));
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<LawDocumentStatusResponse>>>> ListDocuments(CancellationToken cancellationToken)
    {
        var result = await _service.ListDocumentsAsync(cancellationToken);
        return Ok(ApiResponse<List<LawDocumentStatusResponse>>.Ok(result));
    }
}
