namespace SmartCourt.Common.Configuration;

/// <summary>Operational limits and retrieval controls shared by all RAG entry points.</summary>
public sealed class RagOptions
{
    public const string SectionName = "Rag";

    public string LegalCollectionName { get; set; } = "egyptian_law";
    public int EmbeddingBatchSize { get; set; } = 32;
    public int CandidateCount { get; set; } = 20;
    public int RerankedCount { get; set; } = 5;
    public int MaxPromptContextCharacters { get; set; } = 24_000;
    public float MinimumSimilarityScore { get; set; } = 0.35f;
    public string Jurisdiction { get; set; } = "EG";
}
