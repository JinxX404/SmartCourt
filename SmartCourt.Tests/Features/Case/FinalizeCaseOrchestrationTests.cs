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
using SmartCourt.Features.Case.FinalizeCase;
using SmartCourt.Features.CaseAnalysis;
using SmartCourt.Features.Matching;
using SmartCourt.Interfaces;
using SmartCourt.Interfaces.Providers;
using SmartCourt.Persistence;
using SmartCourt.Tests.TestDoubles;
using Xunit;
using CaseEntity = SmartCourt.Entities.Case;

namespace SmartCourt.Tests.Features.Case;

public sealed class FinalizeCaseOrchestrationTests
{
    private static DbContextOptions<ApplicationDbContext> CreateSQLiteOptions()
    {
        return new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlite($"Data Source={Guid.NewGuid()}.db")
            .Options;
    }

    private static ApplicationUser CreateClientUser(ApplicationDbContext db, Guid id = default)
    {
        var uid = id == default ? Guid.NewGuid() : id;
        var user = new ApplicationUser
        {
            Id = uid,
            UserName = $"client_{uid:N}@example.com",
            Email = $"client_{uid:N}@example.com",
            FullName = $"عميل {uid:N}",
            NationalNumber = $"{Random.Shared.NextInt64(10000000000000L, 99999999999999L)}"
        };
        var clientProfile = new ClientProfile { UserId = uid, User = user };
        db.Users.Add(user);
        db.ClientProfile.Add(clientProfile);
        return user;
    }

    private static ApplicationUser CreateLawyerUser(ApplicationDbContext db, out LawyerProfile profile)
    {
        var uid = Guid.NewGuid();
        var user = new ApplicationUser
        {
            Id = uid,
            UserName = $"lawyer_{uid:N}@example.com",
            Email = $"lawyer_{uid:N}@example.com",
            FullName = $"محامي {uid:N}",
            NationalNumber = $"{Random.Shared.NextInt64(10000000000000L, 99999999999999L)}",
            Status = SmartCourt.Features.Auth.Enums.UserStatus.Active,
            EmailConfirmed = true,
            Governorate = "Cairo"
        };
        profile = new LawyerProfile
        {
            UserId = uid,
            User = user,
            Level = LawyerLevel.PrimaryCourt,
            IsAvailable = true,
            AverageRating = 4.7m,
            AverageResponseTimeHours = 3m
        };
        db.Users.Add(user);
        db.LawyerProfiles.Add(profile);
        return user;
    }

    [Fact]
    public async Task Finalize_HappyPath_ExecutesPipelineAndTransitionsToMatched()
    {
        // Arrange
        var dbOptions = CreateSQLiteOptions();
        await using var dbContext = new ApplicationDbContext(dbOptions);
        await dbContext.Database.EnsureCreatedAsync();

        var clientId = Guid.NewGuid();
        CreateClientUser(dbContext, clientId);

        var caseId = Guid.NewGuid();
        var caseEntity = new CaseEntity
        {
            Id = caseId,
            ClientId = clientId,
            Title = "قضية تجارية مدنية",
            Description = "نزاع حول عقد توريد تجاري",
            Governorate = "Cairo",
            Status = CaseStatus.Reviewed
        };
        dbContext.Cases.Add(caseEntity);

        var lawyerUser = CreateLawyerUser(dbContext, out var lawyerProfile);
        lawyerProfile.Specializations.Add(new LawyerSpecialization
        {
            Specialization = Specialization.CommercialLaw,
            YearsOfExperience = 7,
            CasesHandled = 30
        });

        await dbContext.SaveChangesAsync();

        var chatModelProvider = new TestChatModelProvider
        {
            OutputToReturn = $$"""
                {
                  "specialization": "CommercialLaw",
                  "requiredLawyerLevel": "PrimaryCourt",
                  "complexity": "Standard",
                  "{{lawyerUser.Id}}": "سبب ترشيح المحامي التجاري بالقاهرة"
                }
                """
        };

        var currentUserService = new TestCurrentUserService { UserId = clientId };
        var caseAnalysisService = new CaseAnalysisService(dbContext, chatModelProvider, null!, null!, NullLogger<CaseAnalysisService>.Instance);
        var matchingService = new MatchingService(dbContext, chatModelProvider, NullLogger<MatchingService>.Instance);

        var handler = new FinalizeCaseHandler(
            dbContext,
            caseAnalysisService,
            matchingService,
            currentUserService,
            NullLogger<FinalizeCaseHandler>.Instance);

        // Act
        var result = await handler.Handle(new FinalizeCaseCommand { CaseId = caseId }, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.Equal(1, result.Data.TotalEligibleLawyers);
        Assert.Single(result.Data.Recommendations);
        Assert.Equal(lawyerUser.Id, result.Data.Recommendations[0].LawyerId);

        var updatedCase = await dbContext.Cases
            .Include(c => c.CaseProfile)
            .Include(c => c.Recommendations)
            .FirstOrDefaultAsync(c => c.Id == caseId);

        Assert.NotNull(updatedCase);
        Assert.Equal(CaseStatus.Matched, updatedCase.Status);
        Assert.NotNull(updatedCase.CaseProfile);
        Assert.Equal(Specialization.CommercialLaw, updatedCase.CaseProfile.Specialization);
        Assert.Single(updatedCase.Recommendations);
    }

    [Fact]
    public async Task Finalize_AlreadyMatched_ReturnsExistingRecommendationsIdempotently()
    {
        // Arrange
        var dbOptions = CreateSQLiteOptions();
        await using var dbContext = new ApplicationDbContext(dbOptions);
        await dbContext.Database.EnsureCreatedAsync();

        var clientId = Guid.NewGuid();
        CreateClientUser(dbContext, clientId);

        var caseId = Guid.NewGuid();
        var caseEntity = new CaseEntity
        {
            Id = caseId,
            ClientId = clientId,
            Title = "قضية مكتملة التوصية",
            Description = "وصف القضية المكتملة",
            Status = CaseStatus.Matched
        };
        dbContext.Cases.Add(caseEntity);

        var lawyerUser = CreateLawyerUser(dbContext, out var lawyerProfile);
        var recommendation = new CaseRecommendation
        {
            Id = Guid.NewGuid(),
            CaseId = caseId,
            LawyerId = lawyerUser.Id,
            LawyerProfile = lawyerProfile,
            TotalScore = 0.9m,
            LocationScore = 1.0m,
            ExperienceScore = 0.85m,
            RatingScore = 0.9m,
            ResponseTimeScore = 0.9m,
            Explanation = "توصية سابقة متوفرة",
            Rank = 1
        };
        dbContext.CaseRecommendations.Add(recommendation);
        await dbContext.SaveChangesAsync();

        var currentUserService = new TestCurrentUserService { UserId = clientId };
        var caseAnalysisService = new CaseAnalysisService(dbContext, new TestChatModelProvider(), null!, null!, NullLogger<CaseAnalysisService>.Instance);
        var matchingService = new MatchingService(dbContext, new TestChatModelProvider(), NullLogger<MatchingService>.Instance);

        var handler = new FinalizeCaseHandler(
            dbContext,
            caseAnalysisService,
            matchingService,
            currentUserService,
            NullLogger<FinalizeCaseHandler>.Instance);

        // Act
        var result = await handler.Handle(new FinalizeCaseCommand { CaseId = caseId }, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.Equal(1, result.Data!.TotalEligibleLawyers);
        Assert.Equal("توصية سابقة متوفرة", result.Data.Recommendations[0].Explanation);
    }

    [Fact]
    public async Task Finalize_DraftStatus_ThrowsBusinessException()
    {
        // Arrange
        var dbOptions = CreateSQLiteOptions();
        await using var dbContext = new ApplicationDbContext(dbOptions);
        await dbContext.Database.EnsureCreatedAsync();

        var clientId = Guid.NewGuid();
        CreateClientUser(dbContext, clientId);

        var caseId = Guid.NewGuid();
        var caseEntity = new CaseEntity
        {
            Id = caseId,
            ClientId = clientId,
            Title = "مسودة قضية غير مسموح بإتمامها",
            Description = "وصف القضية",
            Status = CaseStatus.Draft
        };
        dbContext.Cases.Add(caseEntity);
        await dbContext.SaveChangesAsync();

        var currentUserService = new TestCurrentUserService { UserId = clientId };
        var caseAnalysisService = new CaseAnalysisService(dbContext, new TestChatModelProvider(), null!, null!, NullLogger<CaseAnalysisService>.Instance);
        var matchingService = new MatchingService(dbContext, new TestChatModelProvider(), NullLogger<MatchingService>.Instance);

        var handler = new FinalizeCaseHandler(
            dbContext,
            caseAnalysisService,
            matchingService,
            currentUserService,
            NullLogger<FinalizeCaseHandler>.Instance);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<BusinessException>(() => handler.Handle(new FinalizeCaseCommand { CaseId = caseId }, CancellationToken.None));
        Assert.Contains("في حالة التقديم أو المراجعة", ex.Message);
    }

    [Fact]
    public async Task Finalize_SubmittedStatus_ExecutesPipelineAndTransitionsToMatched()
    {
        // Arrange
        var dbOptions = CreateSQLiteOptions();
        await using var dbContext = new ApplicationDbContext(dbOptions);
        await dbContext.Database.EnsureCreatedAsync();

        var clientId = Guid.NewGuid();
        CreateClientUser(dbContext, clientId);

        var caseId = Guid.NewGuid();
        var caseEntity = new CaseEntity
        {
            Id = caseId,
            ClientId = clientId,
            Title = "قضية مقدمة جاهزة للإتمام",
            Description = "وصف القضية المقدمة",
            Governorate = "Cairo",
            Status = CaseStatus.Submitted
        };
        dbContext.Cases.Add(caseEntity);

        var lawyerUser = CreateLawyerUser(dbContext, out var lawyerProfile);
        lawyerProfile.Specializations.Add(new LawyerSpecialization
        {
            Specialization = Specialization.CommercialLaw,
            YearsOfExperience = 7,
            CasesHandled = 30
        });

        await dbContext.SaveChangesAsync();

        var chatModelProvider = new TestChatModelProvider
        {
            OutputToReturn = $$"""
                {
                  "specialization": "CommercialLaw",
                  "requiredLawyerLevel": "PrimaryCourt",
                  "complexity": "Standard",
                  "{{lawyerUser.Id}}": "سبب ترشيح المحامي التجاري بالقاهرة"
                }
                """
        };

        var currentUserService = new TestCurrentUserService { UserId = clientId };
        var caseAnalysisService = new CaseAnalysisService(dbContext, chatModelProvider, null!, null!, NullLogger<CaseAnalysisService>.Instance);
        var matchingService = new MatchingService(dbContext, chatModelProvider, NullLogger<MatchingService>.Instance);

        var handler = new FinalizeCaseHandler(
            dbContext,
            caseAnalysisService,
            matchingService,
            currentUserService,
            NullLogger<FinalizeCaseHandler>.Instance);

        // Act
        var result = await handler.Handle(new FinalizeCaseCommand { CaseId = caseId }, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Success);
        var updatedCase = await dbContext.Cases.FirstOrDefaultAsync(c => c.Id == caseId);
        Assert.NotNull(updatedCase);
        Assert.Equal(CaseStatus.Matched, updatedCase.Status);
    }

    [Fact]
    public async Task Finalize_NonOwner_ThrowsForbiddenAccessException()
    {
        // Arrange
        var dbOptions = CreateSQLiteOptions();
        await using var dbContext = new ApplicationDbContext(dbOptions);
        await dbContext.Database.EnsureCreatedAsync();

        var ownerId = Guid.NewGuid();
        CreateClientUser(dbContext, ownerId);

        var attackerId = Guid.NewGuid();
        CreateClientUser(dbContext, attackerId);

        var caseId = Guid.NewGuid();
        var caseEntity = new CaseEntity
        {
            Id = caseId,
            ClientId = ownerId,
            Title = "قضية شخص آخر",
            Description = "وصف القضية",
            Status = CaseStatus.Reviewed
        };
        dbContext.Cases.Add(caseEntity);
        await dbContext.SaveChangesAsync();

        var currentUserService = new TestCurrentUserService { UserId = attackerId };
        var caseAnalysisService = new CaseAnalysisService(dbContext, new TestChatModelProvider(), null!, null!, NullLogger<CaseAnalysisService>.Instance);
        var matchingService = new MatchingService(dbContext, new TestChatModelProvider(), NullLogger<MatchingService>.Instance);

        var handler = new FinalizeCaseHandler(
            dbContext,
            caseAnalysisService,
            matchingService,
            currentUserService,
            NullLogger<FinalizeCaseHandler>.Instance);

        // Act & Assert
        await Assert.ThrowsAsync<ForbiddenAccessException>(() => handler.Handle(new FinalizeCaseCommand { CaseId = caseId }, CancellationToken.None));
    }

    [Fact]
    public async Task Finalize_AiFailure_RollsBackTransactionAndRevertsToReviewed()
    {
        // Arrange
        var dbOptions = CreateSQLiteOptions();
        await using var dbContext = new ApplicationDbContext(dbOptions);
        await dbContext.Database.EnsureCreatedAsync();

        var clientId = Guid.NewGuid();
        CreateClientUser(dbContext, clientId);

        var caseId = Guid.NewGuid();
        var caseEntity = new CaseEntity
        {
            Id = caseId,
            ClientId = clientId,
            Title = "قضية معقدة",
            Description = "وصف القضية المعقدة",
            Status = CaseStatus.Reviewed
        };
        dbContext.Cases.Add(caseEntity);
        await dbContext.SaveChangesAsync();

        var failingChatProvider = new FailingChatModelProvider();
        var currentUserService = new TestCurrentUserService { UserId = clientId };
        var caseAnalysisService = new CaseAnalysisService(dbContext, failingChatProvider, null!, null!, NullLogger<CaseAnalysisService>.Instance);
        var matchingService = new MatchingService(dbContext, failingChatProvider, NullLogger<MatchingService>.Instance);

        var handler = new FinalizeCaseHandler(
            dbContext,
            caseAnalysisService,
            matchingService,
            currentUserService,
            NullLogger<FinalizeCaseHandler>.Instance);

        // Act & Assert
        await Assert.ThrowsAsync<BusinessException>(() => handler.Handle(new FinalizeCaseCommand { CaseId = caseId }, CancellationToken.None));

        // Verify state after rollback
        await using var verifyDbContext = new ApplicationDbContext(dbOptions);
        var refreshedCase = await verifyDbContext.Cases
            .Include(c => c.CaseProfile)
            .FirstOrDefaultAsync(c => c.Id == caseId);

        Assert.NotNull(refreshedCase);
        Assert.Equal(CaseStatus.Reviewed, refreshedCase.Status);
        Assert.Null(refreshedCase.CaseProfile);
        var countRecs = await verifyDbContext.CaseRecommendations.CountAsync(cr => cr.CaseId == caseId);
        Assert.Equal(0, countRecs);
    }

    [Fact]
    public async Task Finalize_NoEligibleLawyers_TransitionsToMatchedWithZeroLawyers()
    {
        // Arrange
        var dbOptions = CreateSQLiteOptions();
        await using var dbContext = new ApplicationDbContext(dbOptions);
        await dbContext.Database.EnsureCreatedAsync();

        var clientId = Guid.NewGuid();
        CreateClientUser(dbContext, clientId);

        var caseId = Guid.NewGuid();
        var caseEntity = new CaseEntity
        {
            Id = caseId,
            ClientId = clientId,
            Title = "قضية لا يتوفر لها محامون حالياً",
            Description = "وصف القضية النادرة",
            Status = CaseStatus.Reviewed
        };
        dbContext.Cases.Add(caseEntity);
        await dbContext.SaveChangesAsync();

        var chatModelProvider = new TestChatModelProvider
        {
            OutputToReturn = """
                {
                  "specialization": "FamilyLaw",
                  "requiredLawyerLevel": "CassationCourt",
                  "complexity": "Exceptional"
                }
                """
        };

        var currentUserService = new TestCurrentUserService { UserId = clientId };
        var caseAnalysisService = new CaseAnalysisService(dbContext, chatModelProvider, null!, null!, NullLogger<CaseAnalysisService>.Instance);
        var matchingService = new MatchingService(dbContext, chatModelProvider, NullLogger<MatchingService>.Instance);

        var handler = new FinalizeCaseHandler(
            dbContext,
            caseAnalysisService,
            matchingService,
            currentUserService,
            NullLogger<FinalizeCaseHandler>.Instance);

        // Act
        var result = await handler.Handle(new FinalizeCaseCommand { CaseId = caseId }, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.Equal(0, result.Data!.TotalEligibleLawyers);
        Assert.Empty(result.Data.Recommendations);

        var updatedCase = await dbContext.Cases.FirstOrDefaultAsync(c => c.Id == caseId);
        Assert.NotNull(updatedCase);
        Assert.Equal(CaseStatus.Matched, updatedCase.Status);
    }

    private sealed class FailingChatModelProvider : IChatModelProvider
    {
        public Task<ChatModelResponse> GenerateAsync(string systemPrompt, string userPrompt, CancellationToken cancellationToken = default)
        {
            throw new InvalidOperationException("AI Service Connection Timeout.");
        }
    }
}
