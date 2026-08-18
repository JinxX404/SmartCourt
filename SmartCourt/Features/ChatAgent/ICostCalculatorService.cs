using System;
using System.Threading;
using System.Threading.Tasks;

namespace SmartCourt.Features.ChatAgent;

public record ModelUsageRecord(
    string ModelName,
    int InputTokens,
    int OutputTokens
);

public interface ICostCalculatorService
{
    /// <summary>
    /// Calculates the exact monetary cost based on the active pricing, 
    /// and logs the usage history to the database.
    /// </summary>
    Task RecordUsageAndCostAsync(
        Guid clientId, 
        Guid? conversationId, 
        ModelUsageRecord[] usages, 
        string region,
        CancellationToken cancellationToken = default);
}
