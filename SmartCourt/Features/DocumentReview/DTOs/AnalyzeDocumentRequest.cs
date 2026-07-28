using Microsoft.AspNetCore.Http;

namespace SmartCourt.Features.DocumentReview.DTOs;

public class AnalyzeDocumentRequest
{
    public IFormFile File { get; set; } = null!;
    public string Query { get; set; } = string.Empty;
}
