using System;

namespace SmartCourt.Features.Case.AddCaseDocument.DTOs;

public class AddedDocumentDto
{
    public Guid DocumentId { get; set; }
    public Guid StoredFileId { get; set; }
    public string FileName { get; set; } = null!;
    public string FileUrl { get; set; } = null!;
    public long SizeInBytes { get; set; }
}
