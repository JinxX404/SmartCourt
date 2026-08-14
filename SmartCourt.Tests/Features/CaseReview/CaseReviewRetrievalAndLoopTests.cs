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
    public async Task GetReviewReport_ReturnsRequestedReportById()
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
        var resultOld = await service.GetReviewReportAsync(caseId, oldReport.Id);
        var resultLatest = await service.GetReviewReportAsync(caseId, latestReport.Id);

        // Assert
        Assert.NotNull(resultOld);
        Assert.Equal(oldReport.Id, resultOld.Id);
        Assert.False(resultOld.IsLatest);

        Assert.NotNull(resultLatest);
        Assert.Equal(latestReport.Id, resultLatest.Id);
        Assert.True(resultLatest.IsLatest);
    }

    [Fact]
    public async Task GetReviewReport_NoReportExistsForGivenId_ThrowsNotFoundException()
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
        await Assert.ThrowsAsync<NotFoundException>(() => service.GetReviewReportAsync(caseId, Guid.NewGuid()));
    }

    [Fact]
    public async Task GetReviewReport_NonOwner_ThrowsForbiddenAccessException()
    {
        // Arrange
        var dbOptions = CreateInMemoryOptions();
        await using var dbContext = new ApplicationDbContext(dbOptions);

        var ownerId = Guid.NewGuid();
        var callerId = Guid.NewGuid();
        var caseId = Guid.NewGuid();
        var reportId = Guid.NewGuid();

        var caseEntity = new CaseEntity
        {
            Id = caseId,
            Title = "دعوى إخلاء",
            Description = "عدم سداد الإيجار",
            ClientId = ownerId,
            Status = CaseStatus.Reviewed
        };
        dbContext.Cases.Add(caseEntity);

        var report = new CaseReviewReport
        {
            Id = reportId,
            CaseId = caseId,
            IsLatest = true,
            CreatedAt = DateTime.UtcNow
        };
        dbContext.CaseReviewReports.Add(report);
        await dbContext.SaveChangesAsync();

        var currentUserService = new TestCurrentUserService { UserId = callerId };
        var service = new CaseReviewService(
            dbContext,
            currentUserService,
            new TestChatModelProvider(),
            NullLogger<CaseReviewService>.Instance);

        // Act & Assert
        await Assert.ThrowsAsync<ForbiddenAccessException>(() => service.GetReviewReportAsync(caseId, reportId));
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

        var reportsInDb = await dbContext.CaseReviewReports
            .Where(r => r.CaseId == caseId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();

        Assert.Equal(2, reportsInDb.Count);
        Assert.Equal(report2.Id, reportsInDb[0].Id);
        Assert.True(reportsInDb[0].IsLatest);
        Assert.False(reportsInDb[1].IsLatest);

        var fetchedReport1 = await reviewService.GetReviewReportAsync(caseId, report1.Id);
        var fetchedReport2 = await reviewService.GetReviewReportAsync(caseId, report2.Id);
        Assert.Equal(report1.Id, fetchedReport1.Id);
        Assert.Equal(report2.Id, fetchedReport2.Id);
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
