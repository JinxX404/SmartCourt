using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SmartCourt.Features.ChatAgent;
using SmartCourt.Features.ChatAgent.DTOs;

namespace SmartCourt.Tests.TestDoubles;

public class TestCostCalculatorService : ICostCalculatorService
{
    public Task RecordUsageAndCostAsync(Guid clientId, Guid? conversationId, ModelUsageRecord[] usages, string region, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }
}
