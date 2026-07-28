using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SmartCourt.Features.DocumentReview.DTOs;
using SmartCourt.Interfaces.Providers;
using SmartCourt.Providers.PdfParser;
using SmartCourt.Features.LawIngestion;

namespace SmartCourt.Features.DocumentReview;

public class DocumentReviewService : IDocumentReviewService
{
    private readonly IEmbeddingProvider _embeddingProvider;
    private readonly IVectorStoreProvider _vectorStore;
    private readonly IChatModelProvider _chatModelProvider;
    private readonly IDocumentParsingProvider _documentParsingProvider;
    private readonly LegalDocumentChunker _chunker;
    private readonly ILogger<DocumentReviewService> _logger;

    public DocumentReviewService(
        IEmbeddingProvider embeddingProvider,
        IVectorStoreProvider vectorStore,
        IChatModelProvider chatModelProvider,
        IDocumentParsingProvider documentParsingProvider,
        LegalDocumentChunker chunker,
        ILogger<DocumentReviewService> logger)
    {
        _embeddingProvider = embeddingProvider;
        _vectorStore = vectorStore;
        _chatModelProvider = chatModelProvider;
        _documentParsingProvider = documentParsingProvider;
        _chunker = chunker;
        _logger = logger;
    }


    public async Task<AnalyzeResponse> ReviewDocumentAsync(ReviewDocumentRequest request, CancellationToken cancellationToken = default)
    {
        using var stream = request.File.OpenReadStream();
        var extractedText = await _documentParsingProvider.ExtractTextAsync(stream, request.File.FileName, cancellationToken);
        
        return await ExecuteRagPipelineAsync(extractedText, request.Query, cancellationToken);
    }

    public async Task<AnalyzeResponse> AskLawAsync(AskLawRequest request, CancellationToken cancellationToken = default)
    {
        var collectionName = "egyptian_law";

        var normalizedQuery = ArabicTextNormalizer.Normalize(request.Query);

        // 1. Embed query
        var queryEmbedding = (await _embeddingProvider.GenerateEmbeddingsAsync(new[] { normalizedQuery }, cancellationToken)).First();
        
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
            // 1. Chunking (default to Arabic for document reviews)
            var chunks = _chunker.ChunkText(text, "ar");
            if (chunks.Count == 0)
            {
                return new AnalyzeResponse { Answer = "No text could be extracted for analysis.", ChunksUsed = 0 };
            }

            // 2. Ensure temporary collection exists
            await _vectorStore.EnsureCollectionExistsAsync(collectionName, _embeddingProvider.Dimensions, cancellationToken);

            // 3. Embed & Ingest
            var embeddings = await _embeddingProvider.GenerateEmbeddingsAsync(chunks.Select(c => c.Text).ToList(), cancellationToken);
            var points = new List<VectorPoint>();
            for (int i = 0; i < chunks.Count; i++)
            {
                var payload = new Dictionary<string, object>
                {
                    { "chunk_text", chunks[i].Text },
                    { "part", chunks[i].Part },
                    { "chapter", chunks[i].Chapter },
                    { "section", chunks[i].Section },
                    { "article_number", chunks[i].Article }
                };
                points.Add(new VectorPoint(Guid.NewGuid(), embeddings[i], payload));
            }
            await _vectorStore.UpsertBatchAsync(collectionName, points, cancellationToken);

            // 4. Retrieve
            var normalizedQuery = ArabicTextNormalizer.Normalize(query);
            var queryEmbedding = (await _embeddingProvider.GenerateEmbeddingsAsync(new[] { normalizedQuery }, cancellationToken)).First();
            var searchResults = await _vectorStore.SearchAsync(collectionName, queryEmbedding, topK: 5, filters: null, cancellationToken: cancellationToken);

            var retrievedChunks = searchResults
                .Where(r => r.Payload.ContainsKey("chunk_text"))
                .Select(r => r.Payload["chunk_text"].ToString())
                .ToList();

            if (retrievedChunks.Count == 0)
            {
                return new AnalyzeResponse { Answer = "Could not find relevant information in the document to answer your query.", ChunksUsed = 0 };
            }

            // 5. Retrieve from Egyptian Law based on combined context
            var documentContextBuilder = new StringBuilder();
            for (int i = 0; i < retrievedChunks.Count; i++)
            {
                documentContextBuilder.AppendLine($"--- Document Snippet {i + 1} ---");
                documentContextBuilder.AppendLine(retrievedChunks[i]);
                documentContextBuilder.AppendLine();
            }

            var combinedSearchText = $"{normalizedQuery}\n\nRelated Document Text:\n{documentContextBuilder}";
            var combinedEmbedding = (await _embeddingProvider.GenerateEmbeddingsAsync(new[] { combinedSearchText }, cancellationToken)).First();
            
            var lawSearchResults = await _vectorStore.SearchAsync("egyptian_law", combinedEmbedding, topK: 5, filters: null, cancellationToken: cancellationToken);
            var retrievedLawChunks = lawSearchResults
                .Where(r => r.Payload.ContainsKey("chunk_text"))
                .Select(r => r.Payload["chunk_text"].ToString())
                .ToList();

            var lawContextBuilder = new StringBuilder();
            for (int i = 0; i < retrievedLawChunks.Count; i++)
            {
                lawContextBuilder.AppendLine($"--- Law Snippet {i + 1} ---");
                lawContextBuilder.AppendLine(retrievedLawChunks[i]);
                lawContextBuilder.AppendLine();
            }

            // 6. Generate final response
            var systemPrompt = $@"You are a highly capable legal analysis assistant specialized in Egyptian Law.
Use the provided document snippets and the relevant law snippets to answer the user's query.
If the snippets do not contain enough information to answer the query, clearly state that.
Do not hallucinate facts outside the provided context.

DOCUMENT CONTEXT:
{documentContextBuilder}
LAW CONTEXT:
{lawContextBuilder}";

            var answer = await _chatModelProvider.GenerateAsync(systemPrompt, query, cancellationToken);

            return new AnalyzeResponse
            {
                Answer = answer,
                ChunksUsed = retrievedChunks.Count + retrievedLawChunks.Count
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

}
