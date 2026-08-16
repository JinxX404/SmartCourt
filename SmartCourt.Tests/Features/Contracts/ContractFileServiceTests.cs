using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using SmartCourt.Common.Exceptions;
using SmartCourt.Common.Models;
using SmartCourt.Features.Contracts.Entities;
using SmartCourt.Features.Contracts.Files;
using SmartCourt.Features.Milestones.Entities;
using SmartCourt.Interfaces;
using SmartCourt.Interfaces.Providers;
using SmartCourt.Persistence;
using Xunit;

namespace SmartCourt.Tests.Features.Contracts;

public sealed class ContractFileServiceTests
{
    private static readonly DateTimeOffset Now =
        new(2026, 8, 16, 10, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task Upload_ParticipantPersistsContractOwnedFileWithSafeMetadata()
    {
        await using var context = CreateContext();
        var contract = AddContract(context);
        await context.SaveChangesAsync();
        var storage = new RecordingFileStorageService();
        var service = CreateService(
            context,
            contract.ClientUserId,
            storage);
        var file = CreateFormFile(
            "evidence.pdf",
            "%PDF-1.7\ncontract evidence"u8.ToArray());

        var result = await service.UploadAsync(
            contract.Id,
            new UploadContractFilesRequest { Files = [file] },
            CancellationToken.None);

        var dto = Assert.Single(result);
        Assert.Equal("evidence.pdf", dto.FileName);
        Assert.Equal("application/pdf", dto.ContentType);
        Assert.Equal(file.Length, dto.SizeInBytes);
        Assert.Equal(Now, dto.UploadedAt);

        var storedFile = await context.StoredFiles.SingleAsync();
        var attachment = await context.ContractAttachments.SingleAsync();
        Assert.Equal(dto.StoredFileId, storedFile.Id);
        Assert.Equal("evidence.pdf", storedFile.OriginalFileName);
        Assert.Equal(contract.Id, attachment.ContractId);
        Assert.Equal(contract.ClientUserId, attachment.UploadedByUserId);
        Assert.Equal(storedFile.Id, attachment.StoredFileId);
        Assert.DoesNotContain("evidence.pdf", storedFile.FileUrl);
        Assert.Contains(contract.Id.ToString("N"), storedFile.FileUrl);
        Assert.Contains(storedFile.Id.ToString("N"), storedFile.FileUrl);
    }

    [Fact]
    public async Task Upload_RejectsNonParticipantBeforeWritingStorage()
    {
        await using var context = CreateContext();
        var contract = AddContract(context);
        await context.SaveChangesAsync();
        var storage = new RecordingFileStorageService();
        var service = CreateService(context, Guid.NewGuid(), storage);

        await Assert.ThrowsAsync<ForbiddenAccessException>(() =>
            service.UploadAsync(
                contract.Id,
                new UploadContractFilesRequest
                {
                    Files =
                    [
                        CreateFormFile(
                            "evidence.pdf",
                            "%PDF-1.7\ntest"u8.ToArray())
                    ]
                },
                CancellationToken.None));

        Assert.Empty(storage.UploadedPaths);
        Assert.Empty(context.StoredFiles);
        Assert.Empty(context.ContractAttachments);
    }

    [Fact]
    public async Task Upload_RejectsContentThatDoesNotMatchExtension()
    {
        await using var context = CreateContext();
        var contract = AddContract(context);
        await context.SaveChangesAsync();
        var storage = new RecordingFileStorageService();
        var service = CreateService(
            context,
            contract.LawyerUserId,
            storage);

        var exception = await Assert.ThrowsAsync<BusinessException>(() =>
            service.UploadAsync(
                contract.Id,
                new UploadContractFilesRequest
                {
                    Files =
                    [
                        CreateFormFile(
                            "forged.pdf",
                            "not a pdf"u8.ToArray())
                    ]
                },
                CancellationToken.None));

        Assert.Contains("does not match", exception.Message);
        Assert.Empty(storage.UploadedPaths);
    }

    [Fact]
    public async Task Upload_WhenLaterStorageWriteFails_CleansEarlierUpload()
    {
        await using var context = CreateContext();
        var contract = AddContract(context);
        await context.SaveChangesAsync();
        var storage = new RecordingFileStorageService
        {
            ThrowOnUploadNumber = 2
        };
        var service = CreateService(
            context,
            contract.ClientUserId,
            storage);

        await Assert.ThrowsAsync<IOException>(() => service.UploadAsync(
            contract.Id,
            new UploadContractFilesRequest
            {
                Files =
                [
                    CreateFormFile("first.txt", "first"u8.ToArray()),
                    CreateFormFile("second.txt", "second"u8.ToArray())
                ]
            },
            CancellationToken.None));

        Assert.Single(storage.UploadedPaths);
        Assert.Equal(storage.UploadedPaths, storage.DeletedPaths);
        Assert.Empty(context.ContractAttachments);
    }

    [Fact]
    public async Task Upload_WhenDatabasePersistenceFails_CleansUploadedObject()
    {
        var databaseName = $"contract-file-db-failure-{Guid.NewGuid():N}";
        await using var context = new ThrowingSaveDbContext(
            new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName)
                .Options);
        var contract = AddContract(context);
        await context.SaveChangesAsync();
        var storage = new RecordingFileStorageService();
        var service = CreateService(
            context,
            contract.ClientUserId,
            storage);
        context.ThrowOnSave = true;

        await Assert.ThrowsAsync<DbUpdateException>(() => service.UploadAsync(
            contract.Id,
            new UploadContractFilesRequest
            {
                Files =
                [
                    CreateFormFile(
                        "evidence.pdf",
                        "%PDF-1.7\nevidence"u8.ToArray())
                ]
            },
            CancellationToken.None));

        Assert.Single(storage.UploadedPaths);
        Assert.Equal(storage.UploadedPaths, storage.DeletedPaths);
    }

    [Fact]
    public async Task Delete_OwnerCanRemoveUnusedFileAndStorageObject()
    {
        await using var context = CreateContext();
        var contract = AddContract(context);
        var storedFile = AddContractFile(
            context,
            contract,
            contract.ClientUserId);
        await context.SaveChangesAsync();
        var storage = new RecordingFileStorageService();
        var service = CreateService(
            context,
            contract.ClientUserId,
            storage);

        await service.DeleteAsync(
            contract.Id,
            storedFile.Id,
            CancellationToken.None);

        Assert.Empty(context.StoredFiles);
        Assert.Empty(context.ContractAttachments);
        Assert.Equal([storedFile.FileUrl], storage.DeletedPaths);
    }

    [Fact]
    public async Task Delete_RejectsFileReferencedByMilestoneSubmission()
    {
        await using var context = CreateContext();
        var contract = AddContract(context);
        var storedFile = AddContractFile(
            context,
            contract,
            contract.LawyerUserId);
        var milestone = new Milestone(
            Guid.NewGuid(),
            contract.Id,
            "مرحلة اختبار الملفات",
            null,
            1,
            1_000m,
            14,
            null,
            Now);
        var submission = new MilestoneSubmission(
            Guid.NewGuid(),
            milestone.Id,
            Guid.NewGuid(),
            contract.LawyerUserId,
            1,
            "تسليم للاختبار",
            Now);
        context.AddRange(
            milestone,
            submission,
            new MilestoneSubmissionAttachment(
                Guid.NewGuid(),
                submission.Id,
                storedFile.Id,
                Now));
        await context.SaveChangesAsync();
        var storage = new RecordingFileStorageService();
        var service = CreateService(
            context,
            contract.LawyerUserId,
            storage);

        await Assert.ThrowsAsync<ConflictException>(() =>
            service.DeleteAsync(
                contract.Id,
                storedFile.Id,
                CancellationToken.None));

        Assert.NotEmpty(context.StoredFiles);
        Assert.Empty(storage.DeletedPaths);
    }

    private static ContractFileService CreateService(
        ApplicationDbContext context,
        Guid actorUserId,
        RecordingFileStorageService storage)
    {
        return new ContractFileService(
            context,
            new StubCurrentUserService(actorUserId),
            storage,
            new FixedTimeProvider(),
            NullLogger<ContractFileService>.Instance);
    }

    private static Contract AddContract(ApplicationDbContext context)
    {
        var contract = new Contract(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            "عقد اختبار الملفات",
            "شروط صالحة لاختبار ملفات العقد.",
            Now);
        context.Contracts.Add(contract);
        return contract;
    }

    private static SmartCourt.Entities.StoredFile AddContractFile(
        ApplicationDbContext context,
        Contract contract,
        Guid ownerUserId)
    {
        var file = new SmartCourt.Entities.StoredFile
        {
            Id = Guid.NewGuid(),
            StoredFileName = "stored.pdf",
            OriginalFileName = "evidence.pdf",
            FileUrl = $"contract-files/{contract.Id:N}/stored.pdf",
            ContentType = "application/pdf",
            Extension = ".pdf",
            SizeInBytes = 100
        };
        context.AddRange(
            file,
            new ContractAttachment(
                Guid.NewGuid(),
                contract.Id,
                file.Id,
                ownerUserId,
                Now));
        return file;
    }

    private static IFormFile CreateFormFile(
        string fileName,
        byte[] content)
    {
        var stream = new MemoryStream(content);
        return new FormFile(stream, 0, content.Length, "Files", fileName);
    }

    private static ApplicationDbContext CreateContext()
    {
        return new ApplicationDbContext(
            new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(
                    $"contract-file-service-{Guid.NewGuid():N}")
                .Options,
            new FixedTimeProvider());
    }

    private sealed class StubCurrentUserService(Guid userId)
        : ICurrentUserService
    {
        public Guid? UserId { get; } = userId;
        public bool IsAuthenticated => true;
    }

    private sealed class RecordingFileStorageService : IFileStorageService
    {
        private int _uploadCount;

        public int? ThrowOnUploadNumber { get; init; }
        public List<string> UploadedPaths { get; } = [];
        public List<string> DeletedPaths { get; } = [];

        public Task<FileUploadResult> UploadAsync(
            Stream stream,
            string filePath,
            string originalFileName,
            CancellationToken cancellationToken = default)
            => UploadAsync(
                stream,
                filePath,
                originalFileName,
                null,
                cancellationToken);

        public Task<FileUploadResult> UploadAsync(
            Stream stream,
            string filePath,
            string originalFileName,
            string? contentType,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            _uploadCount++;
            if (_uploadCount == ThrowOnUploadNumber)
            {
                throw new IOException("Simulated storage failure.");
            }

            UploadedPaths.Add(filePath);
            return Task.FromResult(new FileUploadResult
            {
                StoragePath = filePath,
                OriginalFileName = originalFileName,
                Size = stream.Length
            });
        }

        public Task DeleteAsync(
            string filePath,
            CancellationToken cancellationToken = default)
        {
            DeletedPaths.Add(filePath);
            return Task.CompletedTask;
        }

        public Task<byte[]> DownloadAsync(
            string filePath,
            CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task<bool> ExistsAsync(
            string filePath,
            CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task<string> GetDownloadUrlAsync(
            string filePath,
            CancellationToken cancellationToken = default)
            => throw new NotSupportedException();
    }

    private sealed class FixedTimeProvider : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => Now;
    }

    private sealed class ThrowingSaveDbContext(
        DbContextOptions<ApplicationDbContext> options)
        : ApplicationDbContext(options, new FixedTimeProvider())
    {
        public bool ThrowOnSave { get; set; }

        public override Task<int> SaveChangesAsync(
            bool acceptAllChangesOnSuccess,
            CancellationToken cancellationToken = default)
        {
            if (ThrowOnSave)
            {
                return Task.FromException<int>(
                    new DbUpdateException("Simulated persistence failure."));
            }

            return base.SaveChangesAsync(
                acceptAllChangesOnSuccess,
                cancellationToken);
        }
    }
}
