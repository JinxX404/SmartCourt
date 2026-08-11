namespace SmartCourt.Providers.ChatModel;

public class AlibabaChatModelOptions
{
    public const string SectionName = "AlibabaChatModel";
    public string ApiKey { get; set; } = string.Empty;
    public string Model { get; set; } = "qwen-plus";
    public string BaseUrl { get; set; } = "https://dashscope-intl.aliyuncs.com/compatible-mode/v1/";
    public int MaxTokens { get; set; } = 2000;
}
