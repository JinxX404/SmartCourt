using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SmartCourt.Common.Models;
using SmartCourt.Interfaces.Providers;

namespace SmartCourt.Providers.FileStorage;

public sealed class LocalFileStorageService(
    IOptions<FileStorageOptions> options,
    ILogger<LocalFileStorageService> logger) : IFileStorageService
{
    private readonly string rootPath = ResolveRootPath(options.Value.BasePath);

    public async Task<FileUploadResult> UploadAsync(
        Stream stream,
        string filePath,
        string originalFileName,
        CancellationToken cancellationToken = default) =>
        await UploadAsync(
            stream,
            filePath,
            originalFileName,
            contentType: null,
            cancellationToken);

    public async Task<FileUploadResult> UploadAsync(
        Stream stream,
        string filePath,
        string originalFileName,
        string? contentType,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(stream);
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);
        ArgumentException.ThrowIfNullOrWhiteSpace(originalFileName);
        cancellationToken.ThrowIfCancellationRequested();

        var fullPath = ResolvePath(filePath);
        var directory = Path.GetDirectoryName(fullPath)
                        ?? throw new InvalidOperationException(
                            "تعذر تحديد مجلد تخزين الملف المحلي.");

        Directory.CreateDirectory(directory);
        await using var destination = new FileStream(
            fullPath,
            FileMode.Create,
            FileAccess.Write,
            FileShare.None,
            bufferSize: 64 * 1024,
            options: FileOptions.Asynchronous | FileOptions.SequentialScan);

        await stream.CopyToAsync(destination, cancellationToken);
        await destination.FlushAsync(cancellationToken);

        var size = destination.Length;
        logger.LogDebug(
            "Stored local file {FilePath} ({Size} bytes).",
            filePath,
            size);

        return new FileUploadResult
        {
            StoragePath = filePath.Replace('\\', '/'),
            OriginalFileName = originalFileName,
            Size = size
        };
    }

    public async Task<byte[]> DownloadAsync(
        string filePath,
        CancellationToken cancellationToken = default)
    {
        var fullPath = ResolvePath(filePath);
        return await File.ReadAllBytesAsync(fullPath, cancellationToken);
    }

    public Task DeleteAsync(
        string filePath,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var fullPath = ResolvePath(filePath);
        if (File.Exists(fullPath))
        {
            File.Delete(fullPath);
        }

        return Task.CompletedTask;
    }

    public Task<bool> ExistsAsync(
        string filePath,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(File.Exists(ResolvePath(filePath)));
    }

    public Task<string> GetDownloadUrlAsync(
        string filePath,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var fullPath = ResolvePath(filePath);
        if (!File.Exists(fullPath))
        {
            throw new FileNotFoundException(
                "لم يتم العثور على الملف المحلي المطلوب.",
                fullPath);
        }

        return Task.FromResult(new Uri(fullPath).AbsoluteUri);
    }

    private string ResolvePath(string filePath)
    {
        var normalizedPath = filePath
            .Replace('/', Path.DirectorySeparatorChar)
            .Replace('\\', Path.DirectorySeparatorChar);

        if (Path.IsPathRooted(normalizedPath))
        {
            throw new ArgumentException(
                "مسار الملف المحلي يجب أن يكون نسبيًا.",
                nameof(filePath));
        }

        var fullPath = Path.GetFullPath(
            Path.Combine(rootPath, normalizedPath));
        var relativePath = Path.GetRelativePath(rootPath, fullPath);
        if (Path.IsPathRooted(relativePath)
            || relativePath == ".."
            || relativePath.StartsWith(
                $"..{Path.DirectorySeparatorChar}",
                StringComparison.Ordinal))
        {
            throw new ArgumentException(
                "مسار الملف المحلي خارج مجلد التخزين المسموح.",
                nameof(filePath));
        }

        return fullPath;
    }

    private static string ResolveRootPath(string basePath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(basePath);
        var fullPath = Path.GetFullPath(basePath);
        Directory.CreateDirectory(fullPath);
        return fullPath.TrimEnd(
            Path.DirectorySeparatorChar,
            Path.AltDirectorySeparatorChar);
    }
}
