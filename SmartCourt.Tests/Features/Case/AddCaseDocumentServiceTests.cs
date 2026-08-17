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
        var storedDocValidator = new AddStoredCaseDocumentRequestValidator();
        var storageService = new TestFileStorageService();
        var service = new AddCaseDocumentService(db, httpContextAccessor, validator, storedDocValidator, storageService);

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
        var storedDocValidator = new AddStoredCaseDocumentRequestValidator();
        var storageService = new TestFileStorageService();
        var service = new AddCaseDocumentService(db, httpContextAccessor, validator, storedDocValidator, storageService);

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
            var storedDocValidator = new AddStoredCaseDocumentRequestValidator();
            var storageService = new TestFileStorageService();
            var service = new AddCaseDocumentService(db, httpContextAccessor, validator, storedDocValidator, storageService);

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
            var storedDocValidator = new AddStoredCaseDocumentRequestValidator();
            var storageService = new TestFileStorageService();
            var service = new AddCaseDocumentService(db, httpContextAccessor, validator, storedDocValidator, storageService);

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
        var storedDocValidator = new AddStoredCaseDocumentRequestValidator();
        var storageService = new TestFileStorageService();
        var service = new AddCaseDocumentService(db, httpContextAccessor, validator, storedDocValidator, storageService);

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

    // --- AddStoredDocumentAsync Tests ---

    [Fact]
    public async Task AddStoredDocumentAsync_UnauthenticatedUser_Returns401()
    {
        // Arrange
        var options = CreateSQLiteOptions();
        using var db = new ApplicationDbContext(options);
        db.Database.EnsureCreated();

        var httpContextAccessor = CreateHttpContextAccessor(userId: null);
        var validator = new AddCaseDocumentRequestValidator();
        var storedDocValidator = new AddStoredCaseDocumentRequestValidator();
        var storageService = new TestFileStorageService();
        var service = new AddCaseDocumentService(db, httpContextAccessor, validator, storedDocValidator, storageService);

        var request = new AddStoredCaseDocumentRequest
        {
            StoredFileId = Guid.NewGuid()
        };

        // Act
        var result = await service.AddStoredDocumentAsync(Guid.NewGuid(), request);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(401, result.StatusCode);
    }

    [Fact]
    public async Task AddStoredDocumentAsync_EmptyStoredFileId_Returns400()
    {
        // Arrange
        var options = CreateSQLiteOptions();
        using var db = new ApplicationDbContext(options);
        db.Database.EnsureCreated();

        var userId = Guid.NewGuid();
        var httpContextAccessor = CreateHttpContextAccessor(userId);
        var validator = new AddCaseDocumentRequestValidator();
        var storedDocValidator = new AddStoredCaseDocumentRequestValidator();
        var storageService = new TestFileStorageService();
        var service = new AddCaseDocumentService(db, httpContextAccessor, validator, storedDocValidator, storageService);

        var request = new AddStoredCaseDocumentRequest
        {
            StoredFileId = Guid.Empty
        };

        // Act
        var result = await service.AddStoredDocumentAsync(Guid.NewGuid(), request);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(400, result.StatusCode);
    }

    [Fact]
    public async Task AddStoredDocumentAsync_CaseNotFound_Returns404()
    {
        // Arrange
        var options = CreateSQLiteOptions();
        using var db = new ApplicationDbContext(options);
        db.Database.EnsureCreated();

        var userId = Guid.NewGuid();
        var httpContextAccessor = CreateHttpContextAccessor(userId);
        var validator = new AddCaseDocumentRequestValidator();
        var storedDocValidator = new AddStoredCaseDocumentRequestValidator();
        var storageService = new TestFileStorageService();
        var service = new AddCaseDocumentService(db, httpContextAccessor, validator, storedDocValidator, storageService);

        var request = new AddStoredCaseDocumentRequest
        {
            StoredFileId = Guid.NewGuid()
        };

        // Act
        var result = await service.AddStoredDocumentAsync(Guid.NewGuid(), request);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(404, result.StatusCode);
    }

    [Fact]
    public async Task AddStoredDocumentAsync_UnauthorizedUser_Returns403()
    {
        // Arrange
        var options = CreateSQLiteOptions();
        var caseId = Guid.NewGuid();
        var ownerId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();

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
            var httpContextAccessor = CreateHttpContextAccessor(otherUserId);
            var validator = new AddCaseDocumentRequestValidator();
            var storedDocValidator = new AddStoredCaseDocumentRequestValidator();
            var storageService = new TestFileStorageService();
            var service = new AddCaseDocumentService(db, httpContextAccessor, validator, storedDocValidator, storageService);

            var request = new AddStoredCaseDocumentRequest
            {
                StoredFileId = Guid.NewGuid()
            };

            // Act
            var result = await service.AddStoredDocumentAsync(caseId, request);

            // Assert
            Assert.False(result.Success);
            Assert.Equal(403, result.StatusCode);
        }
    }

    [Fact]
    public async Task AddStoredDocumentAsync_StoredFileNotFound_Returns404()
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
            var storedDocValidator = new AddStoredCaseDocumentRequestValidator();
            var storageService = new TestFileStorageService();
            var service = new AddCaseDocumentService(db, httpContextAccessor, validator, storedDocValidator, storageService);

            var request = new AddStoredCaseDocumentRequest
            {
                StoredFileId = Guid.NewGuid()
            };

            // Act
            var result = await service.AddStoredDocumentAsync(caseId, request);

            // Assert
            Assert.False(result.Success);
            Assert.Equal(404, result.StatusCode);
        }
    }

    [Fact]
    public async Task AddStoredDocumentAsync_AlreadyAttachedStoredFile_Returns400()
    {
        // Arrange
        var options = CreateSQLiteOptions();
        var caseId = Guid.NewGuid();
        var ownerId = Guid.NewGuid();
        var storedFileId = Guid.NewGuid();

        using (var db = new ApplicationDbContext(options))
        {
            db.Database.EnsureCreated();
            var user = new ApplicationUser { Id = ownerId, UserName = "owner", Email = "owner@test.com", FullName = "Owner" };
            var clientProfile = new ClientProfile { UserId = ownerId, User = user };
            db.Users.Add(user);
            db.ClientProfile.Add(clientProfile);

            var existingCase = new SmartCourt.Entities.Case
            {
                Id = caseId,
                Title = "Case 1",
                Description = "Desc",
                ClientId = ownerId,
                Status = CaseStatus.Draft
            };
            db.Cases.Add(existingCase);

            var storedFile = new StoredFile
            {
                Id = storedFileId,
                StoredFileName = $"{storedFileId}.pdf",
                OriginalFileName = "document.pdf",
                ContentType = "application/pdf",
                Extension = ".pdf",
                SizeInBytes = 1024,
                FileUrl = $"uploads/{storedFileId}.pdf"
            };
            db.StoredFiles.Add(storedFile);

            db.CaseDocuments.Add(new CaseDocument
            {
                Id = Guid.NewGuid(),
                CaseId = caseId,
                StoredFileId = storedFileId
            });

            await db.SaveChangesAsync();
        }

        using (var db = new ApplicationDbContext(options))
        {
            var httpContextAccessor = CreateHttpContextAccessor(ownerId);
            var validator = new AddCaseDocumentRequestValidator();
            var storedDocValidator = new AddStoredCaseDocumentRequestValidator();
            var storageService = new TestFileStorageService();
            var service = new AddCaseDocumentService(db, httpContextAccessor, validator, storedDocValidator, storageService);

            var request = new AddStoredCaseDocumentRequest
            {
                StoredFileId = storedFileId
            };

            // Act
            var result = await service.AddStoredDocumentAsync(caseId, request);

            // Assert
            Assert.False(result.Success);
            Assert.Equal(400, result.StatusCode);
            Assert.NotNull(result.Errors);
            Assert.Contains(result.Errors, e => e.Contains("already attached"));
        }
    }

    [Fact]
    public async Task AddStoredDocumentAsync_ValidRequest_ByCaseOwner_SuccessfullyAttachesDocument()
    {
        // Arrange
        var options = CreateSQLiteOptions();
        var caseId = Guid.NewGuid();
        var ownerId = Guid.NewGuid();
        var storedFileId = Guid.NewGuid();

        using (var db = new ApplicationDbContext(options))
        {
            db.Database.EnsureCreated();
            var user = new ApplicationUser { Id = ownerId, UserName = "owner", Email = "owner@test.com", FullName = "Owner" };
            var clientProfile = new ClientProfile { UserId = ownerId, User = user };
            db.Users.Add(user);
            db.ClientProfile.Add(clientProfile);

            var existingCase = new SmartCourt.Entities.Case
            {
                Id = caseId,
                Title = "Case 1",
                Description = "Desc",
                ClientId = ownerId,
                Status = CaseStatus.Draft
            };
            db.Cases.Add(existingCase);

            var storedFile = new StoredFile
            {
                Id = storedFileId,
                StoredFileName = $"{storedFileId}.pdf",
                OriginalFileName = "legal_doc.pdf",
                ContentType = "application/pdf",
                Extension = ".pdf",
                SizeInBytes = 2048,
                FileUrl = $"uploads/{storedFileId}.pdf"
            };
            db.StoredFiles.Add(storedFile);

            await db.SaveChangesAsync();
        }

        using (var db = new ApplicationDbContext(options))
        {
            var httpContextAccessor = CreateHttpContextAccessor(ownerId);
            var validator = new AddCaseDocumentRequestValidator();
            var storedDocValidator = new AddStoredCaseDocumentRequestValidator();
            var storageService = new TestFileStorageService();
            var service = new AddCaseDocumentService(db, httpContextAccessor, validator, storedDocValidator, storageService);

            var request = new AddStoredCaseDocumentRequest
            {
                StoredFileId = storedFileId
            };

            // Act
            var result = await service.AddStoredDocumentAsync(caseId, request);

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal(storedFileId, result.Data.StoredFileId);
            Assert.Equal("legal_doc.pdf", result.Data.FileName);
            Assert.Equal($"uploads/{storedFileId}.pdf", result.Data.FileUrl);
            Assert.Equal(2048, result.Data.SizeInBytes);

            // Verify stored in DB
            var dbCaseDoc = await db.CaseDocuments.FirstOrDefaultAsync(cd => cd.CaseId == caseId && cd.StoredFileId == storedFileId);
            Assert.NotNull(dbCaseDoc);
        }
    }

    [Fact]
    public async Task AddStoredDocumentAsync_ValidRequest_ByAssignedLawyer_SuccessfullyAttachesDocument()
    {
        // Arrange
        var options = CreateSQLiteOptions();
        var caseId = Guid.NewGuid();
        var ownerId = Guid.NewGuid();
        var lawyerId = Guid.NewGuid();
        var storedFileId = Guid.NewGuid();

        using (var db = new ApplicationDbContext(options))
        {
            db.Database.EnsureCreated();
            var clientUser = new ApplicationUser { Id = ownerId, UserName = "owner", Email = "owner@test.com", FullName = "Owner" };
            var lawyerUser = new ApplicationUser { Id = lawyerId, UserName = "lawyer", Email = "lawyer@test.com", FullName = "Lawyer" };
            var clientProfile = new ClientProfile { UserId = ownerId, User = clientUser };
            var lawyerProfile = new LawyerProfile { UserId = lawyerId, User = lawyerUser };
            db.Users.AddRange(clientUser, lawyerUser);
            db.ClientProfile.Add(clientProfile);
            db.LawyerProfiles.Add(lawyerProfile);

            var existingCase = new SmartCourt.Entities.Case
            {
                Id = caseId,
                Title = "Case 1",
                Description = "Desc",
                ClientId = ownerId,
                LawyerId = lawyerId,
                Status = CaseStatus.Assigned
            };
            db.Cases.Add(existingCase);

            var storedFile = new StoredFile
            {
                Id = storedFileId,
                StoredFileName = $"{storedFileId}.docx",
                OriginalFileName = "memo.docx",
                ContentType = "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                Extension = ".docx",
                SizeInBytes = 4096,
                FileUrl = $"uploads/{storedFileId}.docx"
            };
            db.StoredFiles.Add(storedFile);

            await db.SaveChangesAsync();
        }

        using (var db = new ApplicationDbContext(options))
        {
            var httpContextAccessor = CreateHttpContextAccessor(lawyerId, roles: new[] { "Lawyer" });
            var validator = new AddCaseDocumentRequestValidator();
            var storedDocValidator = new AddStoredCaseDocumentRequestValidator();
            var storageService = new TestFileStorageService();
            var service = new AddCaseDocumentService(db, httpContextAccessor, validator, storedDocValidator, storageService);

            var request = new AddStoredCaseDocumentRequest
            {
                StoredFileId = storedFileId
            };

            // Act
            var result = await service.AddStoredDocumentAsync(caseId, request);

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal(storedFileId, result.Data.StoredFileId);
            Assert.Equal("memo.docx", result.Data.FileName);

            // Verify stored in DB
            var dbCaseDoc = await db.CaseDocuments.FirstOrDefaultAsync(cd => cd.CaseId == caseId && cd.StoredFileId == storedFileId);
            Assert.NotNull(dbCaseDoc);
        }
    }

    // --- DeleteDocumentAsync Tests ---

    [Fact]
    public async Task DeleteDocumentAsync_UnauthenticatedUser_Returns401()
    {
        // Arrange
        var options = CreateSQLiteOptions();
        using var db = new ApplicationDbContext(options);
        db.Database.EnsureCreated();

        var httpContextAccessor = CreateHttpContextAccessor(userId: null);
        var validator = new AddCaseDocumentRequestValidator();
        var storedDocValidator = new AddStoredCaseDocumentRequestValidator();
        var storageService = new TestFileStorageService();
        var service = new AddCaseDocumentService(db, httpContextAccessor, validator, storedDocValidator, storageService);

        // Act
        var result = await service.DeleteDocumentAsync(Guid.NewGuid(), Guid.NewGuid());

        // Assert
        Assert.False(result.Success);
        Assert.Equal(401, result.StatusCode);
    }

    [Fact]
    public async Task DeleteDocumentAsync_CaseNotFound_Returns404()
    {
        // Arrange
        var options = CreateSQLiteOptions();
        using var db = new ApplicationDbContext(options);
        db.Database.EnsureCreated();

        var userId = Guid.NewGuid();
        var httpContextAccessor = CreateHttpContextAccessor(userId);
        var validator = new AddCaseDocumentRequestValidator();
        var storedDocValidator = new AddStoredCaseDocumentRequestValidator();
        var storageService = new TestFileStorageService();
        var service = new AddCaseDocumentService(db, httpContextAccessor, validator, storedDocValidator, storageService);

        // Act
        var result = await service.DeleteDocumentAsync(Guid.NewGuid(), Guid.NewGuid());

        // Assert
        Assert.False(result.Success);
        Assert.Equal(404, result.StatusCode);
    }

    [Fact]
    public async Task DeleteDocumentAsync_UnauthorizedUser_Returns403()
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
            var storedDocValidator = new AddStoredCaseDocumentRequestValidator();
            var storageService = new TestFileStorageService();
            var service = new AddCaseDocumentService(db, httpContextAccessor, validator, storedDocValidator, storageService);

            // Act
            var result = await service.DeleteDocumentAsync(caseId, Guid.NewGuid());

            // Assert
            Assert.False(result.Success);
            Assert.Equal(403, result.StatusCode);
        }
    }

    [Fact]
    public async Task DeleteDocumentAsync_DocumentNotFound_Returns404()
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
            var storedDocValidator = new AddStoredCaseDocumentRequestValidator();
            var storageService = new TestFileStorageService();
            var service = new AddCaseDocumentService(db, httpContextAccessor, validator, storedDocValidator, storageService);

            // Act
            var result = await service.DeleteDocumentAsync(caseId, Guid.NewGuid());

            // Assert
            Assert.False(result.Success);
            Assert.Equal(404, result.StatusCode);
        }
    }

    [Fact]
    public async Task DeleteDocumentAsync_ValidRequest_ByCaseOwner_SuccessfullyDeletesDocument()
    {
        // Arrange
        var options = CreateSQLiteOptions();
        var caseId = Guid.NewGuid();
        var ownerId = Guid.NewGuid();
        var storedFileId = Guid.NewGuid();
        var caseDocId = Guid.NewGuid();

        using (var db = new ApplicationDbContext(options))
        {
            db.Database.EnsureCreated();
            var user = new ApplicationUser { Id = ownerId, UserName = "owner", Email = "owner@test.com", FullName = "Owner" };
            var clientProfile = new ClientProfile { UserId = ownerId, User = user };
            db.Users.Add(user);
            db.ClientProfile.Add(clientProfile);

            var existingCase = new SmartCourt.Entities.Case
            {
                Id = caseId,
                Title = "Case 1",
                Description = "Desc",
                ClientId = ownerId,
                Status = CaseStatus.Draft
            };
            db.Cases.Add(existingCase);

            var storedFile = new StoredFile
            {
                Id = storedFileId,
                StoredFileName = $"{storedFileId}.pdf",
                OriginalFileName = "legal_doc.pdf",
                ContentType = "application/pdf",
                Extension = ".pdf",
                SizeInBytes = 2048,
                FileUrl = $"uploads/{storedFileId}.pdf"
            };
            db.StoredFiles.Add(storedFile);

            db.CaseDocuments.Add(new CaseDocument
            {
                Id = caseDocId,
                CaseId = caseId,
                StoredFileId = storedFileId
            });

            await db.SaveChangesAsync();
        }

        using (var db = new ApplicationDbContext(options))
        {
            var httpContextAccessor = CreateHttpContextAccessor(ownerId);
            var validator = new AddCaseDocumentRequestValidator();
            var storedDocValidator = new AddStoredCaseDocumentRequestValidator();
            var storageService = new TestFileStorageService();
            var service = new AddCaseDocumentService(db, httpContextAccessor, validator, storedDocValidator, storageService);

            // Act - delete using StoredFileId
            var result = await service.DeleteDocumentAsync(caseId, storedFileId);

            // Assert
            Assert.True(result.Success);

            // Verify removed from DB
            var dbCaseDoc = await db.CaseDocuments.FirstOrDefaultAsync(cd => cd.CaseId == caseId && cd.StoredFileId == storedFileId);
            Assert.Null(dbCaseDoc);
        }
    }

    [Fact]
    public async Task DeleteDocumentAsync_ValidRequest_ByAssignedLawyer_SuccessfullyDeletesDocument()
    {
        // Arrange
        var options = CreateSQLiteOptions();
        var caseId = Guid.NewGuid();
        var ownerId = Guid.NewGuid();
        var lawyerId = Guid.NewGuid();
        var storedFileId = Guid.NewGuid();
        var caseDocId = Guid.NewGuid();

        using (var db = new ApplicationDbContext(options))
        {
            db.Database.EnsureCreated();
            var clientUser = new ApplicationUser { Id = ownerId, UserName = "owner", Email = "owner@test.com", FullName = "Owner" };
            var lawyerUser = new ApplicationUser { Id = lawyerId, UserName = "lawyer", Email = "lawyer@test.com", FullName = "Lawyer" };
            var clientProfile = new ClientProfile { UserId = ownerId, User = clientUser };
            var lawyerProfile = new LawyerProfile { UserId = lawyerId, User = lawyerUser };
            db.Users.AddRange(clientUser, lawyerUser);
            db.ClientProfile.Add(clientProfile);
            db.LawyerProfiles.Add(lawyerProfile);

            var existingCase = new SmartCourt.Entities.Case
            {
                Id = caseId,
                Title = "Case 1",
                Description = "Desc",
                ClientId = ownerId,
                LawyerId = lawyerId,
                Status = CaseStatus.Assigned
            };
            db.Cases.Add(existingCase);

            var storedFile = new StoredFile
            {
                Id = storedFileId,
                StoredFileName = $"{storedFileId}.docx",
                OriginalFileName = "memo.docx",
                ContentType = "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                Extension = ".docx",
                SizeInBytes = 4096,
                FileUrl = $"uploads/{storedFileId}.docx"
            };
            db.StoredFiles.Add(storedFile);

            db.CaseDocuments.Add(new CaseDocument
            {
                Id = caseDocId,
                CaseId = caseId,
                StoredFileId = storedFileId
            });

            await db.SaveChangesAsync();
        }

        using (var db = new ApplicationDbContext(options))
        {
            var httpContextAccessor = CreateHttpContextAccessor(lawyerId, roles: new[] { "Lawyer" });
            var validator = new AddCaseDocumentRequestValidator();
            var storedDocValidator = new AddStoredCaseDocumentRequestValidator();
            var storageService = new TestFileStorageService();
            var service = new AddCaseDocumentService(db, httpContextAccessor, validator, storedDocValidator, storageService);

            // Act - delete using CaseDocument Id
            var result = await service.DeleteDocumentAsync(caseId, caseDocId);

            // Assert
            Assert.True(result.Success);

            // Verify removed from DB
            var dbCaseDoc = await db.CaseDocuments.FirstOrDefaultAsync(cd => cd.CaseId == caseId && cd.Id == caseDocId);
            Assert.Null(dbCaseDoc);
        }
    }
}
