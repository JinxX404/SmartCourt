namespace SmartCourt.Features.DocumentReview.DTOs;

using System.Collections.Generic;

public class AnalyzeResponse
{
    public string Answer { get; set; } = string.Empty;
    public int ChunksUsed { get; set; }
    public List<string> RetrievedContext { get; set; } = new();
}
