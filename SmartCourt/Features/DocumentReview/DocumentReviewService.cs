using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SmartCourt.Features.DocumentReview.DTOs;
using SmartCourt.Features.LawIngestion; // For ChunkingOptions
using SmartCourt.Interfaces.Providers;

namespace SmartCourt.Features.DocumentReview;

public class DocumentReviewService : IDocumentReviewService
{
    private readonly IEmbeddingProvider _embeddingProvider;
    private readonly IVectorStoreProvider _vectorStore;
    private readonly IChatModelProvider _chatModelProvider;
    private readonly IDocumentParsingProvider _documentParsingProvider;
    private readonly ChunkingOptions _chunkingOptions;
    private readonly ILogger<DocumentReviewService> _logger;

    public DocumentReviewService(
        IEmbeddingProvider embeddingProvider,
        IVectorStoreProvider vectorStore,
        IChatModelProvider chatModelProvider,
        IDocumentParsingProvider documentParsingProvider,
        IOptions<ChunkingOptions> chunkingOptions,
        ILogger<DocumentReviewService> logger)
    {
        _embeddingProvider = embeddingProvider;
        _vectorStore = vectorStore;
        _chatModelProvider = chatModelProvider;
        _documentParsingProvider = documentParsingProvider;
        _chunkingOptions = chunkingOptions.Value;
        _logger = logger;
    }

    public async Task<AnalyzeResponse> AnalyzeTextAsync(AnalyzeTextRequest request, CancellationToken cancellationToken = default)
    {
        return await ExecuteRagPipelineAsync(request.Text, request.Query, cancellationToken);
    }

    public async Task<AnalyzeResponse> AnalyzeDocumentAsync(AnalyzeDocumentRequest request, CancellationToken cancellationToken = default)
    {
        using var stream = request.File.OpenReadStream();
        var extractedText = await _documentParsingProvider.ExtractTextAsync(stream, request.File.FileName, cancellationToken);
        
        return await ExecuteRagPipelineAsync(extractedText, request.Query, cancellationToken);
    }

    public async Task<AnalyzeResponse> AskLawAsync(AskLawRequest request, CancellationToken cancellationToken = default)
    {
        var collectionName = "egyptian_law";

        // 1. Embed query
        var queryEmbedding = (await _embeddingProvider.GenerateEmbeddingsAsync(new[] { request.Query }, cancellationToken)).First();
        
        // 2. Retrieve from the existing egyptian_law collection
        var searchResults = await _vectorStore.SearchAsync(collectionName, queryEmbedding, topK: 5, filters: null, cancellationToken: cancellationToken);

        var retrievedChunks = searchResults
            .Where(r => r.Payload.ContainsKey("chunk_text"))
            .Select(r => r.Payload["chunk_text"].ToString())
            .ToList();

        if (retrievedChunks.Count == 0)
        {
            return new AnalyzeResponse { Answer = "Could not find relevant laws to answer your query.", ChunksUsed = 0 };
        }

        // 3. Generate answer
        var contextBuilder = new StringBuilder();
        for (int i = 0; i < retrievedChunks.Count; i++)
        {
            contextBuilder.AppendLine($"--- Law Snippet {i + 1} ---");
            contextBuilder.AppendLine(retrievedChunks[i]);
            contextBuilder.AppendLine();
        }

        var systemPrompt = $@"You are a highly capable legal analysis assistant specialized in Egyptian Law. 
Use the provided law snippets to answer the user's query.
If the snippets do not contain enough information to answer the query, clearly state that.
Do not hallucinate facts outside the provided context.

LAW CONTEXT:
{contextBuilder}";

        var answer = await _chatModelProvider.GenerateAsync(systemPrompt, request.Query, cancellationToken);

        return new AnalyzeResponse
        {
            Answer = answer,
            ChunksUsed = retrievedChunks.Count
        };
    }

    private async Task<AnalyzeResponse> ExecuteRagPipelineAsync(string text, string query, CancellationToken cancellationToken)
    {
        var collectionName = $"doc_review_{Guid.NewGuid():N}";
        
        try
        {
            // 1. Chunking
            var chunks = ChunkText(text);
            if (chunks.Count == 0)
            {
                return new AnalyzeResponse { Answer = "No text could be extracted for analysis.", ChunksUsed = 0 };
            }

            // 2. Ensure temporary collection exists
            await _vectorStore.EnsureCollectionExistsAsync(collectionName, _embeddingProvider.Dimensions, cancellationToken);

            // 3. Embed & Ingest
            var embeddings = await _embeddingProvider.GenerateEmbeddingsAsync(chunks, cancellationToken);
            var points = new List<VectorPoint>();
            for (int i = 0; i < chunks.Count; i++)
            {
                var payload = new Dictionary<string, object>
                {
                    { "chunk_text", chunks[i] }
                };
                points.Add(new VectorPoint(Guid.NewGuid(), embeddings[i], payload));
            }
            await _vectorStore.UpsertBatchAsync(collectionName, points, cancellationToken);

            // 4. Retrieve
            var queryEmbedding = (await _embeddingProvider.GenerateEmbeddingsAsync(new[] { query }, cancellationToken)).First();
            var searchResults = await _vectorStore.SearchAsync(collectionName, queryEmbedding, topK: 5, filters: null, cancellationToken: cancellationToken);

            var retrievedChunks = searchResults
                .Where(r => r.Payload.ContainsKey("chunk_text"))
                .Select(r => r.Payload["chunk_text"].ToString())
                .ToList();

            if (retrievedChunks.Count == 0)
            {
                return new AnalyzeResponse { Answer = "Could not find relevant information in the document to answer your query.", ChunksUsed = 0 };
            }

            // 5. Generate
            var contextBuilder = new StringBuilder();
            for (int i = 0; i < retrievedChunks.Count; i++)
            {
                contextBuilder.AppendLine($"--- Snippet {i + 1} ---");
                contextBuilder.AppendLine(retrievedChunks[i]);
                contextBuilder.AppendLine();
            }

            var systemPrompt = $@"You are a highly capable legal analysis assistant. 
Use the provided document snippets to answer the user's query.
If the snippets do not contain enough information to answer the query, clearly state that.
Do not hallucinate facts outside the provided context.

DOCUMENT CONTEXT:
{contextBuilder}";

            var answer = await _chatModelProvider.GenerateAsync(systemPrompt, query, cancellationToken);

            return new AnalyzeResponse
            {
                Answer = answer,
                ChunksUsed = retrievedChunks.Count
            };
        }
        finally
        {
            // 6. Cleanup (Best effort delete collection)
            try
            {
                await _vectorStore.DeleteCollectionAsync(collectionName, default);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to clean up temporary collection {CollectionName}", collectionName);
            }
        }
    }

    private List<string> ChunkText(string fullText)
    {
        var result = new List<string>();
        var words = fullText.Split(new[] { ' ', '\r', '\n', '\t' }, StringSplitOptions.RemoveEmptyEntries);
        
        int maxTokens = _chunkingOptions.MaxChunkTokens;
        int overlap = _chunkingOptions.OverlapTokens;

        if (words.Length == 0) return result;

        int i = 0;
        while (i < words.Length)
        {
            int take = Math.Min(maxTokens, words.Length - i);
            var chunkWords = words.Skip(i).Take(take);
            result.Add(string.Join(" ", chunkWords));
            
            i += (maxTokens - overlap);
            if (i >= words.Length || maxTokens <= overlap) break;
        }

        return result;
    }
}
