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
    private readonly IRerankerProvider _rerankerProvider;
    private readonly IChatModelProvider _chatModelProvider;
    private readonly IDocumentParsingProvider _documentParsingProvider;
    private readonly LegalDocumentChunker _chunker;
    private readonly ILogger<DocumentReviewService> _logger;

    public DocumentReviewService(
        IEmbeddingProvider embeddingProvider,
        IVectorStoreProvider vectorStore,
        IRerankerProvider rerankerProvider,
        IChatModelProvider chatModelProvider,
        IDocumentParsingProvider documentParsingProvider,
        LegalDocumentChunker chunker,
        ILogger<DocumentReviewService> logger)
    {
        _embeddingProvider = embeddingProvider;
        _vectorStore = vectorStore;
        _rerankerProvider = rerankerProvider;
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
        
        // 2. Retrieve from the existing egyptian_law collection (over-fetch)
        var searchResults = await _vectorStore.SearchAsync(collectionName, queryEmbedding, topK: 20, filters: null, cancellationToken: cancellationToken);

        var retrievedChunks = searchResults
            .Where(r => r.Payload.ContainsKey("chunk_text"))
            .Select(r => r.Payload["chunk_text"].ToString())
            .ToList();

        // 3. Rerank to get the top 5 most relevant chunks
        if (retrievedChunks.Count > 0)
        {
            var reranked = await _rerankerProvider.RerankAsync(request.Query, retrievedChunks, topN: 5, cancellationToken);
            retrievedChunks = reranked.OrderByDescending(r => r.RelevanceScore).Select(r => retrievedChunks[r.Index]).ToList();
        }

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

        var systemPrompt = DocumentReviewPrompts.GetAskLawSystemPrompt(contextBuilder.ToString());

        var answer = await _chatModelProvider.GenerateAsync(systemPrompt, request.Query, cancellationToken);

        return new AnalyzeResponse
        {
            Answer = answer,
            ChunksUsed = retrievedChunks.Count,
            RetrievedContext = retrievedChunks
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

            // 4. Retrieve (over-fetch)
            var normalizedQuery = ArabicTextNormalizer.Normalize(query);
            var queryEmbedding = (await _embeddingProvider.GenerateEmbeddingsAsync(new[] { normalizedQuery }, cancellationToken)).First();
            var searchResults = await _vectorStore.SearchAsync(collectionName, queryEmbedding, topK: 20, filters: null, cancellationToken: cancellationToken);

            var retrievedChunks = searchResults
                .Where(r => r.Payload.ContainsKey("chunk_text"))
                .Select(r => r.Payload["chunk_text"].ToString())
                .ToList();

            // Rerank document chunks
            if (retrievedChunks.Count > 0)
            {
                var reranked = await _rerankerProvider.RerankAsync(query, retrievedChunks, topN: 5, cancellationToken);
                retrievedChunks = reranked.OrderByDescending(r => r.RelevanceScore).Select(r => retrievedChunks[r.Index]).ToList();
            }

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

            var documentContextString = documentContextBuilder.ToString();

            // 5a. Generate multi-queries for search
            var searchQueriesPrompt = DocumentReviewPrompts.GetMultiQuerySearchPrompt(documentContextString);
            var searchQueriesResponse = await _chatModelProvider.GenerateAsync(searchQueriesPrompt, "Generate queries", cancellationToken);
            
            var rawQueries = searchQueriesResponse
                .Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(q => q.Trim().TrimStart('-', '*', '1', '2', '3', '4', '5', '.').Trim())
                .Where(q => !string.IsNullOrWhiteSpace(q) && q.Length > 2)
                .Take(5)
                .ToList();

            if (rawQueries.Count == 0)
            {
                rawQueries.Add(normalizedQuery); // Fallback
            }

            // 5b. Generate document summary for reranking
            var rerankSummaryPrompt = DocumentReviewPrompts.GetRerankerSummaryPrompt(documentContextString);
            var documentSummary = await _chatModelProvider.GenerateAsync(rerankSummaryPrompt, "Generate summary", cancellationToken);
            var rerankQuery = $"{normalizedQuery}\n{documentSummary}";

            // Embed all queries
            var queryEmbeddings = await _embeddingProvider.GenerateEmbeddingsAsync(rawQueries, cancellationToken);
            
            // Search Qdrant for each query and aggregate results
            var allLawSearchResults = new List<SmartCourt.Interfaces.Providers.VectorSearchResult>();
            foreach (var qEmbedding in queryEmbeddings)
            {
                var lawSearchResults = await _vectorStore.SearchAsync("egyptian_law", qEmbedding, topK: 15, filters: null, cancellationToken: cancellationToken);
                allLawSearchResults.AddRange(lawSearchResults);
            }

            // Deduplicate by Payload's chunk_id or text
            var uniqueLawChunks = allLawSearchResults
                .GroupBy(r => r.Payload.ContainsKey("chunk_text") ? r.Payload["chunk_text"].ToString() : Guid.NewGuid().ToString())
                .Select(g => g.OrderByDescending(x => x.Score).First())
                .Take(40)
                .ToList();

            var retrievedLawTexts = uniqueLawChunks
                .Where(r => r.Payload.ContainsKey("chunk_text"))
                .Select(r => r.Payload["chunk_text"].ToString())
                .ToList();

            // Rerank law chunks using the document summary
            if (retrievedLawTexts.Count > 0)
            {
                var rerankedLaw = await _rerankerProvider.RerankAsync(rerankQuery, retrievedLawTexts, topN: 10, cancellationToken);
                var finalLawChunks = rerankedLaw.OrderByDescending(r => r.RelevanceScore).Select(r => uniqueLawChunks[r.Index]).ToList();

                var lawContextBuilder = new StringBuilder();
                for (int i = 0; i < finalLawChunks.Count; i++)
                {
                    var chunk = finalLawChunks[i];
                    var chunkText = chunk.Payload.ContainsKey("chunk_text") ? chunk.Payload["chunk_text"].ToString() : "";
                    var lawName = chunk.Payload.ContainsKey("law_name") ? chunk.Payload["law_name"].ToString() : "Unknown Law";
                    var articleNumber = chunk.Payload.ContainsKey("article_number") ? chunk.Payload["article_number"].ToString() : "";
                    var articleLabel = string.IsNullOrWhiteSpace(articleNumber) ? "" : $" - المادة {articleNumber}";

                    lawContextBuilder.AppendLine($"--- Law Snippet {i + 1} [{lawName}{articleLabel}] ---");
                    lawContextBuilder.AppendLine(chunkText);
                    lawContextBuilder.AppendLine();
                }

                // 6. Generate final response
                var systemPrompt = DocumentReviewPrompts.GetReviewDocumentSystemPrompt(documentContextString, lawContextBuilder.ToString());

                var answer = await _chatModelProvider.GenerateAsync(systemPrompt, query, cancellationToken);

                return new AnalyzeResponse
                {
                    Answer = answer,
                    ChunksUsed = retrievedChunks.Count + finalLawChunks.Count,
                    RetrievedContext = retrievedChunks.Concat(finalLawChunks.Select(c => c.Payload.ContainsKey("chunk_text") ? c.Payload["chunk_text"].ToString() : "")).ToList()
                };
            }

            return new AnalyzeResponse { Answer = "Could not find relevant Egyptian law to review the document.", ChunksUsed = retrievedChunks.Count };
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
