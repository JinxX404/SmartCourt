namespace SmartCourt.Providers.VectorStore;

public class QdrantOptions
{
    public const string SectionName = "Qdrant";
    public string Host { get; set; } = "localhost";
    public int Port { get; set; } = 6334; // gRPC port
    public bool UseTls { get; set; } = false;
    public string? ApiKey { get; set; }
    public string CollectionName { get; set; } = "egyptian_law";
}
