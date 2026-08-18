using System;

namespace SmartCourt.Features.ChatAgent.Entities;

public class ModelPricing
{
    public int Id { get; set; }
    public string ModelName { get; set; } = string.Empty;
    public string Region { get; set; } = string.Empty;
    
    // Price per 1,000,000 tokens
    public decimal InputPricePerMillion { get; set; }
    public decimal OutputPricePerMillion { get; set; }
    
    public DateTimeOffset EffectiveFrom { get; set; }
    public DateTimeOffset? EffectiveTo { get; set; }
    public bool IsActive { get; set; }
}
