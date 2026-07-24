using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SmartCourt.Interfaces.Providers;

namespace SmartCourt.Providers.Embedding;

public class MockEmbeddingProvider : IEmbeddingProvider
{
    // Keeping dimensions same as BAAI/bge-m3 so it matches the Qdrant collection we already created
    public int Dimensions => 1024;

    public Task<IReadOnlyList<float[]>> GenerateEmbeddingsAsync(IReadOnlyList<string> texts, CancellationToken cancellationToken = default)
    {
        var random = new Random();
        var results = new List<float[]>();

        foreach (var _ in texts)
        {
            var vector = new float[Dimensions];
            for (int i = 0; i < Dimensions; i++)
            {
                // Generate some dummy float values between -1.0 and 1.0
                vector[i] = (float)(random.NextDouble() * 2.0 - 1.0);
            }
            results.Add(vector);
        }

        return Task.FromResult<IReadOnlyList<float[]>>(results);
    }
}
