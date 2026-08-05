using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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

        public Task<string> GetDownloadUrlAsync(string filePath, CancellationToken cancellationToken = default)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(filePath);

            var url = _client.Storage
                .From(_options.Bucket)
                .GetPublicUrl(filePath);

            return Task.FromResult(url);
        }

        public Task<FileUploadResult> UploadAsync(
            Stream stream,
            string filePath,
            string originalFileName,
            CancellationToken cancellationToken = default)
        {
            return UploadAsync(stream, filePath, originalFileName, contentType: null, cancellationToken);
        }

        public async Task<FileUploadResult> UploadAsync(
            Stream stream,
            string filePath,
            string originalFileName,
            string? contentType,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(stream);

            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentException("Storage path cannot be empty.", nameof(filePath));

            if (string.IsNullOrWhiteSpace(originalFileName))
                throw new ArgumentException("Original file name cannot be empty.", nameof(originalFileName));

            await using var memoryStream = new MemoryStream();
            await stream.CopyToAsync(memoryStream, cancellationToken);
            byte[] bytes = memoryStream.ToArray();

            var resolvedContentType = ResolveContentType(originalFileName, contentType);

            var fileOptions = new Supabase.Storage.FileOptions
            {
                ContentType = resolvedContentType,
                Upsert = true
            };

            await _client.Storage.From(_options.Bucket).Upload(bytes, filePath, fileOptions);

            return new FileUploadResult
            {
                StoragePath = filePath,
                OriginalFileName = originalFileName,
                Size = bytes.LongLength
            };
        }

        private static string ResolveContentType(string fileName, string? providedContentType)
        {
            var ext = Path.GetExtension(fileName).ToLowerInvariant();
            return ext switch
            {
                ".pdf" => "application/pdf",
                ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                ".doc" => "application/msword",
                ".txt" => "text/plain",
                ".png" => "image/png",
                ".jpg" or ".jpeg" => "image/jpeg",
                _ => !string.IsNullOrWhiteSpace(providedContentType) && providedContentType != "application/octet-stream"
                    ? providedContentType
                    : "application/octet-stream"
            };
        }
    }
}
