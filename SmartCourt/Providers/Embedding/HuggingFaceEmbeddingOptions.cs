namespace SmartCourt.Providers.Embedding;

public class HuggingFaceEmbeddingOptions
{
    public const string SectionName = "HuggingFaceEmbedding";
    public string ApiKey { get; set; } = string.Empty;
    
    // Model identifier on HF, e.g., "BAAI/bge-m3"
    public string Model { get; set; } = "BAAI/bge-m3";
    
    // Dimensions for BAAI/bge-m3
    public int Dimensions { get; set; } = 1024;
    
    // API endpoint, generally https://api-inference.huggingface.co/pipeline/feature-extraction/
    public string BaseUrl { get; set; } = "https://api-inference.huggingface.co/pipeline/feature-extraction/";
}
