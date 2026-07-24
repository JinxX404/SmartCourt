namespace SmartCourt.Features.LawIngestion.DTOs;

public class LawChunkDto
{
    public string Text { get; set; } = string.Empty;
    public string Part { get; set; } = string.Empty;
    public string Chapter { get; set; } = string.Empty;
    public string Section { get; set; } = string.Empty;
    public int Article { get; set; }
    public int ChunkIndex { get; set; }
}
