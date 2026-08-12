using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using SmartCourt.Common.Models;
using SmartCourt.Interfaces.Providers;

namespace SmartCourt.Tests.TestDoubles;

public class TestFileStorageService : IFileStorageService
{
    public byte[] DownloadBytesToReturn { get; set; } = Array.Empty<byte>();

    public Task<FileUploadResult> UploadAsync(Stream stream, string filePath, string originalFileName, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(new FileUploadResult { StoragePath = filePath, OriginalFileName = originalFileName, Size = stream.Length });
    }

    public Task<FileUploadResult> UploadAsync(Stream stream, string filePath, string originalFileName, string? contentType, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(new FileUploadResult { StoragePath = filePath, OriginalFileName = originalFileName, Size = stream.Length });
    }

    public Task<byte[]> DownloadAsync(string filePath, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(DownloadBytesToReturn);
    }

    public Task DeleteAsync(string filePath, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    public Task<bool> ExistsAsync(string filePath, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(true);
    }

    public Task<string> GetDownloadUrlAsync(string filePath, CancellationToken cancellationToken = default)
    {
        return Task.FromResult($"http://test.url/{filePath}");
    }
}
