using System.Threading;
using System.Threading.Tasks;
using SmartCourt.Interfaces.Providers;

namespace SmartCourt.Tests.TestDoubles;

public class TestChatModelProvider : IChatModelProvider
{
    public string OutputToReturn { get; set; } = "{}";

    public Task<ChatModelResponse> GenerateAsync(string systemPrompt, string userPrompt, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(new ChatModelResponse(OutputToReturn, new TokenUsageMetadata(0, 0, 0, "test-model")));
    }
}
