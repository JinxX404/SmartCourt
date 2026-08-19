using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SmartCourt.Interfaces.Providers;

public interface IEmbeddingProvider
{
    /// <summary>
    /// Generate embeddings for a batch of text inputs.
    /// Returns the vectors and token usage.
    /// </summary>
    Task<EmbeddingResponse> GenerateEmbeddingsAsync(
        IReadOnlyList<string> texts,
        CancellationToken cancellationToken = default);

    /// <summary>The dimensionality of vectors this provider produces.</summary>
    int Dimensions { get; }
}

public record EmbeddingResponse(
    IReadOnlyList<float[]> Embeddings,
    int InputTokens
);
