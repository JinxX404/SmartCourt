using System;

namespace SmartCourt.Features.LawIngestion.DTOs;

public class IngestLawDocumentResponse
{
    public Guid DocumentId { get; set; }
    public string Status { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}
