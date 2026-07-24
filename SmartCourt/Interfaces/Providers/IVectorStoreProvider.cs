using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SmartCourt.Interfaces.Providers;

public interface IVectorStoreProvider
{
    Task EnsureCollectionExistsAsync(string collectionName, int vectorSize,
        CancellationToken cancellationToken = default);

    Task UpsertBatchAsync(string collectionName,
        IReadOnlyList<VectorPoint> points,
        CancellationToken cancellationToken = default);

    Task DeleteByDocumentIdAsync(string collectionName, Guid documentId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<VectorSearchResult>> SearchAsync(string collectionName,
        float[] queryVector, int topK,
        Dictionary<string, string>? filters = null,
        CancellationToken cancellationToken = default);
}

public record VectorPoint(
    Guid Id,
    float[] Vector,
    Dictionary<string, object> Payload);

public record VectorSearchResult(
    Guid Id,
    float Score,
    Dictionary<string, object> Payload);
