using System.Threading;
using System.Threading.Tasks;

namespace SmartCourt.Interfaces.Providers;

public interface IChatModelProvider
{
    Task<ChatModelResponse> GenerateAsync(string systemPrompt, string userPrompt, CancellationToken cancellationToken = default);
}
