namespace SmartCourt.Features.DocumentReview.DTOs;

public class AnalyzeResponse
{
    public string Answer { get; set; } = string.Empty;
    public int ChunksUsed { get; set; }
}
