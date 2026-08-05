using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using SmartCourt.Common.Entities;
using SmartCourt.Common.Enums;
using SmartCourt.Common.Exceptions;
using SmartCourt.Entities;
using SmartCourt.Features.CaseReview;
using SmartCourt.Interfaces;
using SmartCourt.Interfaces.Providers;
using SmartCourt.Persistence;
using Xunit;
using CaseEntity = SmartCourt.Entities.Case;

namespace SmartCourt.Tests.Features.CaseReview;

public sealed class CaseReviewServiceTests
{
    private static DbContextOptions<ApplicationDbContext> CreateInMemoryOptions()
    {
        return new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
    }

    [Fact]
    public async Task CreateReviewReport_HappyPath_TransitionsToReviewedAndCreatesReport()
    {
        // Arrange
        var dbOptions = CreateInMemoryOptions();
        await using var dbContext = new ApplicationDbContext(dbOptions);

        var userId = Guid.NewGuid();
        var clientProfile = new ClientProfile { UserId = userId };
        dbContext.ClientProfile.Add(clientProfile);

        var caseId = Guid.NewGuid();
        var caseEntity = new CaseEntity
        {
            Id = caseId,
            Title = "تأخر في تسليم العقار",
            Description = "تم الاتفاق على التسليم ولكن تم التأخير بدون عذر",
            ClientId = userId,
            ClientProfile = clientProfile,
            Status = CaseStatus.Submitted
        };
        dbContext.Cases.Add(caseEntity);
        await dbContext.SaveChangesAsync();

        var currentUserService = new TestCurrentUserService { UserId = userId };
        var chatModelProvider = new TestChatModelProvider
        {
            OutputToReturn = """
                [
                  { "type": "Strength", "description": "وجود عقد مكتوب يوضح المواعيد" },
                  { "type": "Weakness", "description": "عدم وجود إثبات بالإنذار الرسمي" },
                  { "type": "Suggestion", "description": "توجيه إنذار رسمي على يد محضر" }
                ]
                """
        };

        var service = new CaseReviewService(
            dbContext,
            currentUserService,
            chatModelProvider,
            NullLogger<CaseReviewService>.Instance);

        // Act
        var result = await service.CreateReviewReportAsync(caseId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(caseId, result.CaseId);
        Assert.True(result.IsLatest);
        Assert.Equal(3, result.ReviewPoints.Count);

        var updatedCase = await dbContext.Cases.FindAsync(caseId);
        Assert.NotNull(updatedCase);
        Assert.Equal(CaseStatus.Reviewed, updatedCase.Status);

        var reportInDb = await dbContext.CaseReviewReports
            .Include(r => r.ReviewPoints)
            .FirstOrDefaultAsync(r => r.Id == result.Id);
        Assert.NotNull(reportInDb);
        Assert.True(reportInDb.IsLatest);
        Assert.Equal(3, reportInDb.ReviewPoints.Count);
    }

    [Fact]
    public async Task CreateReviewReport_NonOwner_ThrowsForbiddenAccessException()
    {
        // Arrange
        var dbOptions = CreateInMemoryOptions();
        await using var dbContext = new ApplicationDbContext(dbOptions);

        var ownerId = Guid.NewGuid();
        var callerId = Guid.NewGuid();

        var caseId = Guid.NewGuid();
        var caseEntity = new CaseEntity
        {
            Id = caseId,
            Title = "قضية عمالية",
            Description = "المطالبة بمستحقات نهاية الخدمة",
            ClientId = ownerId,
            Status = CaseStatus.Submitted
        };
        dbContext.Cases.Add(caseEntity);
        await dbContext.SaveChangesAsync();

        var currentUserService = new TestCurrentUserService { UserId = callerId };
        var chatModelProvider = new TestChatModelProvider();

        var service = new CaseReviewService(
            dbContext,
            currentUserService,
            chatModelProvider,
            NullLogger<CaseReviewService>.Instance);

        // Act & Assert
        await Assert.ThrowsAsync<ForbiddenAccessException>(() => service.CreateReviewReportAsync(caseId));
    }

    [Fact]
    public async Task CreateReviewReport_InvalidStatus_ThrowsBusinessException()
    {
        // Arrange
        var dbOptions = CreateInMemoryOptions();
        await using var dbContext = new ApplicationDbContext(dbOptions);

        var userId = Guid.NewGuid();

        var caseId = Guid.NewGuid();
        var caseEntity = new CaseEntity
        {
            Id = caseId,
            Title = "قضية تجارية",
            Description = "نزاع حول بنود العقد",
            ClientId = userId,
            Status = CaseStatus.Draft // Draft cannot transition directly to Reviewed
        };
        dbContext.Cases.Add(caseEntity);
        await dbContext.SaveChangesAsync();

        var currentUserService = new TestCurrentUserService { UserId = userId };
        var chatModelProvider = new TestChatModelProvider();

        var service = new CaseReviewService(
            dbContext,
            currentUserService,
            chatModelProvider,
            NullLogger<CaseReviewService>.Instance);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<BusinessException>(() => service.CreateReviewReportAsync(caseId));
        Assert.Contains("Invalid case status transition", ex.Message);
    }

    [Fact]
    public async Task CreateReviewReport_IsLatestFlagManagement_SetsPreviousReportToFalse()
    {
        // Arrange
        var dbOptions = CreateInMemoryOptions();
        await using var dbContext = new ApplicationDbContext(dbOptions);

        var userId = Guid.NewGuid();

        var caseId = Guid.NewGuid();
        var caseEntity = new CaseEntity
        {
            Id = caseId,
            Title = "دعوى تعويض",
            Description = "مطالبة بالتعويض عن أضرار الحادث",
            ClientId = userId,
            Status = CaseStatus.Submitted
        };
        dbContext.Cases.Add(caseEntity);

        var oldReport = new CaseReviewReport
        {
            Id = Guid.NewGuid(),
            CaseId = caseId,
            IsLatest = true,
            CreatedAt = DateTime.UtcNow.AddHours(-1)
        };
        dbContext.CaseReviewReports.Add(oldReport);

        await dbContext.SaveChangesAsync();

        var currentUserService = new TestCurrentUserService { UserId = userId };
        var chatModelProvider = new TestChatModelProvider
        {
            OutputToReturn = """[{"type":"Suggestion","description":"تحديث البيانات"}]"""
        };

        var service = new CaseReviewService(
            dbContext,
            currentUserService,
            chatModelProvider,
            NullLogger<CaseReviewService>.Instance);

        // Act
        var result = await service.CreateReviewReportAsync(caseId);

        // Assert
        var oldReportInDb = await dbContext.CaseReviewReports.FindAsync(oldReport.Id);
        Assert.NotNull(oldReportInDb);
        Assert.False(oldReportInDb.IsLatest);

        var newReportInDb = await dbContext.CaseReviewReports.FindAsync(result.Id);
        Assert.NotNull(newReportInDb);
        Assert.True(newReportInDb.IsLatest);
    }

    [Fact]
    public async Task CreateReviewReport_AiFailure_PropagatesExceptionAndLeavesCaseSubmitted()
    {
        // Arrange
        var dbOptions = CreateInMemoryOptions();
        await using var dbContext = new ApplicationDbContext(dbOptions);

        var userId = Guid.NewGuid();

        var caseId = Guid.NewGuid();
        var caseEntity = new CaseEntity
        {
            Id = caseId,
            Title = "قضية إيجارات",
            Description = "المطالبة بفسخ عقد الإيجار",
            ClientId = userId,
            Status = CaseStatus.Submitted
        };
        dbContext.Cases.Add(caseEntity);
        await dbContext.SaveChangesAsync();

        var currentUserService = new TestCurrentUserService { UserId = userId };
        var chatModelProvider = new TestChatModelProvider
        {
            ExceptionToThrow = new InvalidOperationException("AI Provider Timeout")
        };

        var service = new CaseReviewService(
            dbContext,
            currentUserService,
            chatModelProvider,
            NullLogger<CaseReviewService>.Instance);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => service.CreateReviewReportAsync(caseId));

        var caseInDb = await dbContext.Cases.FindAsync(caseId);
        Assert.NotNull(caseInDb);
        Assert.Equal(CaseStatus.Submitted, caseInDb.Status);

        var reportsCount = await dbContext.CaseReviewReports.CountAsync(r => r.CaseId == caseId);
        Assert.Equal(0, reportsCount);
    }

    private sealed class TestCurrentUserService : ICurrentUserService
    {
        public Guid? UserId { get; set; }
        public bool IsAuthenticated => UserId.HasValue;
    }

    private sealed class TestChatModelProvider : IChatModelProvider
    {
        public string OutputToReturn { get; set; } = """[{"type":"Suggestion","description":"ملاحظة أصلية"}]""";
        public Exception? ExceptionToThrow { get; set; }

        public Task<string> GenerateAsync(string systemPrompt, string userPrompt, CancellationToken cancellationToken = default)
        {
            if (ExceptionToThrow != null)
            {
                throw ExceptionToThrow;
            }
            return Task.FromResult(OutputToReturn);
        }
    }
}
