using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SmartCourt.Common.Entities;
using SmartCourt.Common.Enums;
using SmartCourt.Entities;
using SmartCourt.Features.Case.BusinessRules;
using SmartCourt.Features.Matching;
using SmartCourt.Persistence;
using SmartCourt.Tests.TestDoubles;
using Xunit;
using CaseEntity = SmartCourt.Entities.Case;

namespace SmartCourt.Tests.Features.Matching;

public sealed class MatchingEngineAndServiceTests
{
    private static DbContextOptions<ApplicationDbContext> CreateInMemoryOptions()
    {
        return new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
    }

    [Fact]
    public void GovernorateRegions_LocationScoring_CalculatesCorrectly()
    {
        // Same Governorate
        Assert.Equal(1.0, GovernorateRegions.CalculateLocationScore("Cairo", "Cairo"));
        Assert.Equal(1.0, GovernorateRegions.CalculateLocationScore("القاهرة", "القاهرة"));

        // Same Region (Greater Cairo)
        Assert.Equal(0.5, GovernorateRegions.CalculateLocationScore("Cairo", "Giza"));
        Assert.Equal(0.5, GovernorateRegions.CalculateLocationScore("القاهرة", "الجيزة"));

        // Different Region (Greater Cairo vs Alexandria)
        Assert.Equal(0.0, GovernorateRegions.CalculateLocationScore("Cairo", "Alexandria"));
        Assert.Equal(0.0, GovernorateRegions.CalculateLocationScore("القاهرة", "الإسكندرية"));
    }

    [Fact]
    public void MatchingEngine_SingleLawyer_AllNormalizedFactorsGet1()
    {
        // Arrange
        var candidate = new LawyerCandidate
        {
            LawyerId = Guid.NewGuid(),
            LawyerName = "محامي 1",
            Governorate = "Cairo",
            Level = LawyerLevel.AppealCourt,
            IsAvailable = true,
            AverageRating = 4.5m,
            AverageResponseTimeHours = 12m,
            SpecializationYearsOfExperience = 7,
            SpecializationCasesHandled = 25
        };

        var strategy = MatchingStrategy.GetStrategy(CaseComplexity.Standard);

        // Act
        var results = MatchingEngine.RankCandidates([candidate], "Cairo", strategy);

        // Assert
        Assert.Single(results);
        var scored = results[0];
        Assert.Equal(1, scored.Rank);
        Assert.Equal(1.0, scored.ExperienceScore);
        Assert.Equal(1.0, scored.LocationScore);
        Assert.Equal(0.9, scored.RatingScore); // 4.5 / 5.0 = 0.9
        Assert.Equal(1.0, scored.ResponseTimeScore);
    }

    [Fact]
    public void MatchingEngine_AllLawyersSameFactorValue_Gets1ForThatFactor()
    {
        // Arrange
        var candidate1 = new LawyerCandidate
        {
            LawyerId = Guid.NewGuid(),
            LawyerName = "محامي A",
            Governorate = "Cairo",
            Level = LawyerLevel.PrimaryCourt,
            IsAvailable = true,
            AverageRating = 4.0m,
            AverageResponseTimeHours = 10m,
            SpecializationYearsOfExperience = 5, // Same
            SpecializationCasesHandled = 10     // Same
        };

        var candidate2 = new LawyerCandidate
        {
            LawyerId = Guid.NewGuid(),
            LawyerName = "محامي B",
            Governorate = "Giza",
            Level = LawyerLevel.PrimaryCourt,
            IsAvailable = true,
            AverageRating = 5.0m,
            AverageResponseTimeHours = 5m,
            SpecializationYearsOfExperience = 5, // Same
            SpecializationCasesHandled = 10     // Same
        };

        var strategy = MatchingStrategy.GetStrategy(CaseComplexity.Routine);

        // Act
        var results = MatchingEngine.RankCandidates([candidate1, candidate2], "Cairo", strategy);

        // Assert
        Assert.Equal(2, results.Count);
        Assert.All(results, r => Assert.Equal(1.0, r.ExperienceScore));
    }

    [Fact]
    public void MatchingEngine_MultipleLawyers_RanksByDescendingTotalScore()
    {
        // Arrange
        var c1 = new LawyerCandidate
        {
            LawyerId = Guid.NewGuid(),
            LawyerName = "محامي Cairo High Exp",
            Governorate = "Cairo",
            AverageRating = 5.0m,
            AverageResponseTimeHours = 2m,
            SpecializationYearsOfExperience = 15,
            SpecializationCasesHandled = 80
        };

        var c2 = new LawyerCandidate
        {
            LawyerId = Guid.NewGuid(),
            LawyerName = "محامي Aswan Low Exp",
            Governorate = "Aswan",
            AverageRating = 3.0m,
            AverageResponseTimeHours = 24m,
            SpecializationYearsOfExperience = 2,
            SpecializationCasesHandled = 5
        };

        var strategy = MatchingStrategy.GetStrategy(CaseComplexity.Advanced);

        // Act
        var results = MatchingEngine.RankCandidates([c1, c2], "Cairo", strategy);

        // Assert
        Assert.Equal(2, results.Count);
        Assert.Equal(1, results[0].Rank);
        Assert.Equal(c1.LawyerId, results[0].Candidate.LawyerId);
        Assert.True(results[0].TotalScore > results[1].TotalScore);
    }

    [Fact]
    public async Task MatchingService_EligibilityFiltering_FiltersUnavailableLowerLevelMismatchedSpec()
    {
        // Arrange
        var dbOptions = CreateInMemoryOptions();
        await using var dbContext = new ApplicationDbContext(dbOptions);

        var caseId = Guid.NewGuid();
        var caseEntity = new CaseEntity
        {
            Id = caseId,
            Title = "قضية إيجارات وشقق",
            Description = "نزاع حول عقارات مدنية",
            Governorate = "Cairo",
            Status = CaseStatus.FinalSubmitted
        };
        dbContext.Cases.Add(caseEntity);

        var caseProfile = new CaseProfile
        {
            Id = Guid.NewGuid(),
            CaseId = caseId,
            Specialization = Specialization.CivilLaw,
            RequiredLawyerLevelId = LawyerLevel.PrimaryCourt,
            Complexity = CaseComplexity.Standard
        };
        dbContext.CaseProfiles.Add(caseProfile);

        // Lawyer A: Eligible (Available, PrimaryCourt, CivilLaw)
        var userA = new ApplicationUser { Id = Guid.NewGuid(), UserName = "lawyerA@example.com", Email = "lawyerA@example.com", FullName = "Lawyer A", Status = SmartCourt.Features.Auth.Enums.UserStatus.Active, EmailConfirmed = true, Governorate = "Cairo" };
        var profileA = new LawyerProfile { UserId = userA.Id, User = userA, Level = LawyerLevel.PrimaryCourt, IsAvailable = true, AverageRating = 4.5m, AverageResponseTimeHours = 10m };
        profileA.Specializations.Add(new LawyerSpecialization { Specialization = Specialization.CivilLaw, YearsOfExperience = 6, CasesHandled = 20 });
        dbContext.Users.Add(userA);
        dbContext.LawyerProfiles.Add(profileA);

        // Lawyer B: Ineligible (IsAvailable = false)
        var userB = new ApplicationUser { Id = Guid.NewGuid(), UserName = "lawyerB@example.com", Email = "lawyerB@example.com", FullName = "Lawyer B", Status = SmartCourt.Features.Auth.Enums.UserStatus.Active, EmailConfirmed = true, Governorate = "Cairo" };
        var profileB = new LawyerProfile { UserId = userB.Id, User = userB, Level = LawyerLevel.PrimaryCourt, IsAvailable = false, AverageRating = 5.0m, AverageResponseTimeHours = 2m };
        profileB.Specializations.Add(new LawyerSpecialization { Specialization = Specialization.CivilLaw, YearsOfExperience = 10, CasesHandled = 50 });
        dbContext.Users.Add(userB);
        dbContext.LawyerProfiles.Add(profileB);

        // Lawyer C: Ineligible (GeneralRegistration < PrimaryCourt)
        var userC = new ApplicationUser { Id = Guid.NewGuid(), UserName = "lawyerC@example.com", Email = "lawyerC@example.com", FullName = "Lawyer C", Status = SmartCourt.Features.Auth.Enums.UserStatus.Active, EmailConfirmed = true, Governorate = "Cairo" };
        var profileC = new LawyerProfile { UserId = userC.Id, User = userC, Level = LawyerLevel.GeneralRegistration, IsAvailable = true, AverageRating = 4.8m, AverageResponseTimeHours = 4m };
        profileC.Specializations.Add(new LawyerSpecialization { Specialization = Specialization.CivilLaw, YearsOfExperience = 2, CasesHandled = 5 });
        dbContext.Users.Add(userC);
        dbContext.LawyerProfiles.Add(profileC);

        // Lawyer D: Ineligible (Mismatched Specialization: CriminalLaw instead of CivilLaw)
        var userD = new ApplicationUser { Id = Guid.NewGuid(), UserName = "lawyerD@example.com", Email = "lawyerD@example.com", FullName = "Lawyer D", Status = SmartCourt.Features.Auth.Enums.UserStatus.Active, EmailConfirmed = true, Governorate = "Cairo" };
        var profileD = new LawyerProfile { UserId = userD.Id, User = userD, Level = LawyerLevel.AppealCourt, IsAvailable = true, AverageRating = 4.9m, AverageResponseTimeHours = 1m };
        profileD.Specializations.Add(new LawyerSpecialization { Specialization = Specialization.CriminalLaw, YearsOfExperience = 8, CasesHandled = 30 });
        dbContext.Users.Add(userD);
        dbContext.LawyerProfiles.Add(profileD);

        await dbContext.SaveChangesAsync();

        var service = new MatchingService(
            dbContext,
            new TestChatModelProvider(),
            Microsoft.Extensions.Logging.Abstractions.NullLogger<MatchingService>.Instance);

        // Act
        var matches = await service.FindAndScoreMatchesAsync(caseId);

        // Assert
        Assert.Single(matches);
        Assert.Equal(userA.Id, matches[0].Candidate.LawyerId);
        Assert.Equal("Lawyer A", matches[0].Candidate.LawyerName);
    }
}
