using SmartCourt.Common.Domain;

namespace SmartCourt.Features.ChatAgent.Entities;

public sealed class TokenUsageHistory
{
    private TokenUsageHistory()
    {
    }

    internal TokenUsageHistory(Guid clientId, Guid conversationId, string model, int inputTokens, int outputTokens, int totalTokens)
    {
        Id = Guid.NewGuid();
        ClientId = EntityGuard.NotEmpty(clientId, nameof(clientId));
        ConversationId = EntityGuard.NotEmpty(conversationId, nameof(conversationId));
        if (string.IsNullOrWhiteSpace(model)) throw new ArgumentException("Model cannot be empty", nameof(model));
        Model = model;
        InputTokens = EntityGuard.NonNegative(inputTokens, nameof(inputTokens));
        OutputTokens = EntityGuard.NonNegative(outputTokens, nameof(outputTokens));
        TotalTokens = EntityGuard.NonNegative(totalTokens, nameof(totalTokens));
        CreatedAt = DateTimeOffset.UtcNow;
    }

    public Guid Id { get; private set; }
    public Guid ClientId { get; private set; }
    public Guid ConversationId { get; private set; }
    public string Model { get; private set; } = string.Empty;
    public int InputTokens { get; private set; }
    public int OutputTokens { get; private set; }
    public int TotalTokens { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }

    public static TokenUsageHistory Create(Guid clientId, Guid conversationId, string model, int inputTokens, int outputTokens, int totalTokens)
    {
        return new TokenUsageHistory(clientId, conversationId, model, inputTokens, outputTokens, totalTokens);
    }
}
