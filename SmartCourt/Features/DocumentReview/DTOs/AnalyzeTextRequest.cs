namespace SmartCourt.Features.DocumentReview.DTOs;

public class AnalyzeTextRequest
{
    public string Text { get; set; } = string.Empty;
    public string Query { get; set; } = string.Empty;
}
