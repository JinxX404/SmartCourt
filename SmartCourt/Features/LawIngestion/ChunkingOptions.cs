namespace SmartCourt.Features.LawIngestion;

public class ChunkingOptions
{
    public const string SectionName = "Chunking";
    public int MaxChunkTokens { get; set; } = 512;
    public int OverlapTokens { get; set; } = 64;
    public int MinChunkTokens { get; set; } = 50;
}
