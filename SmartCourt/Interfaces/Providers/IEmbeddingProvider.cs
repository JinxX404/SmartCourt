using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SmartCourt.Interfaces.Providers;

public interface IEmbeddingProvider
{
    /// <summary>
    /// Generate embeddings for a batch of text inputs.
    /// Returns vectors in the same order as the inputs.
    /// </summary>
    Task<IReadOnlyList<float[]>> GenerateEmbeddingsAsync(
        IReadOnlyList<string> texts,
        CancellationToken cancellationToken = default);

    /// <summary>The dimensionality of vectors this provider produces.</summary>
    int Dimensions { get; }
}
