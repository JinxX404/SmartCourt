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
using SmartCourt.Features.Case.UpdateCase;
using SmartCourt.Features.Case.UpdateCase.DTOs;
using SmartCourt.Interfaces.Providers;
using SmartCourt.Persistence;
using Xunit;

namespace SmartCourt.Tests.Features.Case;

public class UpdateCaseHandlerTests
{
    private static DbContextOptions<ApplicationDbContext> CreateSQLiteOptions()
    {
        return new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlite($"Data Source={Guid.NewGuid()}.db")
            .Options;
    }

    private class TestFileStorageService : IFileStorageService
    {
        public Task<FileUploadResult> UploadAsync(Stream stream, string filePath, string originalFileName, CancellationToken cancellationToken = default)
            => Task.FromResult(new FileUploadResult { StoragePath = filePath, OriginalFileName = originalFileName, Size = stream.Length });

        public Task<FileUploadResult> UploadAsync(Stream stream, string filePath, string originalFileName, string? contentType, CancellationToken cancellationToken = default)
            => Task.FromResult(new FileUploadResult { StoragePath = filePath, OriginalFileName = originalFileName, Size = stream.Length });

        public Task DeleteAsync(string filePath, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task<byte[]> DownloadAsync(string filePath, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task<bool> ExistsAsync(string filePath, CancellationToken cancellationToken = default) => Task.FromResult(true);
        public Task<string> GetDownloadUrlAsync(string filePath, CancellationToken cancellationToken = default) => Task.FromResult(filePath);
    }

    private static IHttpContextAccessor CreateHttpContextAccessor(Guid? userId = null)
    {
        var context = new DefaultHttpContext();
        if (userId.HasValue)
        {
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, userId.Value.ToString())
            };
            var identity = new ClaimsIdentity(claims, "TestAuth");
            context.User = new ClaimsPrincipal(identity);
        }

        return new HttpContextAccessor { HttpContext = context };
    }

    [Fact]
    public async Task Handle_WhenCaseIsAssigned_Returns400()
    {
        // Arrange
        var options = CreateSQLiteOptions();
        var caseId = Guid.NewGuid();
        var clientId = Guid.NewGuid();
        var lawyerId = Guid.NewGuid();

        using (var db = new ApplicationDbContext(options))
        {
            db.Database.EnsureCreated();
            var clientUser = new ApplicationUser { Id = clientId, UserName = "client", Email = "client@test.com", FullName = "Client" };
            var lawyerUser = new ApplicationUser { Id = lawyerId, UserName = "lawyer", Email = "lawyer@test.com", FullName = "Lawyer" };
            var clientProfile = new ClientProfile { UserId = clientId, User = clientUser };
            var lawyerProfile = new LawyerProfile { UserId = lawyerId, User = lawyerUser };
            db.Users.AddRange(clientUser, lawyerUser);
            db.ClientProfile.Add(clientProfile);
            db.LawyerProfiles.Add(lawyerProfile);

            var existingCase = new SmartCourt.Entities.Case
            {
                Id = caseId,
                Title = "Old Title",
                Description = "Old Desc",
                ClientId = clientId,
                LawyerId = lawyerId,
                Status = CaseStatus.Assigned
            };
            db.Cases.Add(existingCase);
            await db.SaveChangesAsync();
        }

        using (var db = new ApplicationDbContext(options))
        {
            var httpContextAccessor = CreateHttpContextAccessor(clientId);
            var validator = new UpdateCaseCommandValidator();
            var storageService = new TestFileStorageService();
            var handler = new UpdateCaseHandler(db, httpContextAccessor, validator, storageService);

            var command = new UpdateCaseCommand
            {
                CaseId = caseId,
                Title = "New Title",
                Description = "New Desc"
            };

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.False(result.Success);
            Assert.Equal(400, result.StatusCode);
            Assert.NotNull(result.Errors);
            Assert.Contains(result.Errors, e => e.Contains("Cannot update a case that has already been assigned"));
        }
    }

    [Fact]
    public async Task Handle_WhenCaseIsSubmitted_SuccessfullyUpdatesCase()
    {
        // Arrange
        var options = CreateSQLiteOptions();
        var caseId = Guid.NewGuid();
        var clientId = Guid.NewGuid();

        using (var db = new ApplicationDbContext(options))
        {
            db.Database.EnsureCreated();
            var clientUser = new ApplicationUser { Id = clientId, UserName = "client", Email = "client@test.com", FullName = "Client" };
            var clientProfile = new ClientProfile { UserId = clientId, User = clientUser };
            db.Users.Add(clientUser);
            db.ClientProfile.Add(clientProfile);

            var existingCase = new SmartCourt.Entities.Case
            {
                Id = caseId,
                Title = "Old Title",
                Description = "Old Desc",
                ClientId = clientId,
                Status = CaseStatus.Submitted
            };
            db.Cases.Add(existingCase);
            await db.SaveChangesAsync();
        }

        using (var db = new ApplicationDbContext(options))
        {
            var httpContextAccessor = CreateHttpContextAccessor(clientId);
            var validator = new UpdateCaseCommandValidator();
            var storageService = new TestFileStorageService();
            var handler = new UpdateCaseHandler(db, httpContextAccessor, validator, storageService);

            var command = new UpdateCaseCommand
            {
                CaseId = caseId,
                Title = "New Title",
                Description = "New Desc"
            };

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.True(result.Success);
            var updated = await db.Cases.FindAsync(caseId);
            Assert.NotNull(updated);
            Assert.Equal("New Title", updated.Title);
            Assert.Equal("New Desc", updated.Description);
        }
    }
}
