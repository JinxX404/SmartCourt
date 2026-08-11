using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using SmartCourt.Providers.FileStorage;
using Xunit;

namespace SmartCourt.Tests.Providers.FileStorage;

public sealed class LocalFileStorageServiceTests
{
    [Fact]
    public async Task UploadDownloadExistsAndDeleteUseConfiguredRoot()
    {
        var root = CreateTemporaryRoot();
        try
        {
            var service = CreateService(root);
            await using var input = new MemoryStream([1, 2, 3, 4]);

            var upload = await service.UploadAsync(
                input,
                "user-id/national-id/file.jpg",
                "document.jpg",
                "image/jpeg");

            Assert.Equal("user-id/national-id/file.jpg", upload.StoragePath);
            Assert.Equal(4, upload.Size);
            Assert.True(await service.ExistsAsync(upload.StoragePath));
            Assert.Equal(
                [1, 2, 3, 4],
                await service.DownloadAsync(upload.StoragePath));

            var downloadUrl = await service.GetDownloadUrlAsync(upload.StoragePath);
            Assert.True(Uri.TryCreate(downloadUrl, UriKind.Absolute, out var uri));
            Assert.Equal(Uri.UriSchemeFile, uri!.Scheme);

            await service.DeleteAsync(upload.StoragePath);
            Assert.False(await service.ExistsAsync(upload.StoragePath));
        }
        finally
        {
            DeleteTemporaryRoot(root);
        }
    }

    [Theory]
    [InlineData("../outside.txt")]
    [InlineData("..\\outside.txt")]
    [InlineData("C:\\outside.txt")]
    public void PathTraversalIsRejected(string path)
    {
        var root = CreateTemporaryRoot();
        try
        {
            var service = CreateService(root);
            Assert.Throws<ArgumentException>(() =>
                service.ExistsAsync(path).GetAwaiter().GetResult());
        }
        finally
        {
            DeleteTemporaryRoot(root);
        }
    }

    private static LocalFileStorageService CreateService(string root) =>
        new(
            Options.Create(new FileStorageOptions
            {
                Provider = "Local",
                BasePath = root
            }),
            NullLogger<LocalFileStorageService>.Instance);

    private static string CreateTemporaryRoot()
    {
        var root = Path.Combine(
            Path.GetTempPath(),
            "smartcourt-local-storage-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        return root;
    }

    private static void DeleteTemporaryRoot(string root)
    {
        if (Directory.Exists(root))
        {
            Directory.Delete(root, recursive: true);
        }
    }
}
