namespace SmartCourt.Providers.ChatModel;

public class GeminiChatModelOptions
{
    public const string SectionName = "GeminiChatModel";
    public string ApiKey { get; set; } = string.Empty;
    public string BaseUrl { get; set; } = string.Empty;
    public string Model { get; set; } = "gemini-2.0-flash";
}
