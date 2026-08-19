namespace SmartCourt.Interfaces.Providers;

public record TokenUsageMetadata(
    int InputTokens,
    int OutputTokens,
    int TotalTokens,
    string ModelName = "");

public record ChatModelResponse(
    string Content,
    TokenUsageMetadata Usage);
