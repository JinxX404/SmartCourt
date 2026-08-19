using System;

namespace SmartCourt.Features.ChatAgent.Entities;

public class ModelUsageHistory
{
    public Guid Id { get; set; }
    
    // Links to the request that initiated the model call
    public Guid ClientId { get; set; }
    public Guid? ConversationId { get; set; }
    
    public string ModelName { get; set; } = string.Empty;
    
    // Tokens Used
    public int InputTokens { get; set; }
    public int OutputTokens { get; set; }
    public int TotalTokens { get; set; }
    
    // Monetary Cost Tracking
    public decimal InputCost { get; set; }
    public decimal OutputCost { get; set; }
    public decimal TotalCost { get; set; }
    
    public DateTimeOffset CreatedAt { get; set; }
}
