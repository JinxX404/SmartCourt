using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Qdrant.Client;
using Qdrant.Client.Grpc;
using SmartCourt.Common.Exceptions;
using SmartCourt.Interfaces.Providers;

namespace SmartCourt.Providers.VectorStore;

public class QdrantVectorStoreProvider : IVectorStoreProvider
{
    private readonly QdrantClient _client;
    private readonly ILogger<QdrantVectorStoreProvider> _logger;

    public QdrantVectorStoreProvider(
        QdrantClient client,
        ILogger<QdrantVectorStoreProvider> logger)
    {
        _client = client;
        _logger = logger;
    }

    public async Task EnsureCollectionExistsAsync(string collectionName, int vectorSize, CancellationToken cancellationToken = default)
    {
        try
        {
            var exists = await _client.CollectionExistsAsync(collectionName, cancellationToken);
            if (!exists)
            {
                await _client.CreateCollectionAsync(
                    collectionName,
                    new VectorParams { Size = (ulong)vectorSize, Distance = Distance.Cosine },
                    cancellationToken: cancellationToken);

                _logger.LogInformation("Created Qdrant collection {CollectionName} with vector size {VectorSize}", collectionName, vectorSize);

                // Create payload indexes
                await _client.CreatePayloadIndexAsync(collectionName, "document_id", PayloadSchemaType.Keyword, cancellationToken: cancellationToken);
                await _client.CreatePayloadIndexAsync(collectionName, "document_title", PayloadSchemaType.Keyword, cancellationToken: cancellationToken);
                await _client.CreatePayloadIndexAsync(collectionName, "language", PayloadSchemaType.Keyword, cancellationToken: cancellationToken);
                await _client.CreatePayloadIndexAsync(collectionName, "part", PayloadSchemaType.Keyword, cancellationToken: cancellationToken);
                await _client.CreatePayloadIndexAsync(collectionName, "chapter", PayloadSchemaType.Keyword, cancellationToken: cancellationToken);
                await _client.CreatePayloadIndexAsync(collectionName, "article_number", PayloadSchemaType.Integer, cancellationToken: cancellationToken);
                await _client.CreatePayloadIndexAsync(collectionName, "version", PayloadSchemaType.Integer, cancellationToken: cancellationToken);
                
                _logger.LogInformation("Created payload indexes for {CollectionName}", collectionName);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to ensure Qdrant collection exists.");
            throw new BusinessException("Vector store initialization failed.", ex);
        }
    }

    public async Task UpsertBatchAsync(string collectionName, IReadOnlyList<VectorPoint> points, CancellationToken cancellationToken = default)
    {
        if (points.Count == 0) return;

        var qdrantPoints = points.Select(p => new PointStruct
        {
            Id = p.Id,
            Vectors = p.Vector,
            Payload = { ConvertPayload(p.Payload) }
        }).ToList();

        try
        {
            // Upsert in batches of 100 to avoid large payload errors
            int batchSize = 100;
            for (int i = 0; i < qdrantPoints.Count; i += batchSize)
            {
                var batch = qdrantPoints.Skip(i).Take(batchSize).ToList();
                await _client.UpsertAsync(collectionName, batch, cancellationToken: cancellationToken);
            }
            _logger.LogInformation("Upserted {Count} points to {CollectionName}", points.Count, collectionName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to upsert points to Qdrant.");
            throw new BusinessException("Failed to save embeddings to vector store.", ex);
        }
    }

    public async Task DeleteByDocumentIdAsync(string collectionName, Guid documentId, CancellationToken cancellationToken = default)
    {
        try
        {
            await _client.DeleteAsync(collectionName, new Filter
            {
                Must = { MatchKeyword("document_id", documentId.ToString()) }
            }, cancellationToken: cancellationToken);
            
            _logger.LogInformation("Deleted points for document {DocumentId} from {CollectionName}", documentId, collectionName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete points for document {DocumentId}.", documentId);
            throw new BusinessException("Failed to delete existing document embeddings.", ex);
        }
    }

    public async Task<IReadOnlyList<VectorSearchResult>> SearchAsync(
        string collectionName, 
        float[] queryVector, 
        int topK, 
        Dictionary<string, string>? filters = null, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            Filter? filter = null;
            if (filters != null && filters.Count > 0)
            {
                var conditions = filters.Select(kvp => MatchKeyword(kvp.Key, kvp.Value)).ToList();
                filter = new Filter { Must = { conditions } };
            }

            var results = await _client.SearchAsync(
                collectionName,
                queryVector,
                filter,
                limit: (ulong)topK,
                cancellationToken: cancellationToken);

            return results.Select(r => new VectorSearchResult(
                Guid.Parse(r.Id.Uuid),
                r.Score,
                ExtractPayload(r.Payload)
            )).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to search Qdrant.");
            throw new BusinessException("Vector search failed.", ex);
        }
    }

    private static Condition MatchKeyword(string key, string value)
    {
        return new Condition
        {
            Field = new FieldCondition
            {
                Key = key,
                Match = new Match { Keyword = value }
            }
        };
    }

    private static Dictionary<string, Value> ConvertPayload(Dictionary<string, object> dict)
    {
        var result = new Dictionary<string, Value>();
        foreach (var kvp in dict)
        {
            if (kvp.Value == null) continue;
            
            result[kvp.Key] = kvp.Value switch
            {
                string s => s,
                int i => i,
                long l => l,
                double d => d,
                bool b => b,
                Guid g => g.ToString(),
                _ => kvp.Value.ToString()!
            };
        }
        return result;
    }

    private static Dictionary<string, object> ExtractPayload(IDictionary<string, Value> map)
    {
        var result = new Dictionary<string, object>();
        foreach (var kvp in map)
        {
            result[kvp.Key] = kvp.Value.KindCase switch
            {
                Value.KindOneofCase.StringValue => kvp.Value.StringValue,
                Value.KindOneofCase.IntegerValue => kvp.Value.IntegerValue,
                Value.KindOneofCase.DoubleValue => kvp.Value.DoubleValue,
                Value.KindOneofCase.BoolValue => kvp.Value.BoolValue,
                _ => kvp.Value.ToString()! // Fallback
            };
        }
        return result;
    }
}
