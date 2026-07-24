namespace SmartCourt.Features.LawIngestion.DTOs;

public class IngestLawDocumentRequest
{
    public string FilePath { get; set; } = string.Empty;
    public string DocumentTitle { get; set; } = string.Empty;
    public string Language { get; set; } = string.Empty; // "ar" or "en"
    public string? Description { get; set; }
}
