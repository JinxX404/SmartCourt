using Microsoft.EntityFrameworkCore;
using SmartCourt.Common.Enums;
using SmartCourt.Common.Exceptions;
using SmartCourt.Common.Models;
using SmartCourt.Entities;
using SmartCourt.Features.Files.Integration;
using SmartCourt.Interfaces.Providers;
using SmartCourt.Persistence;
using Xunit;

namespace SmartCourt.Tests.Features.Files;

public sealed class ContractFileAccessServiceTests
{
    private static readonly DateTimeOffset Now =
        new(2026, 8, 12, 9, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task AuthorizeForUseAsync_AllowsOwnerAndRejectsOtherUser()
    {
        await using var context = CreateContext();
        var ownerUserId = Guid.NewGuid();
        var file = await AddOwnedFileAsync(context, ownerUserId);
        var service = CreateService(context);

        var authorized = await service.AuthorizeForUseAsync(
            ownerUserId,
            [file.Id],
            ContractFilePurpose.MilestoneSubmission,
            Guid.NewGuid(),
            CancellationToken.None);

        var result = Assert.Single(authorized);
        Assert.Equal(file.Id, result.StoredFileId);
        Assert.Equal(ownerUserId, result.OwnerUserId);
        await Assert.ThrowsAsync<ForbiddenAccessException>(() =>
            service.AuthorizeForUseAsync(
                Guid.NewGuid(),
                [file.Id],
                ContractFilePurpose.MilestoneSubmission,
                Guid.NewGuid(),
                CancellationToken.None));
    }

    [Fact]
    public async Task GetAuthorizedReadAccessAsync_ReturnsProviderUrlForOwner()
    {
        await using var context = CreateContext();
        var ownerUserId = Guid.NewGuid();
        var file = await AddOwnedFileAsync(context, ownerUserId);
        var service = CreateService(context);

        var access = await service.GetAuthorizedReadAccessAsync(
            ownerUserId,
            file.Id,
            ContractFilePurpose.MilestoneSubmission,
            Guid.NewGuid(),
            CancellationToken.None);

        Assert.NotNull(access);
        Assert.Equal(file.Id, access.StoredFileId);
        Assert.Equal(
            new Uri("https://files.example/submissions/file.pdf"),
            access.SignedUri);
        Assert.Equal(Now.AddMinutes(5), access.ExpiresAt);
    }

    private static ContractFileAccessService CreateService(
        ApplicationDbContext context)
    {
        return new ContractFileAccessService(
            context,
            new TestFileStorageService(),
            new FixedTimeProvider(Now));
    }

    private static async Task<StoredFile> AddOwnedFileAsync(
        ApplicationDbContext context,
        Guid ownerUserId)
    {
        var file = new StoredFile
        {
            Id = Guid.NewGuid(),
            StoredFileName = "file.pdf",
            OriginalFileName = "submission.pdf",
            FileUrl = "submissions/file.pdf",
            ContentType = "application/pdf",
            Extension = ".pdf",
            SizeInBytes = 100
        };
        context.StoredFiles.Add(file);
        context.UserVerificationDocuments.Add(
            new UserVerificationDocument
            {
                Id = Guid.NewGuid(),
                UserId = ownerUserId,
                StoredFileId = file.Id,
                StoredFile = file,
                DocumentType =
                    VerificationDocumentType.NationalIdFront,
                Status = VerificationDocumentStatus.Verified,
                ExpirationDate = new DateOnly(2030, 1, 1),
                IsCurrent = true
            });
        await context.SaveChangesAsync();
        return file;
    }

    private static ApplicationDbContext CreateContext()
    {
        return new ApplicationDbContext(
            new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(
                    $"contract-files-{Guid.NewGuid():N}")
                .Options,
            new FixedTimeProvider(Now));
    }

    private sealed class TestFileStorageService : IFileStorageService
    {
        public Task<string> GetDownloadUrlAsync(
            string filePath,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult(
                $"https://files.example/{filePath}");
        }

        public Task<FileUploadResult> UploadAsync(
            Stream stream,
            string filePath,
            string originalFileName,
            CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task<FileUploadResult> UploadAsync(
            Stream stream,
            string filePath,
            string originalFileName,
            string? contentType,
            CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task<byte[]> DownloadAsync(
            string filePath,
            CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task DeleteAsync(
            string filePath,
            CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task<bool> ExistsAsync(
            string filePath,
            CancellationToken cancellationToken = default)
            => throw new NotSupportedException();
    }

    private sealed class FixedTimeProvider(DateTimeOffset utcNow)
        : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => utcNow;
    }
}
