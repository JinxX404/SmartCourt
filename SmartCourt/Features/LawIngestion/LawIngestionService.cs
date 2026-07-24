using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SmartCourt.Common.Entities;
using SmartCourt.Common.Exceptions;
using SmartCourt.Features.LawIngestion.DTOs;
using SmartCourt.Interfaces.Providers;
using SmartCourt.Persistence;

namespace SmartCourt.Features.LawIngestion;

public class LawIngestionService : ILawIngestionService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IBackgroundJobProvider _backgroundJobProvider;
    private readonly IPdfParserProvider _pdfParser;
    private readonly LegalDocumentChunker _chunker;
    private readonly IEmbeddingProvider _embeddingProvider;
    private readonly IVectorStoreProvider _vectorStore;
    private readonly ILogger<LawIngestionService> _logger;
    private const string CollectionName = "egyptian_law";

    public LawIngestionService(
        ApplicationDbContext dbContext,
        IBackgroundJobProvider backgroundJobProvider,
        IPdfParserProvider pdfParser,
        LegalDocumentChunker chunker,
        IEmbeddingProvider embeddingProvider,
        IVectorStoreProvider vectorStore,
        ILogger<LawIngestionService> logger)
    {
        _dbContext = dbContext;
        _backgroundJobProvider = backgroundJobProvider;
        _pdfParser = pdfParser;
        _chunker = chunker;
        _embeddingProvider = embeddingProvider;
        _vectorStore = vectorStore;
        _logger = logger;
    }

    public async Task<IngestLawDocumentResponse> StartIngestionAsync(
        IngestLawDocumentRequest request,
        CancellationToken cancellationToken)
    {
        // Check if file exists locally (assuming it's a local path for now)
        if (!File.Exists(request.FilePath))
        {
            throw new NotFoundException($"File not found at path: {request.FilePath}");
        }

        // Check for existing document to increment version
        var existingDoc = await _dbContext.LawDocuments
            .FirstOrDefaultAsync(d => d.DocumentTitle == request.DocumentTitle && d.Language == request.Language, cancellationToken);

        var doc = new LawDocument
        {
            FileName = Path.GetFileName(request.FilePath),
            DocumentTitle = request.DocumentTitle,
            Language = request.Language,
            Description = request.Description,
            FileStoragePath = request.FilePath,
            Status = IngestionStatus.Pending,
            Version = existingDoc != null ? existingDoc.Version + 1 : 1
        };

        _dbContext.LawDocuments.Add(doc);
        await _dbContext.SaveChangesAsync(cancellationToken);

        // Enqueue background job
        _backgroundJobProvider.Enqueue<ILawIngestionService>(s => s.ExecuteIngestionAsync(doc.Id));

        return new IngestLawDocumentResponse
        {
            DocumentId = doc.Id,
            Status = "Pending",
            Message = "Ingestion job has been enqueued."
        };
    }

    public async Task<LawDocumentStatusResponse> GetStatusAsync(Guid documentId, CancellationToken cancellationToken)
    {
        var doc = await _dbContext.LawDocuments.FindAsync(new object[] { documentId }, cancellationToken);
        if (doc == null) throw new NotFoundException("Document not found.");

        return MapToDto(doc);
    }

    public async Task DeleteDocumentAsync(Guid documentId, CancellationToken cancellationToken)
    {
        var doc = await _dbContext.LawDocuments.FindAsync(new object[] { documentId }, cancellationToken);
        if (doc == null) throw new NotFoundException("Document not found.");

        await _vectorStore.DeleteByDocumentIdAsync(CollectionName, documentId, cancellationToken);
        
        _dbContext.LawDocuments.Remove(doc);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<List<LawDocumentStatusResponse>> ListDocumentsAsync(CancellationToken cancellationToken)
    {
        var docs = await _dbContext.LawDocuments
            .OrderByDescending(d => d.CreatedAt)
            .ToListAsync(cancellationToken);

        return docs.Select(MapToDto).ToList();
    }

    public async Task ExecuteIngestionAsync(Guid documentId)
    {
        var doc = await _dbContext.LawDocuments.FindAsync(documentId);
        if (doc == null) return;

        try
        {
            doc.Status = IngestionStatus.Processing;
            doc.ProcessingStartedAt = DateTime.UtcNow;
            await _dbContext.SaveChangesAsync();

            // 1. Ensure Qdrant Collection exists
            await _vectorStore.EnsureCollectionExistsAsync(CollectionName, _embeddingProvider.Dimensions);

            // 2. Read PDF
            if (!File.Exists(doc.FileStoragePath))
            {
                throw new FileNotFoundException($"PDF file missing: {doc.FileStoragePath}");
            }

            using var stream = File.OpenRead(doc.FileStoragePath);
            
            // 3. Parse PDF
            var parseResult = await _pdfParser.ParseAsync(stream);
            doc.TotalPages = parseResult.TotalPages;
            await _dbContext.SaveChangesAsync(); // Update page count

            // 4. Chunk
            var chunks = _chunker.ChunkDocument(parseResult.Pages, doc.Language);
            if (chunks.Count == 0) throw new BusinessException("No text could be extracted or chunked from the document.");

            // 5. Delete existing vectors if this is a re-ingestion
            if (doc.Version > 1)
            {
                await _vectorStore.DeleteByDocumentIdAsync(CollectionName, doc.Id);
            }

            // 6. Embed and Store (Batch processing)
            var points = new List<VectorPoint>();
            
            // Generate embeddings for all chunk texts
            var chunkTexts = chunks.Select(c => c.Text).ToList();
            var embeddings = await _embeddingProvider.GenerateEmbeddingsAsync(chunkTexts);

            for (int i = 0; i < chunks.Count; i++)
            {
                var chunk = chunks[i];
                var vector = embeddings[i];

                var payload = new Dictionary<string, object>
                {
                    { "document_id", doc.Id.ToString() },
                    { "document_title", doc.DocumentTitle },
                    { "language", doc.Language },
                    { "part", chunk.Part },
                    { "chapter", chunk.Chapter },
                    { "section", chunk.Section },
                    { "article_number", chunk.Article },
                    { "chunk_index", chunk.ChunkIndex },
                    { "chunk_text", chunk.Text },
                    { "version", doc.Version }
                };

                points.Add(new VectorPoint(Guid.NewGuid(), vector, payload));
            }

            await _vectorStore.UpsertBatchAsync(CollectionName, points);

            // 7. Complete
            doc.ChunkCount = points.Count;
            doc.Status = IngestionStatus.Completed;
            doc.CompletedAt = DateTime.UtcNow;
            await _dbContext.SaveChangesAsync();

            _logger.LogInformation("Successfully ingested document {DocumentId}. Generated {ChunkCount} chunks.", doc.Id, doc.ChunkCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to ingest document {DocumentId}", doc.Id);
            doc.Status = IngestionStatus.Failed;
            doc.ErrorMessage = ex.Message;
            await _dbContext.SaveChangesAsync();
            throw; // Rethrow for Hangfire to track failure
        }
    }

    private static LawDocumentStatusResponse MapToDto(LawDocument doc)
    {
        return new LawDocumentStatusResponse
        {
            DocumentId = doc.Id,
            FileName = doc.FileName,
            DocumentTitle = doc.DocumentTitle,
            Language = doc.Language,
            Status = doc.Status.ToString(),
            ErrorMessage = doc.ErrorMessage,
            TotalPages = doc.TotalPages,
            ChunkCount = doc.ChunkCount,
            Version = doc.Version,
            CompletedAt = doc.CompletedAt
        };
    }
}
