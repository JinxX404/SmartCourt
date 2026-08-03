namespace SmartCourt.Providers.ChatModel;

public class DeepSeekChatModelOptions
{
    public const string SectionName = "DeepSeekChatModel";

    public string BaseUrl { get; set; } = "http://apiaccess.iti.net.eg/api/v1/";
    public string Model { get; set; } = "deepseek.r1-v1:0";
    public string ApiKey { get; set; } = string.Empty;
    public int MaxTokens { get; set; } = 300;
}
