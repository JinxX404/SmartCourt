namespace SmartCourt.Providers.Embedding;

public class GeminiEmbeddingOptions
{
    public const string SectionName = "GeminiEmbedding";
    public string ApiKey { get; set; } = string.Empty;
    
    // Model identifier, e.g., "gemini-embedding-2"
    public string Model { get; set; } = "gemini-embedding-2";
    
    // Configurable output dimensionality for Matryoshka Representation Learning (MRL)
    public int Dimensions { get; set; } = 1536;
    
    // API endpoint, generally https://generativelanguage.googleapis.com/v1beta/
    public string BaseUrl { get; set; } = "https://generativelanguage.googleapis.com/v1beta/";
}
