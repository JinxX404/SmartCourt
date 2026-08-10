namespace SmartCourt.Providers.Embedding;

public class AlibabaEmbeddingOptions
{
    public const string SectionName = "AlibabaEmbedding";
    public string ApiKey { get; set; } = string.Empty;
    public string Model { get; set; } = "text-embedding-v4";
    public int Dimensions { get; set; } = 1536;
    public string BaseUrl { get; set; } = string.Empty;
}
