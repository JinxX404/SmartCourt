using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SmartCourt.Interfaces.Providers;

namespace SmartCourt.Tests.TestDoubles;

public class TestVectorStoreProvider : IVectorStoreProvider
{
    public List<VectorSearchResult> SearchResultsToReturn { get; set; } = [];

    public Task EnsureCollectionExistsAsync(string collectionName, int vectorSize, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    public Task UpsertBatchAsync(string collectionName, IReadOnlyList<VectorPoint> points, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    public Task DeleteByDocumentIdAsync(string collectionName, Guid documentId, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<VectorSearchResult>> SearchAsync(
        string collectionName,
        float[] queryVector,
        int topK,
        Dictionary<string, string>? filters = null,
        CancellationToken cancellationToken = default)
    {
        IReadOnlyList<VectorSearchResult> results = SearchResultsToReturn;
        return Task.FromResult(results);
    }

    public Task DeleteCollectionAsync(string collectionName, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }
}
