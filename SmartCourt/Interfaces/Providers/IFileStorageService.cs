using SmartCourt.Common;

namespace SmartCourt.Interfaces.Providers
{
    public interface IFileStorageService
    {
        Task<FileUploadResult> UploadAsync(
            Stream stream,
            string filePath,
            string originalFileName,
            CancellationToken cancellationToken = default);

        Task<byte[]> DownloadAsync(
            string filePath,
            CancellationToken cancellationToken = default);

        Task DeleteAsync(
            string filePath,
            CancellationToken cancellationToken = default);

        Task<bool> ExistsAsync(
            string filePath,
            CancellationToken cancellationToken = default);
    }
}
