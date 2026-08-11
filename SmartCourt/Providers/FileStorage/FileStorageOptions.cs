namespace SmartCourt.Providers.FileStorage;

public sealed class FileStorageOptions
{
    public const string SectionName = "FileStorage";

    public string Provider { get; init; } = "Supabase";

    public string BasePath { get; init; } = "./uploads";
}
