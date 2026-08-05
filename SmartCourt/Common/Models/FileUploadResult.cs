namespace SmartCourt.Common.Models
{
    public class FileUploadResult
    {
        public required string StoragePath { get; init; }
        public required string OriginalFileName { get; init; }
        public required long Size { get; init; }
    }
}
