using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SmartCourt.Features.ChatAgent.Entities;
using SmartCourt.Persistence;

namespace SmartCourt.Features.ChatAgent;

public class CostCalculatorService : ICostCalculatorService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly TimeProvider _timeProvider;

    public CostCalculatorService(ApplicationDbContext dbContext, TimeProvider timeProvider)
    {
        _dbContext = dbContext;
        _timeProvider = timeProvider;
    }

    public async Task RecordUsageAndCostAsync(
        Guid clientId,
        Guid? conversationId,
        ModelUsageRecord[] usages,
        string region,
        CancellationToken cancellationToken = default)
    {
        if (usages == null || usages.Length == 0) return;

        var modelNames = usages.Select(u => u.ModelName).Distinct().ToList();

        // Get active pricing for all models in the specified region
        var pricings = await _dbContext.ModelPricings
            .AsNoTracking()
            .Where(p => p.IsActive && p.Region == region && modelNames.Contains(p.ModelName))
            .ToListAsync(cancellationToken);

        var historyRecords = new List<ModelUsageHistory>();
        var timestamp = _timeProvider.GetUtcNow();

        foreach (var usage in usages)
        {
            var pricing = pricings.FirstOrDefault(p => p.ModelName == usage.ModelName);
            
            decimal inputCost = 0;
            decimal outputCost = 0;

            if (pricing != null)
            {
                inputCost = (usage.InputTokens / 1_000_000.0m) * pricing.InputPricePerMillion;
                outputCost = (usage.OutputTokens / 1_000_000.0m) * pricing.OutputPricePerMillion;
            }

            var record = new ModelUsageHistory
            {
                Id = Guid.NewGuid(),
                ClientId = clientId,
                ConversationId = conversationId,
                ModelName = usage.ModelName,
                InputTokens = usage.InputTokens,
                OutputTokens = usage.OutputTokens,
                TotalTokens = usage.InputTokens + usage.OutputTokens,
                InputCost = inputCost,
                OutputCost = outputCost,
                TotalCost = inputCost + outputCost,
                CreatedAt = timestamp
            };

            historyRecords.Add(record);
        }

        _dbContext.ModelUsageHistories.AddRange(historyRecords);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
