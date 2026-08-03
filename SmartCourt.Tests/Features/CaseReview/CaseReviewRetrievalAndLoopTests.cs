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
using SmartCourt.Features.Case.UpdateCase;
using SmartCourt.Features.Case.UpdateCase.DTOs;
using SmartCourt.Features.CaseReview;
using SmartCourt.Interfaces;
using SmartCourt.Interfaces.Providers;
using SmartCourt.Persistence;
using Xunit;
using CaseEntity = SmartCourt.Entities.Case;

namespace SmartCourt.Tests.Features.CaseReview;

public sealed class CaseReviewRetrievalAndLoopTests
{
    private static DbContextOptions<ApplicationDbContext> CreateInMemoryOptions()
    {
        return new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
    }

    [Fact]
    public async Task GetReviewReports_ReturnsAllOrderedByCreatedAtDesc()
    {
        // Arrange
        var dbOptions = CreateInMemoryOptions();
        await using var dbContext = new ApplicationDbContext(dbOptions);

        var userId = Guid.NewGuid();
        var caseId = Guid.NewGuid();

        var caseEntity = new CaseEntity
        {
            Id = caseId,
            Title = "نزاع ملكية",
            Description = "نزاع حول قطعة أرض",
            ClientId = userId,
            Status = CaseStatus.Reviewed
        };
        dbContext.Cases.Add(caseEntity);

        var report1 = new CaseReviewReport
        {
            Id = Guid.NewGuid(),
            CaseId = caseId,
            IsLatest = false,
            CreatedAt = DateTime.UtcNow.AddHours(-2)
        };
        var report2 = new CaseReviewReport
        {
            Id = Guid.NewGuid(),
            CaseId = caseId,
            IsLatest = true,
            CreatedAt = DateTime.UtcNow.AddHours(-1)
        };

        dbContext.CaseReviewReports.AddRange(report1, report2);
        await dbContext.SaveChangesAsync();

        // Explicitly set distinct CreatedAt timestamps to test ordering
        report1.CreatedAt = DateTime.UtcNow.AddMinutes(-10);
        report2.CreatedAt = DateTime.UtcNow;
        await dbContext.SaveChangesAsync();

        var currentUserService = new TestCurrentUserService { UserId = userId };
        var service = new CaseReviewService(
            dbContext,
            currentUserService,
            new TestChatModelProvider(),
            NullLogger<CaseReviewService>.Instance);

        // Act
        var reports = await service.GetReviewReportsAsync(caseId);

        // Assert
        Assert.Equal(2, reports.Count);
        Assert.Equal(report2.Id, reports[0].Id);
        Assert.Equal(report1.Id, reports[1].Id);
    }

    [Fact]
    public async Task GetReviewReports_NonOwner_ThrowsForbiddenAccessException()
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
            Title = "دعوى إخلاء",
            Description = "عدم سداد الإيجار",
            ClientId = ownerId,
            Status = CaseStatus.Reviewed
        };
        dbContext.Cases.Add(caseEntity);
        await dbContext.SaveChangesAsync();

        var currentUserService = new TestCurrentUserService { UserId = callerId };
        var service = new CaseReviewService(
            dbContext,
            currentUserService,
            new TestChatModelProvider(),
            NullLogger<CaseReviewService>.Instance);

        // Act & Assert
        await Assert.ThrowsAsync<ForbiddenAccessException>(() => service.GetReviewReportsAsync(caseId));
    }

    [Fact]
    public async Task GetLatestReviewReport_ReturnsOnlyIsLatestReport()
    {
        // Arrange
        var dbOptions = CreateInMemoryOptions();
        await using var dbContext = new ApplicationDbContext(dbOptions);

        var userId = Guid.NewGuid();
        var caseId = Guid.NewGuid();

        var caseEntity = new CaseEntity
        {
            Id = caseId,
            Title = "نزاع شركة",
            Description = "تصفية شراكة تجارية",
            ClientId = userId,
            Status = CaseStatus.Reviewed
        };
        dbContext.Cases.Add(caseEntity);

        var oldReport = new CaseReviewReport
        {
            Id = Guid.NewGuid(),
            CaseId = caseId,
            IsLatest = false,
            CreatedAt = DateTime.UtcNow.AddHours(-3)
        };
        var latestReport = new CaseReviewReport
        {
            Id = Guid.NewGuid(),
            CaseId = caseId,
            IsLatest = true,
            CreatedAt = DateTime.UtcNow.AddHours(-1)
        };

        dbContext.CaseReviewReports.AddRange(oldReport, latestReport);
        await dbContext.SaveChangesAsync();

        var currentUserService = new TestCurrentUserService { UserId = userId };
        var service = new CaseReviewService(
            dbContext,
            currentUserService,
            new TestChatModelProvider(),
            NullLogger<CaseReviewService>.Instance);

        // Act
        var result = await service.GetLatestReviewReportAsync(caseId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(latestReport.Id, result.Id);
        Assert.True(result.IsLatest);
    }

    [Fact]
    public async Task GetLatestReviewReport_NoReportsExist_ThrowsNotFoundException()
    {
        // Arrange
        var dbOptions = CreateInMemoryOptions();
        await using var dbContext = new ApplicationDbContext(dbOptions);

        var userId = Guid.NewGuid();
        var caseId = Guid.NewGuid();

        var caseEntity = new CaseEntity
        {
            Id = caseId,
            Title = "قضية بدون مراجعات",
            Description = "وصف قضية جديدة",
            ClientId = userId,
            Status = CaseStatus.Submitted
        };
        dbContext.Cases.Add(caseEntity);
        await dbContext.SaveChangesAsync();

        var currentUserService = new TestCurrentUserService { UserId = userId };
        var service = new CaseReviewService(
            dbContext,
            currentUserService,
            new TestChatModelProvider(),
            NullLogger<CaseReviewService>.Instance);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => service.GetLatestReviewReportAsync(caseId));
    }

    [Fact]
    public async Task FullReReviewLoop_Create_Submit_Review_Edit_Resubmit_ReReview()
    {
        // Arrange
        var dbOptions = CreateInMemoryOptions();
        await using var dbContext = new ApplicationDbContext(dbOptions);

        var userId = Guid.NewGuid();
        var caseId = Guid.NewGuid();

        // 1. Create case in Submitted status
        var caseEntity = new CaseEntity
        {
            Id = caseId,
            Title = "عقد بيع ابتدائي",
            Description = "تأخر المشتري في السداد",
            ClientId = userId,
            Status = CaseStatus.Submitted
        };
        dbContext.Cases.Add(caseEntity);
        await dbContext.SaveChangesAsync();

        var currentUserService = new TestCurrentUserService { UserId = userId };
        var chatModelProvider = new TestChatModelProvider
        {
            OutputToReturn = """[{"type":"Weakness","description":"نقص المستندات"}]"""
        };

        var reviewService = new CaseReviewService(
            dbContext,
            currentUserService,
            chatModelProvider,
            NullLogger<CaseReviewService>.Instance);

        // 2. First AI Review -> Case transitions Submitted -> Reviewed
        var report1 = await reviewService.CreateReviewReportAsync(caseId);
        Assert.True(report1.IsLatest);

        var caseAfterReview = await dbContext.Cases.FindAsync(caseId);
        Assert.NotNull(caseAfterReview);
        Assert.Equal(CaseStatus.Reviewed, caseAfterReview.Status);

        // 3. Edit Case while in Reviewed status -> Case status reverts Reviewed -> Submitted
        caseAfterReview.Description = "تحديث: تم إضافة إثبات الإنذار الرسمي";
        SmartCourt.Features.Case.BusinessRules.CaseStatusTransitionGuard.EnsureCanTransition(
            caseAfterReview.Status,
            CaseStatus.Submitted);
        caseAfterReview.Status = CaseStatus.Submitted;
        await dbContext.SaveChangesAsync();

        var caseAfterEdit = await dbContext.Cases.FindAsync(caseId);
        Assert.NotNull(caseAfterEdit);
        Assert.Equal(CaseStatus.Submitted, caseAfterEdit.Status);

        // 4. Second AI Review -> Case transitions Submitted -> Reviewed again
        chatModelProvider.OutputToReturn = """[{"type":"Strength","description":"تم إضافة الإنذار بنجاح"}]""";
        var report2 = await reviewService.CreateReviewReportAsync(caseId);

        // Assert
        Assert.True(report2.IsLatest);
        Assert.NotEqual(report1.Id, report2.Id);

        var allReports = await reviewService.GetReviewReportsAsync(caseId);
        Assert.Equal(2, allReports.Count);
        Assert.Equal(report2.Id, allReports[0].Id);
        Assert.True(allReports[0].IsLatest);
        Assert.False(allReports[1].IsLatest);

        var latestReport = await reviewService.GetLatestReviewReportAsync(caseId);
        Assert.Equal(report2.Id, latestReport.Id);
    }

    private sealed class TestCurrentUserService : ICurrentUserService
    {
        public Guid? UserId { get; set; }
        public bool IsAuthenticated => UserId.HasValue;
    }

    private sealed class TestChatModelProvider : IChatModelProvider
    {
        public string OutputToReturn { get; set; } = """[{"type":"Suggestion","description":"ملاحظة أصلية"}]""";

        public Task<string> GenerateAsync(string systemPrompt, string userPrompt, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(OutputToReturn);
        }
    }
}
