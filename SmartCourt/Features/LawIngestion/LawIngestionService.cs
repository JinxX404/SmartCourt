using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SmartCourt.Common.Configuration;
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
    private readonly IFileStorageService _fileStorage;
    private readonly ILogger<LawIngestionService> _logger;
    private readonly RagOptions _ragOptions;

    public LawIngestionService(
        ApplicationDbContext dbContext,
        IBackgroundJobProvider backgroundJobProvider,
        IPdfParserProvider pdfParser,
        LegalDocumentChunker chunker,
        IEmbeddingProvider embeddingProvider,
        IVectorStoreProvider vectorStore,
        IFileStorageService fileStorage,
        ILogger<LawIngestionService> logger,
        IOptions<RagOptions> ragOptions)
    {
        _dbContext = dbContext;
        _backgroundJobProvider = backgroundJobProvider;
        _pdfParser = pdfParser;
        _chunker = chunker;
        _embeddingProvider = embeddingProvider;
        _vectorStore = vectorStore;
        _fileStorage = fileStorage;
        _logger = logger;
        _ragOptions = ragOptions.Value;
    }

    public async Task<IngestLawDocumentResponse> StartIngestionAsync(
        IngestLawDocumentRequest request,
        CancellationToken cancellationToken)
    {
        if (request.File == null || request.File.Length == 0)
        {
            throw new BusinessException("File is required and cannot be empty.");
        }

        var existingDoc = await _dbContext.LawDocuments
            .FirstOrDefaultAsync(d => d.DocumentTitle == request.DocumentTitle && d.Language == request.Language, cancellationToken);

        // Upload the replacement before changing the database record.
        using var stream = request.File.OpenReadStream();
        var filePath = $"law_documents/{Guid.NewGuid()}_{request.File.FileName}";
        var uploadResult = await _fileStorage.UploadAsync(stream, filePath, request.File.FileName, cancellationToken);

        var previousStoragePath = existingDoc?.FileStoragePath;
        var doc = existingDoc ?? new LawDocument();
        doc.FileName = request.File.FileName;
        doc.DocumentTitle = request.DocumentTitle;
        doc.Language = request.Language;
        doc.Description = request.Description;
        doc.FileStoragePath = uploadResult.StoragePath;
        doc.Status = IngestionStatus.Pending;
        doc.ErrorMessage = null;
        doc.TotalPages = 0;
        doc.ChunkCount = 0;
        doc.ProcessingStartedAt = null;
        doc.CompletedAt = null;
        if (existingDoc is not null)
        {
            doc.Version++;
        }

        if (existingDoc is null) _dbContext.LawDocuments.Add(doc);
        await _dbContext.SaveChangesAsync(cancellationToken);

        if (!string.IsNullOrWhiteSpace(previousStoragePath)
            && !string.Equals(previousStoragePath, uploadResult.StoragePath, StringComparison.Ordinal))
        {
            try { await _fileStorage.DeleteAsync(previousStoragePath, cancellationToken); }
            catch (Exception ex) { _logger.LogWarning(ex, "Failed to delete replaced law file {FilePath}", previousStoragePath); }
        }

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

        await _vectorStore.DeleteByDocumentIdAsync(_ragOptions.LegalCollectionName, documentId, cancellationToken);
        
        try
        {
            await _fileStorage.DeleteAsync(doc.FileStoragePath, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to delete physical file {FilePath}", doc.FileStoragePath);
        }
        
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
            doc.ProcessingStartedAt = DateTimeOffset.UtcNow;
            await _dbContext.SaveChangesAsync();

            // 1. Ensure Qdrant Collection exists
            await _vectorStore.EnsureCollectionExistsAsync(_ragOptions.LegalCollectionName, _embeddingProvider.Dimensions);

            // 2. Read PDF
            var fileBytes = await _fileStorage.DownloadAsync(doc.FileStoragePath);
            using var stream = new MemoryStream(fileBytes);
            
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
                await _vectorStore.DeleteByDocumentIdAsync(_ragOptions.LegalCollectionName, doc.Id);
            }

            // 6. Embed and Store (Batch processing)
            var points = new List<VectorPoint>(chunks.Count);
            for (var offset = 0; offset < chunks.Count; offset += _ragOptions.EmbeddingBatchSize)
            {
                var batch = chunks.Skip(offset).Take(_ragOptions.EmbeddingBatchSize).ToList();
                var embeddings = await _embeddingProvider.GenerateEmbeddingsAsync(batch.Select(c => c.Text).ToList());
                if (embeddings.Count != batch.Count || embeddings.Any(v => v.Length != _embeddingProvider.Dimensions))
                    throw new BusinessException("Embedding provider returned invalid vectors.");

                for (var i = 0; i < batch.Count; i++)
                {
                    var chunk = batch[i];
                    var chunkId = $"{doc.Id:N}:{doc.Version}:{offset + i}:{chunk.Article}:{chunk.ChunkIndex}";
                    var payload = new Dictionary<string, object>
                    {
                        { "chunk_id", chunkId },
                        { "document_id", doc.Id.ToString() },
                        { "document_title", doc.DocumentTitle },
                        { "law_name", doc.DocumentTitle },
                        { "language", doc.Language },
                        { "jurisdiction", _ragOptions.Jurisdiction },
                        { "source_type", "uploaded_law" },
                        { "part", chunk.Part },
                        { "chapter", chunk.Chapter },
                        { "section", chunk.Section },
                        { "article_number", chunk.Article },
                        { "chunk_index", offset + i },
                        { "chunk_text", chunk.Text },
                        { "version", doc.Version }
                    };
                    points.Add(new VectorPoint(DeterministicGuid(chunkId), embeddings[i], payload));
                }
            }

            await _vectorStore.UpsertBatchAsync(_ragOptions.LegalCollectionName, points);

            // 7. Complete
            doc.ChunkCount = points.Count;
            doc.Status = IngestionStatus.Completed;
            doc.CompletedAt = DateTimeOffset.UtcNow;
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

    private static Guid DeterministicGuid(string value)
    {
        var hash = SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(value));
        return new Guid(hash.AsSpan(0, 16));
    }
}
