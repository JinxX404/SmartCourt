using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using SmartCourt.Common.Entities;
using SmartCourt.Common.Enums;
using SmartCourt.Common.Models;
using SmartCourt.Entities;
using SmartCourt.Features.Case.AddCaseDocument;
using SmartCourt.Features.Case.AddCaseDocument.DTOs;
using SmartCourt.Features.Case.AddCaseDocument.Validators;
using SmartCourt.Interfaces.Providers;
using SmartCourt.Persistence;
using Xunit;

namespace SmartCourt.Tests.Features.Case;

public class AddCaseDocumentServiceTests
{
    private static DbContextOptions<ApplicationDbContext> CreateSQLiteOptions()
    {
        return new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlite($"Data Source={Guid.NewGuid()}.db")
            .LogTo(Console.WriteLine, Microsoft.Extensions.Logging.LogLevel.Information)
            .Options;
    }

    private class TestFileStorageService : IFileStorageService
    {
        public List<string> UploadedPaths { get; } = new();

        public Task<FileUploadResult> UploadAsync(Stream stream, string filePath, string originalFileName, CancellationToken cancellationToken = default)
        {
            UploadedPaths.Add(filePath);
            return Task.FromResult(new FileUploadResult { StoragePath = filePath, OriginalFileName = originalFileName, Size = stream.Length });
        }

        public Task<FileUploadResult> UploadAsync(Stream stream, string filePath, string originalFileName, string? contentType, CancellationToken cancellationToken = default)
        {
            UploadedPaths.Add(filePath);
            return Task.FromResult(new FileUploadResult { StoragePath = filePath, OriginalFileName = originalFileName, Size = stream.Length });
        }

        public Task DeleteAsync(string filePath, CancellationToken cancellationToken = default)
        {
            UploadedPaths.Remove(filePath);
            return Task.CompletedTask;
        }

        public Task<byte[]> DownloadAsync(string filePath, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task<bool> ExistsAsync(string filePath, CancellationToken cancellationToken = default) => Task.FromResult(true);
        public Task<string> GetDownloadUrlAsync(string filePath, CancellationToken cancellationToken = default) => Task.FromResult(filePath);
    }

    private static IHttpContextAccessor CreateHttpContextAccessor(Guid? userId = null, string[]? roles = null)
    {
        var context = new DefaultHttpContext();
        if (userId.HasValue)
        {
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, userId.Value.ToString())
            };
            if (roles != null)
            {
                foreach (var role in roles)
                {
                    claims.Add(new Claim(ClaimTypes.Role, role));
                }
            }
            var identity = new ClaimsIdentity(claims, "TestAuth");
            context.User = new ClaimsPrincipal(identity);
        }

        return new HttpContextAccessor { HttpContext = context };
    }

    private static IFormFile CreateMockFormFile(string fileName, string contentType = "application/pdf", byte[]? content = null)
    {
        content ??= new byte[] { 1, 2, 3, 4 };
        var stream = new MemoryStream(content);
        return new FormFile(stream, 0, content.Length, "Documents", fileName)
        {
            Headers = new HeaderDictionary(),
            ContentType = contentType
        };
    }

    [Fact]
    public async Task AddDocumentsAsync_UnauthenticatedUser_Returns401()
    {
        // Arrange
        var options = CreateSQLiteOptions();
        using var db = new ApplicationDbContext(options);
        db.Database.EnsureCreated();

        var httpContextAccessor = CreateHttpContextAccessor(userId: null);
        var validator = new AddCaseDocumentRequestValidator();
        var storageService = new TestFileStorageService();
        var service = new AddCaseDocumentService(db, httpContextAccessor, validator, storageService);

        var request = new AddCaseDocumentRequest
        {
            Documents = new List<IFormFile> { CreateMockFormFile("doc.pdf") }
        };

        // Act
        var result = await service.AddDocumentsAsync(Guid.NewGuid(), request);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(401, result.StatusCode);
    }

    [Fact]
    public async Task AddDocumentsAsync_CaseNotFound_Returns404()
    {
        // Arrange
        var options = CreateSQLiteOptions();
        using var db = new ApplicationDbContext(options);
        db.Database.EnsureCreated();

        var userId = Guid.NewGuid();
        var httpContextAccessor = CreateHttpContextAccessor(userId);
        var validator = new AddCaseDocumentRequestValidator();
        var storageService = new TestFileStorageService();
        var service = new AddCaseDocumentService(db, httpContextAccessor, validator, storageService);

        var request = new AddCaseDocumentRequest
        {
            Documents = new List<IFormFile> { CreateMockFormFile("doc.pdf") }
        };

        // Act
        var result = await service.AddDocumentsAsync(Guid.NewGuid(), request);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(404, result.StatusCode);
    }

    [Fact]
    public async Task AddDocumentsAsync_UnauthorizedUser_Returns403()
    {
        // Arrange
        var options = CreateSQLiteOptions();
        var caseId = Guid.NewGuid();
        var ownerId = Guid.NewGuid();
        var unauthorizedUserId = Guid.NewGuid();

        using (var db = new ApplicationDbContext(options))
        {
            db.Database.EnsureCreated();
            var user = new ApplicationUser { Id = ownerId, UserName = "owner", Email = "owner@test.com", FullName = "Owner" };
            var clientProfile = new ClientProfile { UserId = ownerId, User = user };
            db.Users.Add(user);
            db.ClientProfile.Add(clientProfile);
            db.Cases.Add(new SmartCourt.Entities.Case
            {
                Id = caseId,
                Title = "Case 1",
                Description = "Desc",
                ClientId = ownerId,
                Status = CaseStatus.Draft
            });
            await db.SaveChangesAsync();
        }

        using (var db = new ApplicationDbContext(options))
        {
            var httpContextAccessor = CreateHttpContextAccessor(unauthorizedUserId);
            var validator = new AddCaseDocumentRequestValidator();
            var storageService = new TestFileStorageService();
            var service = new AddCaseDocumentService(db, httpContextAccessor, validator, storageService);

            var request = new AddCaseDocumentRequest
            {
                Documents = new List<IFormFile> { CreateMockFormFile("doc.pdf") }
            };

            // Act
            var result = await service.AddDocumentsAsync(caseId, request);

            // Assert
            Assert.False(result.Success);
            Assert.Equal(403, result.StatusCode);
        }
    }

    [Fact]
    public async Task AddDocumentsAsync_ValidRequest_ByCaseOwner_SuccessfullyAddsDocuments()
    {
        // Arrange
        var options = CreateSQLiteOptions();
        var caseId = Guid.NewGuid();
        var ownerId = Guid.NewGuid();

        using (var db = new ApplicationDbContext(options))
        {
            db.Database.EnsureCreated();
            var user = new ApplicationUser { Id = ownerId, UserName = "owner", Email = "owner@test.com", FullName = "Owner" };
            var clientProfile = new ClientProfile { UserId = ownerId, User = user };
            db.Users.Add(user);
            db.ClientProfile.Add(clientProfile);
            db.Cases.Add(new SmartCourt.Entities.Case
            {
                Id = caseId,
                Title = "Case 1",
                Description = "Desc",
                ClientId = ownerId,
                Status = CaseStatus.Draft
            });
            await db.SaveChangesAsync();
        }

        using (var db = new ApplicationDbContext(options))
        {
            var httpContextAccessor = CreateHttpContextAccessor(ownerId);
            var validator = new AddCaseDocumentRequestValidator();
            var storageService = new TestFileStorageService();
            var service = new AddCaseDocumentService(db, httpContextAccessor, validator, storageService);

            var request = new AddCaseDocumentRequest
            {
                Documents = new List<IFormFile>
                {
                    CreateMockFormFile("evidence.pdf"),
                    CreateMockFormFile("contract.docx", contentType: "application/vnd.openxmlformats-officedocument.wordprocessingml.document")
                }
            };

            // Act
            var result = await service.AddDocumentsAsync(caseId, request);

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal(caseId, result.Data.CaseId);
            Assert.Equal(2, result.Data.AddedDocuments.Count);
            Assert.Empty(result.Data.FailedDocuments);

            // Verify stored in DB
            var dbCaseDocuments = await db.CaseDocuments.Include(cd => cd.StoredFile).ToListAsync();
            Assert.Equal(2, dbCaseDocuments.Count);
        }
    }

    [Fact]
    public async Task AddDocumentsAsync_InvalidExtension_Returns400()
    {
        // Arrange
        var options = CreateSQLiteOptions();
        using var db = new ApplicationDbContext(options);
        db.Database.EnsureCreated();

        var userId = Guid.NewGuid();
        var httpContextAccessor = CreateHttpContextAccessor(userId);
        var validator = new AddCaseDocumentRequestValidator();
        var storageService = new TestFileStorageService();
        var service = new AddCaseDocumentService(db, httpContextAccessor, validator, storageService);

        var request = new AddCaseDocumentRequest
        {
            Documents = new List<IFormFile> { CreateMockFormFile("malicious.exe", contentType: "application/x-msdownload") }
        };

        // Act
        var result = await service.AddDocumentsAsync(Guid.NewGuid(), request);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(400, result.StatusCode);
    }
}
