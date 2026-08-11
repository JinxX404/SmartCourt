namespace SmartCourt.Providers.Reranker;

public class AlibabaRerankerOptions
{
    public const string SectionName = "AlibabaReranker";
    
    public string ApiKey { get; set; } = string.Empty;
    public string Model { get; set; } = "qwen3-rerank";
    public string BaseUrl { get; set; } = string.Empty;
}
