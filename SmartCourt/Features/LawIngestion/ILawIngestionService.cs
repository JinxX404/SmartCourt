using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SmartCourt.Features.LawIngestion.DTOs;

namespace SmartCourt.Features.LawIngestion;

public interface ILawIngestionService
{
    Task<IngestLawDocumentResponse> StartIngestionAsync(
        IngestLawDocumentRequest request,
        CancellationToken cancellationToken);

    Task<LawDocumentStatusResponse> GetStatusAsync(
        Guid documentId,
        CancellationToken cancellationToken);

    Task DeleteDocumentAsync(
        Guid documentId,
        CancellationToken cancellationToken);

    Task<List<LawDocumentStatusResponse>> ListDocumentsAsync(
        CancellationToken cancellationToken);

    /// <summary>
    /// The actual ingestion pipeline — called by Hangfire in the background.
    /// </summary>
    Task ExecuteIngestionAsync(Guid documentId);
}
