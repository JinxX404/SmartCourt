using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using SmartCourt.Common.Entities;
using SmartCourt.Common.Enums;
using SmartCourt.Common.Exceptions;
using SmartCourt.Entities;
using SmartCourt.Features.Matching;
using SmartCourt.Interfaces.Providers;
using SmartCourt.Persistence;
using SmartCourt.Tests.TestDoubles;
using Xunit;
using CaseEntity = SmartCourt.Entities.Case;

namespace SmartCourt.Tests.Features.Matching;

public sealed class ExplanationAndRecommendationTests
{
    private static DbContextOptions<ApplicationDbContext> CreateInMemoryOptions()
    {
        return new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
    }

    [Fact]
    public async Task ProcessMatchingAndPersist_HappyPath_GeneratesExplanationsAndPersists()
    {
        // Arrange
        var dbOptions = CreateInMemoryOptions();
        await using var dbContext = new ApplicationDbContext(dbOptions);

        var clientId = Guid.NewGuid();
        var caseId = Guid.NewGuid();
        var caseEntity = new CaseEntity
        {
            Id = caseId,
            ClientId = clientId,
            Title = "قضية تجارية وسندات",
            Description = "نزاع تجاري يتعلق بعقود توريد",
            Governorate = "Cairo",
            Status = CaseStatus.FinalSubmitted
        };
        dbContext.Cases.Add(caseEntity);

        var caseProfile = new CaseProfile
        {
            Id = Guid.NewGuid(),
            CaseId = caseId,
            Specialization = Specialization.CommercialLaw,
            RequiredLawyerLevelId = LawyerLevel.PrimaryCourt,
            Complexity = CaseComplexity.Standard
        };
        dbContext.CaseProfiles.Add(caseProfile);

        var userA = new ApplicationUser { Id = Guid.NewGuid(), UserName = "lawyerA@example.com", Email = "lawyerA@example.com", FullName = "محامي أ", Status = SmartCourt.Features.Auth.Enums.UserStatus.Active, EmailConfirmed = true, Governorate = "Cairo" };
        var profileA = new LawyerProfile { UserId = userA.Id, User = userA, Level = LawyerLevel.PrimaryCourt, IsAvailable = true, AverageRating = 4.8m, AverageResponseTimeHours = 5m };
        profileA.Specializations.Add(new LawyerSpecialization { Specialization = Specialization.CommercialLaw, YearsOfExperience = 8, CasesHandled = 40 });
        dbContext.Users.Add(userA);
        dbContext.LawyerProfiles.Add(profileA);

        await dbContext.SaveChangesAsync();

        var chatModelProvider = new TestChatModelProvider
        {
            OutputToReturn = $$"""
                {
                  "{{userA.Id}}": "تم ترشيح المحامي لخبرته الكبيرة في القانون التجاري بالمحاكم الابتدائية بالقاهرة."
                }
                """
        };

        var service = new MatchingService(
            dbContext,
            chatModelProvider,
            NullLogger<MatchingService>.Instance);

        // Act
        var result = await service.ProcessMatchingAndPersistAsync(caseId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.TotalEligibleLawyers);
        Assert.Single(result.Recommendations);
        Assert.Equal(userA.Id, result.Recommendations[0].LawyerId);
        Assert.Equal("تم ترشيح المحامي لخبرته الكبيرة في القانون التجاري بالمحاكم الابتدائية بالقاهرة.", result.Recommendations[0].Explanation);

        var persistedRecs = await dbContext.CaseRecommendations.ToListAsync();
        Assert.Single(persistedRecs);
        Assert.Equal(userA.Id, persistedRecs[0].LawyerId);
        Assert.Equal(1, persistedRecs[0].Rank);
    }

    [Fact]
    public async Task GetRecommendations_CaseMatched_ReturnsRecommendations()
    {
        // Arrange
        var dbOptions = CreateInMemoryOptions();
        await using var dbContext = new ApplicationDbContext(dbOptions);

        var clientId = Guid.NewGuid();
        var caseId = Guid.NewGuid();
        var caseEntity = new CaseEntity
        {
            Id = caseId,
            ClientId = clientId,
            Title = "قضية إدارية",
            Description = "طعن إلغاء قرار إداري",
            Status = CaseStatus.Matched
        };
        dbContext.Cases.Add(caseEntity);

        var lawyerUser = new ApplicationUser { Id = Guid.NewGuid(), UserName = "lawyer@example.com", Email = "lawyer@example.com", FullName = "محامي إداري" };
        var lawyerProfile = new LawyerProfile { UserId = lawyerUser.Id, User = lawyerUser };
        dbContext.Users.Add(lawyerUser);
        dbContext.LawyerProfiles.Add(lawyerProfile);

        var rec = new CaseRecommendation
        {
            Id = Guid.NewGuid(),
            CaseId = caseId,
            LawyerId = lawyerUser.Id,
            LawyerProfile = lawyerProfile,
            TotalScore = 0.95m,
            LocationScore = 1.0m,
            ExperienceScore = 0.9m,
            RatingScore = 0.95m,
            ResponseTimeScore = 0.9m,
            Explanation = "توصية ممتازة",
            Rank = 1
        };
        dbContext.CaseRecommendations.Add(rec);
        await dbContext.SaveChangesAsync();

        var service = new MatchingService(
            dbContext,
            new TestChatModelProvider(),
            NullLogger<MatchingService>.Instance);

        // Act
        var result = await service.GetRecommendationsAsync(caseId, clientId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.TotalEligibleLawyers);
        Assert.Single(result.Recommendations);
        Assert.Equal("محامي إداري", result.Recommendations[0].LawyerName);
        Assert.Equal("توصية ممتازة", result.Recommendations[0].Explanation);
    }

    [Fact]
    public async Task GetRecommendations_NotMatchedStatus_ThrowsBusinessException()
    {
        // Arrange
        var dbOptions = CreateInMemoryOptions();
        await using var dbContext = new ApplicationDbContext(dbOptions);

        var clientId = Guid.NewGuid();
        var caseId = Guid.NewGuid();
        var caseEntity = new CaseEntity
        {
            Id = caseId,
            ClientId = clientId,
            Title = "قضية قيد المراجعة",
            Description = "وصف القضية",
            Status = CaseStatus.Reviewed // Not Matched!
        };
        dbContext.Cases.Add(caseEntity);
        await dbContext.SaveChangesAsync();

        var service = new MatchingService(
            dbContext,
            new TestChatModelProvider(),
            NullLogger<MatchingService>.Instance);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<BusinessException>(() => service.GetRecommendationsAsync(caseId, clientId));
        Assert.Equal("Recommendations are not available. The case has not been matched yet.", ex.Message);
    }

    [Fact]
    public async Task GetRecommendations_NonOwner_ThrowsForbiddenException()
    {
        // Arrange
        var dbOptions = CreateInMemoryOptions();
        await using var dbContext = new ApplicationDbContext(dbOptions);

        var ownerId = Guid.NewGuid();
        var attackerId = Guid.NewGuid();
        var caseId = Guid.NewGuid();
        var caseEntity = new CaseEntity
        {
            Id = caseId,
            ClientId = ownerId,
            Title = "قضية خاصة",
            Description = "وصف خاص",
            Status = CaseStatus.Matched
        };
        dbContext.Cases.Add(caseEntity);
        await dbContext.SaveChangesAsync();

        var service = new MatchingService(
            dbContext,
            new TestChatModelProvider(),
            NullLogger<MatchingService>.Instance);

        // Act & Assert
        await Assert.ThrowsAsync<ForbiddenAccessException>(() => service.GetRecommendationsAsync(caseId, attackerId));
    }
}
