using System;

namespace SmartCourt.Features.LawIngestion.DTOs;

public class LawDocumentStatusResponse
{
    public Guid DocumentId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string DocumentTitle { get; set; } = string.Empty;
    public string Language { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? ErrorMessage { get; set; }
    public int TotalPages { get; set; }
    public int ChunkCount { get; set; }
    public int Version { get; set; }
    public DateTime? CompletedAt { get; set; }
}
