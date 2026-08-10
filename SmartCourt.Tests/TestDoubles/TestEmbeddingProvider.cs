using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SmartCourt.Interfaces.Providers;

namespace SmartCourt.Tests.TestDoubles;

public class TestEmbeddingProvider : IEmbeddingProvider
{
    public int Dimensions => 1536;

    public Task<IReadOnlyList<float[]>> GenerateEmbeddingsAsync(
        IReadOnlyList<string> texts,
        CancellationToken cancellationToken = default)
    {
        IReadOnlyList<float[]> result = texts.Select(_ => new float[Dimensions]).ToList();
        return Task.FromResult(result);
    }
}
