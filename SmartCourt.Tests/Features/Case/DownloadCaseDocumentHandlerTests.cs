using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SmartCourt.Common.Entities;
using SmartCourt.Common.Enums;
using SmartCourt.Common.Exceptions;
using SmartCourt.Common.Models;
using SmartCourt.Entities;
using SmartCourt.Features.Case.DownloadCaseDocument;
using SmartCourt.Interfaces.Providers;
using SmartCourt.Persistence;
using Xunit;

namespace SmartCourt.Tests.Features.Case;

public class DownloadCaseDocumentHandlerTests
{
    private static DbContextOptions<ApplicationDbContext> CreateSQLiteOptions()
    {
        return new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlite($"Data Source={Guid.NewGuid()}.db")
            .Options;
    }

    private sealed class TestFileStorageService : IFileStorageService
    {
        public byte[] DownloadBytesToReturn { get; set; } = Array.Empty<byte>();
        public string? DownloadRequestedPath { get; private set; }

        public Task<byte[]> DownloadAsync(string filePath, CancellationToken cancellationToken = default)
        {
            DownloadRequestedPath = filePath;
            return Task.FromResult(DownloadBytesToReturn);
        }

        public Task DeleteAsync(string filePath, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task<bool> ExistsAsync(string filePath, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task<string> GetDownloadUrlAsync(string filePath, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task<FileUploadResult> UploadAsync(Stream stream, string filePath, string originalFileName, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task<FileUploadResult> UploadAsync(Stream stream, string filePath, string originalFileName, string? contentType, CancellationToken cancellationToken = default) => throw new NotSupportedException();
    }

    private static ApplicationUser SeedClient(ApplicationDbContext db, Guid clientId)
    {
        var user = new ApplicationUser
        {
            Id = clientId,
            UserName = $"client_{clientId:N}@example.com",
            Email = $"client_{clientId:N}@example.com",
            FullName = $"Client {clientId:N}"
        };
        var clientProfile = new ClientProfile { UserId = clientId, User = user };
        db.Users.Add(user);
        db.ClientProfile.Add(clientProfile);
        return user;
    }

    [Fact]
    public async Task Handle_ValidCaseAndDocumentId_ReturnsDocumentResult()
    {
        // Arrange
        var options = CreateSQLiteOptions();
        var caseId = Guid.NewGuid();
        var storedFileId = Guid.NewGuid();
        var clientId = Guid.NewGuid();
        var expectedBytes = new byte[] { 0x1, 0x2, 0x3, 0x4 };
        const string storagePath = "test-client/case-documents/doc123.pdf";
        const string originalFileName = "contract.pdf";
        const string contentType = "application/pdf";

        using (var db = new ApplicationDbContext(options))
        {
            db.Database.EnsureCreated();
            SeedClient(db, clientId);

            var legalCase = new SmartCourt.Entities.Case
            {
                Id = caseId,
                Title = "Test Case",
                Description = "Description",
                ClientId = clientId,
                Status = CaseStatus.Draft
            };
            db.Cases.Add(legalCase);

            var storedFile = new StoredFile
            {
                Id = storedFileId,
                StoredFileName = "doc123.pdf",
                OriginalFileName = originalFileName,
                ContentType = contentType,
                Extension = ".pdf",
                SizeInBytes = 4,
                FileUrl = storagePath
            };
            db.StoredFiles.Add(storedFile);

            var caseDocument = new CaseDocument
            {
                Id = Guid.NewGuid(),
                CaseId = caseId,
                StoredFileId = storedFileId
            };
            db.CaseDocuments.Add(caseDocument);

            await db.SaveChangesAsync();
        }

        var fakeStorageService = new TestFileStorageService { DownloadBytesToReturn = expectedBytes };

        using (var db = new ApplicationDbContext(options))
        {
            var handler = new DownloadCaseDocumentHandler(db, fakeStorageService);
            var query = new DownloadCaseDocumentQuery
            {
                CaseId = caseId,
                DocumentId = storedFileId
            };

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedBytes, result.FileBytes);
            Assert.Equal(contentType, result.ContentType);
            Assert.Equal(originalFileName, result.FileName);
            Assert.Equal(storagePath, fakeStorageService.DownloadRequestedPath);
        }
    }

    [Fact]
    public async Task Handle_DocumentNotFound_ThrowsBusinessException()
    {
        // Arrange
        var options = CreateSQLiteOptions();
        var fakeStorageService = new TestFileStorageService();

        using (var db = new ApplicationDbContext(options))
        {
            db.Database.EnsureCreated();
        }

        using (var db = new ApplicationDbContext(options))
        {
            var handler = new DownloadCaseDocumentHandler(db, fakeStorageService);
            var query = new DownloadCaseDocumentQuery
            {
                CaseId = Guid.NewGuid(),
                DocumentId = Guid.NewGuid()
            };

            // Act & Assert
            var ex = await Assert.ThrowsAsync<BusinessException>(() => handler.Handle(query, CancellationToken.None));
            Assert.Equal("Document not found for the specified case.", ex.Message);
        }
    }

    [Fact]
    public async Task Handle_DocumentBelongsToDifferentCase_ThrowsBusinessException()
    {
        // Arrange
        var options = CreateSQLiteOptions();
        var caseId1 = Guid.NewGuid();
        var caseId2 = Guid.NewGuid();
        var storedFileId = Guid.NewGuid();
        var clientId = Guid.NewGuid();

        using (var db = new ApplicationDbContext(options))
        {
            db.Database.EnsureCreated();
            SeedClient(db, clientId);

            var legalCase1 = new SmartCourt.Entities.Case
            {
                Id = caseId1,
                Title = "Case 1",
                Description = "Description 1",
                ClientId = clientId,
                Status = CaseStatus.Draft
            };
            db.Cases.Add(legalCase1);

            var storedFile = new StoredFile
            {
                Id = storedFileId,
                StoredFileName = "doc123.pdf",
                OriginalFileName = "contract.pdf",
                ContentType = "application/pdf",
                Extension = ".pdf",
                SizeInBytes = 4,
                FileUrl = "path/to/file"
            };
            db.StoredFiles.Add(storedFile);

            var caseDocument = new CaseDocument
            {
                Id = Guid.NewGuid(),
                CaseId = caseId1,
                StoredFileId = storedFileId
            };
            db.CaseDocuments.Add(caseDocument);

            await db.SaveChangesAsync();
        }

        var fakeStorageService = new TestFileStorageService();

        using (var db = new ApplicationDbContext(options))
        {
            var handler = new DownloadCaseDocumentHandler(db, fakeStorageService);
            var query = new DownloadCaseDocumentQuery
            {
                CaseId = caseId2, // wrong case ID
                DocumentId = storedFileId
            };

            // Act & Assert
            var ex = await Assert.ThrowsAsync<BusinessException>(() => handler.Handle(query, CancellationToken.None));
            Assert.Equal("Document not found for the specified case.", ex.Message);
        }
    }
}
