using Microsoft.AspNetCore.Http;

namespace SmartCourt.Features.LawIngestion.DTOs;

public class IngestLawDocumentRequest
{
    public IFormFile File { get; set; } = null!;
    public string DocumentTitle { get; set; } = string.Empty;
    public string Language { get; set; } = string.Empty; // "ar" or "en"
    public string? Description { get; set; }
}
