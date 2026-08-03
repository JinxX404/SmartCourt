using SmartCourt.Common.Models;

namespace SmartCourt.Interfaces.Providers
{
    public interface IFileStorageService
    {
        Task<FileUploadResult> UploadAsync(
            Stream stream,
            string filePath,
            string originalFileName,
            CancellationToken cancellationToken = default);

        Task<FileUploadResult> UploadAsync(
            Stream stream,
            string filePath,
            string originalFileName,
            string? contentType,
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

        /// <summary>
        /// Returns a URL that can be used to access the stored file.
        /// For providers that support signed URLs this should return a short-lived URL;
        /// for providers that only expose public URLs this returns the stored file URL.
        /// </summary>
        Task<string> GetDownloadUrlAsync(
            string filePath,
            CancellationToken cancellationToken = default);
    }
}
