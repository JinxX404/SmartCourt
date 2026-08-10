namespace SmartCourt.Features.Case.DownloadCaseDocument;

public class DownloadCaseDocumentResult
{
    public byte[] FileBytes { get; set; } = null!;
    public string ContentType { get; set; } = null!;
    public string FileName { get; set; } = null!;
}
