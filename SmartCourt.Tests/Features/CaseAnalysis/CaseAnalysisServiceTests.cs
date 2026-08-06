using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using SmartCourt.Common.Enums;
using SmartCourt.Common.Exceptions;
using SmartCourt.Entities;
using SmartCourt.Features.CaseAnalysis;
using SmartCourt.Interfaces.Providers;
using SmartCourt.Persistence;
using SmartCourt.Tests.TestDoubles;
using Xunit;
using CaseEntity = SmartCourt.Entities.Case;

namespace SmartCourt.Tests.Features.CaseAnalysis;

public sealed class CaseAnalysisServiceTests
{
    private static DbContextOptions<ApplicationDbContext> CreateInMemoryOptions()
    {
        return new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
    }

    [Fact]
    public async Task AnalyzeCase_HappyPath_CreatesCaseProfile()
    {
        // Arrange
        var dbOptions = CreateInMemoryOptions();
        await using var dbContext = new ApplicationDbContext(dbOptions);

        var caseId = Guid.NewGuid();
        var caseEntity = new CaseEntity
        {
            Id = caseId,
            Title = "نزاع مدني وحيازة",
            Description = "المطالبة بإثبات ملكية واسترداد حيازة أرض زراعية",
            Status = CaseStatus.FinalSubmitted
        };
        dbContext.Cases.Add(caseEntity);
        await dbContext.SaveChangesAsync();

        var chatModelProvider = new TestChatModelProvider
        {
            OutputToReturn = """
                {
                  "specialization": "CivilLaw",
                  "requiredLawyerLevel": "AppealCourt",
                  "complexity": "Advanced"
                }
                """
        };

        var service = new CaseAnalysisService(
            dbContext,
            chatModelProvider,
            null!,
            null!,
            NullLogger<CaseAnalysisService>.Instance);

        // Act
        var profile = await service.AnalyzeCaseAsync(caseId);

        // Assert
        Assert.NotNull(profile);
        Assert.Equal(caseId, profile.CaseId);
        Assert.Equal(Specialization.CivilLaw, profile.Specialization);
        Assert.Equal(LawyerLevel.AppealCourt, profile.RequiredLawyerLevelId);
        Assert.Equal(CaseComplexity.Advanced, profile.Complexity);

        var profileInDb = await dbContext.CaseProfiles.FirstOrDefaultAsync(cp => cp.CaseId == caseId);
        Assert.NotNull(profileInDb);
        Assert.Equal(Specialization.CivilLaw, profileInDb.Specialization);
        Assert.Equal(LawyerLevel.AppealCourt, profileInDb.RequiredLawyerLevelId);
        Assert.Equal(CaseComplexity.Advanced, profileInDb.Complexity);
    }

    [Fact]
    public async Task AnalyzeCase_ExistingProfile_UpdatesCaseProfile()
    {
        // Arrange
        var dbOptions = CreateInMemoryOptions();
        await using var dbContext = new ApplicationDbContext(dbOptions);

        var caseId = Guid.NewGuid();
        var caseEntity = new CaseEntity
        {
            Id = caseId,
            Title = "قضية تجارية متقدمة",
            Description = "نزاع أسهم وترخيص شركة",
            Status = CaseStatus.FinalSubmitted
        };
        dbContext.Cases.Add(caseEntity);

        var existingProfile = new CaseProfile
        {
            Id = Guid.NewGuid(),
            CaseId = caseId,
            Specialization = Specialization.CivilLaw,
            RequiredLawyerLevelId = LawyerLevel.GeneralRegistration,
            Complexity = CaseComplexity.Routine
        };
        dbContext.CaseProfiles.Add(existingProfile);

        await dbContext.SaveChangesAsync();

        var chatModelProvider = new TestChatModelProvider
        {
            OutputToReturn = """
                {
                  "specialization": "CommercialLaw",
                  "requiredLawyerLevel": "CassationCourt",
                  "complexity": "Exceptional"
                }
                """
        };

        var service = new CaseAnalysisService(
            dbContext,
            chatModelProvider,
            null!,
            null!,
            NullLogger<CaseAnalysisService>.Instance);

        // Act
        var updatedProfile = await service.AnalyzeCaseAsync(caseId);

        // Assert
        Assert.Equal(existingProfile.Id, updatedProfile.Id);
        Assert.Equal(Specialization.CommercialLaw, updatedProfile.Specialization);
        Assert.Equal(LawyerLevel.CassationCourt, updatedProfile.RequiredLawyerLevelId);
        Assert.Equal(CaseComplexity.Exceptional, updatedProfile.Complexity);
    }

    [Fact]
    public async Task AnalyzeCase_UnparseableAiResponse_ThrowsBusinessException()
    {
        // Arrange
        var dbOptions = CreateInMemoryOptions();
        await using var dbContext = new ApplicationDbContext(dbOptions);

        var caseId = Guid.NewGuid();
        var caseEntity = new CaseEntity
        {
            Id = caseId,
            Title = "قضية غامضة",
            Description = "وصف غير واضح",
            Status = CaseStatus.FinalSubmitted
        };
        dbContext.Cases.Add(caseEntity);
        await dbContext.SaveChangesAsync();

        var chatModelProvider = new TestChatModelProvider
        {
            OutputToReturn = """
                {
                  "specialization": "InvalidSpecName",
                  "requiredLawyerLevel": "PrimaryCourt",
                  "complexity": "Standard"
                }
                """
        };

        var service = new CaseAnalysisService(
            dbContext,
            chatModelProvider,
            null!,
            null!,
            NullLogger<CaseAnalysisService>.Instance);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<BusinessException>(() => service.AnalyzeCaseAsync(caseId));
        Assert.Equal("AI analysis failed. Please try again.", ex.Message);
    }

    [Fact]
    public async Task AnalyzeCase_CaseNotFound_ThrowsNotFoundException()
    {
        // Arrange
        var dbOptions = CreateInMemoryOptions();
        await using var dbContext = new ApplicationDbContext(dbOptions);

        var service = new CaseAnalysisService(
            dbContext,
            new TestChatModelProvider(),
            null!,
            null!,
            NullLogger<CaseAnalysisService>.Instance);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => service.AnalyzeCaseAsync(Guid.NewGuid()));
    }
}
