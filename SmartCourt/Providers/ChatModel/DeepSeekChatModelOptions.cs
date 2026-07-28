namespace SmartCourt.Providers.ChatModel;

public class DeepSeekChatModelOptions
{
    public const string SectionName = "DeepSeekChatModel";

    public string BaseUrl { get; set; } = "https://api.deepseek.com/v1/";
    public string Model { get; set; } = "deepseek-chat";
    public string ApiKey { get; set; } = string.Empty;
}
