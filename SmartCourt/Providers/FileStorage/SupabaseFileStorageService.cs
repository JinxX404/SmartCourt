using Microsoft.Extensions.Options;
using SmartCourt.Common.Models;
using SmartCourt.Common.Options;
using SmartCourt.Interfaces.Providers;

namespace SmartCourt.Providers.FileStorage
{
    public class SupabaseFileStorageService : IFileStorageService
    {
        private readonly Supabase.Client _client;
        private readonly SupabaseOptions _options;

        public SupabaseFileStorageService(Supabase.Client client, IOptions<SupabaseOptions> options)
        {
            _client = client;
            _options = options.Value;
        }

        public async Task DeleteAsync(string filePath, CancellationToken cancellationToken = default)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(filePath);

            await _client.Storage
                .From(_options.Bucket)
                .Remove(new List<string> { filePath });
        }

        public async Task<byte[]> DownloadAsync(string filePath, CancellationToken cancellationToken = default)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(filePath);

            return await _client.Storage
                .From(_options.Bucket)
                .Download(filePath, transformOptions: null, onProgress: null);
        }

        public async Task<bool> ExistsAsync(string filePath, CancellationToken cancellationToken = default)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(filePath);

            var directory = Path.GetDirectoryName(filePath)?.Replace("\\", "/") ?? string.Empty;
            var fileName = Path.GetFileName(filePath);

            var files = await _client.Storage
                .From(_options.Bucket)
                .List(directory);

            return files.Any(file =>
                file.Name.Equals(fileName, StringComparison.Ordinal));
        }

        public async Task<FileUploadResult> UploadAsync(Stream stream, string filePath, string originalFileName, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(stream);

            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentException("Storage path cannot be empty.", nameof(filePath));

            if (string.IsNullOrWhiteSpace(originalFileName))
                throw new ArgumentException("Original file name cannot be empty.", nameof(originalFileName));

            await using var memoryStream = new MemoryStream();

            await stream.CopyToAsync(memoryStream, cancellationToken);

            byte[] bytes = memoryStream.ToArray();

            await _client.Storage.From(_options.Bucket).Upload(bytes,filePath);

            return new FileUploadResult
            {
                StoragePath = filePath,
                OriginalFileName = originalFileName,
                Size = bytes.LongLength
            };
        }
    }
}
