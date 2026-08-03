namespace SmartCourt.Providers.ChatModel;

public class DeepSeekChatModelOptions
{
    public const string SectionName = "DeepSeekChatModel";

    public string BaseUrl { get; set; } = "https://api.deepseek.com/v1/";
    public string Model { get; set; } = "deepseek.r1-v1:0";
    public string ApiKey { get; set; } = "sbg_8WeGulirobgK5wPnVJz0S7XGETioi8mu";
    public int MaxTokens { get; set; } = 500;
}
