using System;

namespace SmartCourt.Common.Entities;

public class LawDocument : AuditableEntity
{
    public string FileName { get; set; } = string.Empty;
    public string DocumentTitle { get; set; } = string.Empty;
    public string Language { get; set; } = string.Empty; // "ar" or "en"
    public string? Description { get; set; }

    public IngestionStatus Status { get; set; } = IngestionStatus.Pending;
    public string? ErrorMessage { get; set; }

    public int TotalPages { get; set; }
    public int ChunkCount { get; set; }
    public string? FileStoragePath { get; set; } // Path on local or cloud

    public DateTime? ProcessingStartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }

    /// <summary>Incremented on re-ingestion to version the vectors.</summary>
    public int Version { get; set; } = 1;
}

public enum IngestionStatus
{
    Pending = 0,
    Processing = 1,
    Completed = 2,
    Failed = 3
}
