using SmartCourt.Common.Entities;
using SmartCourt.Common.Enums;
using SmartCourt.Common.Exceptions;
using SmartCourt.Features.Auth.Enums;
using SmartCourt.Features.Auth.Shared;
using SmartCourt.Features.Users.Lawyers;
using SmartCourt.Features.Users.Lawyers.DTOs;
using SmartCourt.Interfaces;
using SmartCourt.Tests.Features.Auth;
using SmartCourt.Interfaces.Providers;
using SmartCourt.Common.Models;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace SmartCourt.Tests.Features.Users.Lawyers;

public sealed class LawyerServiceReleaseTests
{
    [Fact]
    public async Task PublicProfile_ClientReturnsNotFound()
    {
        await using var testContext = await PasswordServiceTestContext.CreateAsync();
        var client = await testContext.CreateUserAsync();
        var service = CreateService(testContext, client.Id);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            service.GetPublicProfileAsync(client.Id, CancellationToken.None));
    }

    [Fact]
    public async Task PublicProfile_NonActiveLawyerReturnsNotFound()
    {
        await using var testContext = await PasswordServiceTestContext.CreateAsync();
        var lawyer = await testContext.CreateUserAsync(
            UserStatus.PendingReview,
            emailConfirmed: true);
        await AddLawyerProfileAsync(testContext, lawyer.Id);
        var service = CreateService(testContext, lawyer.Id);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            service.GetPublicProfileAsync(lawyer.Id, CancellationToken.None));
    }

    [Fact]
    public async Task PublicProfile_ActiveConfirmedLawyerSucceeds()
    {
        await using var testContext = await PasswordServiceTestContext.CreateAsync();
        var lawyer = await testContext.CreateUserAsync(
            UserStatus.Active,
            emailConfirmed: true);
        await AddLawyerProfileAsync(testContext, lawyer.Id);
        var service = CreateService(testContext, lawyer.Id);

        var response = await service.GetPublicProfileAsync(
            lawyer.Id,
            CancellationToken.None);

        Assert.Equal(lawyer.Id, response.Id);
    }

    [Fact]
    public async Task PublicProfile_ReturnsRatingsAndSpecializationExperienceCorrectly()
    {
        await using var testContext = await PasswordServiceTestContext.CreateAsync();
        var lawyer = await testContext.CreateUserAsync(
            UserStatus.Active,
            emailConfirmed: true);
        
        var user = await testContext.ReloadUserAsync(lawyer.Id);
        user.Governorate = "Cairo";
        user.City = "Nasr City";
        
        var profile = new LawyerProfile
        {
            UserId = lawyer.Id,
            Level = LawyerLevel.AppealCourt,
            IsAvailable = true,
            AverageRating = 4.85m,
            TotalRatingCount = 12,
            TotalRatingSum = 58
        };
        testContext.DbContext.LawyerProfiles.Add(profile);
        testContext.DbContext.LawyerSpecializations.AddRange(
            new LawyerSpecialization
            {
                Id = Guid.NewGuid(),
                LawyerProfileUserId = lawyer.Id,
                Specialization = Specialization.CriminalLaw,
                YearsOfExperience = 7,
                CasesHandled = 45
            },
            new LawyerSpecialization
            {
                Id = Guid.NewGuid(),
                LawyerProfileUserId = lawyer.Id,
                Specialization = Specialization.CommercialLaw,
                YearsOfExperience = 10,
                CasesHandled = 80
            }
        );
        await testContext.DbContext.SaveChangesAsync();
        testContext.DbContext.ChangeTracker.Clear();

        var service = CreateService(testContext, lawyer.Id);

        var response = await service.GetPublicProfileAsync(lawyer.Id, CancellationToken.None);

        Assert.Equal(lawyer.Id, response.Id);
        Assert.Equal(4.85m, response.AverageRating);
        Assert.Equal(12, response.RatingCount);
        Assert.Equal("Cairo", response.Governorate);
        Assert.Equal("Nasr City", response.City);
        Assert.Equal(2, response.Specializations.Count);
        Assert.Contains(response.Specializations, s => s.Specialization == Specialization.CriminalLaw && s.YearsOfExperience == 7 && s.CasesHandled == 45);
        Assert.Contains(response.Specializations, s => s.Specialization == Specialization.CommercialLaw && s.YearsOfExperience == 10 && s.CasesHandled == 80);
        Assert.Equal(response.Specializations.First().YearsOfExperience, response.YearsOfExperience);
        Assert.Equal(response.Specializations.First().Specialization.ToString(), response.SpecializationName);
    }

    [Fact]
    public async Task SearchLawyers_ReturnsRatingsAndSpecializationExperienceCorrectly()
    {
        await using var testContext = await PasswordServiceTestContext.CreateAsync();
        var lawyer = await testContext.CreateUserAsync(
            UserStatus.Active,
            emailConfirmed: true);
        
        var user = await testContext.ReloadUserAsync(lawyer.Id);
        user.Governorate = "Giza";
        user.City = "Dokki";
        
        var profile = new LawyerProfile
        {
            UserId = lawyer.Id,
            Level = LawyerLevel.CassationCourt,
            IsAvailable = true,
            AverageRating = 4.90m,
            TotalRatingCount = 25,
            TotalRatingSum = 122
        };
        testContext.DbContext.LawyerProfiles.Add(profile);
        testContext.DbContext.LawyerSpecializations.Add(
            new LawyerSpecialization
            {
                Id = Guid.NewGuid(),
                LawyerProfileUserId = lawyer.Id,
                Specialization = Specialization.CorporateLaw,
                YearsOfExperience = 15,
                CasesHandled = 150
            }
        );
        await testContext.DbContext.SaveChangesAsync();
        testContext.DbContext.ChangeTracker.Clear();

        var service = CreateService(testContext, lawyer.Id);

        var response = await service.SearchLawyersAsync(new SearchLawyersRequest
        {
            PageNumber = 1,
            PageSize = 10,
            SortBy = LawyerSortBy.ExperienceLevel
        }, CancellationToken.None);

        Assert.NotNull(response.Data);
        var item = Assert.Single(response.Data);
        Assert.Equal(lawyer.Id, item.Id);
        Assert.Equal(4.90m, item.AverageRating);
        Assert.Equal(25, item.RatingCount);
        Assert.Equal("Giza", item.Governorate);
        Assert.Equal("Dokki", item.City);
        Assert.Single(item.Specializations);
        Assert.Equal(Specialization.CorporateLaw, item.Specializations[0].Specialization);
        Assert.Equal(15, item.Specializations[0].YearsOfExperience);
        Assert.Equal(150, item.Specializations[0].CasesHandled);
        Assert.Equal(15, item.YearsOfExperience);
        Assert.Equal(Specialization.CorporateLaw.ToString(), item.SpecializationName);
    }

    [Fact]
    public async Task GetProfile_ReturnsRatingsAndSpecializationExperienceCorrectly()
    {
        await using var testContext = await PasswordServiceTestContext.CreateAsync();
        var lawyer = await testContext.CreateUserAsync(
            UserStatus.Active,
            emailConfirmed: true);
        
        var user = await testContext.ReloadUserAsync(lawyer.Id);
        
        var profile = new LawyerProfile
        {
            UserId = lawyer.Id,
            Level = LawyerLevel.PrimaryCourt,
            IsAvailable = true,
            AverageRating = 4.20m,
            TotalRatingCount = 5,
            TotalRatingSum = 21
        };
        testContext.DbContext.LawyerProfiles.Add(profile);
        testContext.DbContext.LawyerSpecializations.Add(
            new LawyerSpecialization
            {
                Id = Guid.NewGuid(),
                LawyerProfileUserId = lawyer.Id,
                Specialization = Specialization.FamilyLaw,
                YearsOfExperience = 3,
                CasesHandled = 12
            }
        );
        await testContext.DbContext.SaveChangesAsync();
        testContext.DbContext.ChangeTracker.Clear();

        var service = CreateService(testContext, lawyer.Id);

        var response = await service.GetProfileAsync(CancellationToken.None);

        Assert.Equal(lawyer.Id, response.Id);
        Assert.Equal(4.20m, response.AverageRating);
        Assert.Equal(5, response.RatingCount);
        Assert.Single(response.Specializations);
        Assert.Equal(Specialization.FamilyLaw, response.Specializations[0].Specialization);
        Assert.Equal(3, response.Specializations[0].YearsOfExperience);
        Assert.Equal(12, response.Specializations[0].CasesHandled);
    }

    [Fact]
    public async Task UpdateProfile_InvalidEnumFailsValidation()
    {
        await using var testContext = await PasswordServiceTestContext.CreateAsync();
        var lawyer = await testContext.CreateUserAsync();
        var request = CreateUpdateRequest();
        request.Level = (LawyerLevel)127;
        var service = CreateService(testContext, lawyer.Id);

        var exception = await Assert.ThrowsAsync<ValidationException>(() =>
            service.UpdateProfileAsync(request, CancellationToken.None));

        Assert.Contains(nameof(request.Level), exception.Errors.Keys);
    }

    [Fact]
    public async Task SwitchAvailability_FromAvailableToUnavailable_Succeeds()
    {
        await using var testContext = await PasswordServiceTestContext.CreateAsync();
        var lawyer = await testContext.CreateUserAsync(UserStatus.Active, emailConfirmed: true);
        await AddLawyerProfileAsync(testContext, lawyer.Id);
        var service = CreateService(testContext, lawyer.Id);

        var request = new UpdateLawyerAvailabilityRequest { IsAvailable = false };
        var response = await service.SwitchAvailabilityAsync(request, CancellationToken.None);

        Assert.False(response.IsAvailable);
        Assert.Equal(lawyer.Id, response.LawyerId);

        var profile = await testContext.DbContext.LawyerProfiles.SingleAsync(lp => lp.UserId == lawyer.Id);
        Assert.False(profile.IsAvailable);
    }

    [Fact]
    public async Task SwitchAvailability_FromUnavailableToAvailable_Succeeds()
    {
        await using var testContext = await PasswordServiceTestContext.CreateAsync();
        var lawyer = await testContext.CreateUserAsync(UserStatus.Active, emailConfirmed: true);
        await AddLawyerProfileAsync(testContext, lawyer.Id);
        
        var profile = await testContext.DbContext.LawyerProfiles.SingleAsync(lp => lp.UserId == lawyer.Id);
        profile.IsAvailable = false;
        await testContext.DbContext.SaveChangesAsync();
        testContext.DbContext.ChangeTracker.Clear();

        var service = CreateService(testContext, lawyer.Id);

        var request = new UpdateLawyerAvailabilityRequest { IsAvailable = true };
        var response = await service.SwitchAvailabilityAsync(request, CancellationToken.None);

        Assert.True(response.IsAvailable);
        Assert.Equal(lawyer.Id, response.LawyerId);

        testContext.DbContext.ChangeTracker.Clear();
        var updatedProfile = await testContext.DbContext.LawyerProfiles.SingleAsync(lp => lp.UserId == lawyer.Id);
        Assert.True(updatedProfile.IsAvailable);
    }

    [Fact]
    public async Task SwitchAvailability_ToggleWithoutBody_FlipsAvailability()
    {
        await using var testContext = await PasswordServiceTestContext.CreateAsync();
        var lawyer = await testContext.CreateUserAsync(UserStatus.Active, emailConfirmed: true);
        await AddLawyerProfileAsync(testContext, lawyer.Id);
        var service = CreateService(testContext, lawyer.Id);

        // Initially true -> should flip to false
        var response1 = await service.SwitchAvailabilityAsync(null, CancellationToken.None);
        Assert.False(response1.IsAvailable);

        testContext.DbContext.ChangeTracker.Clear();
        var profile1 = await testContext.DbContext.LawyerProfiles.SingleAsync(lp => lp.UserId == lawyer.Id);
        Assert.False(profile1.IsAvailable);

        // Next toggle -> should flip back to true
        var response2 = await service.SwitchAvailabilityAsync(new UpdateLawyerAvailabilityRequest(), CancellationToken.None);
        Assert.True(response2.IsAvailable);

        testContext.DbContext.ChangeTracker.Clear();
        var profile2 = await testContext.DbContext.LawyerProfiles.SingleAsync(lp => lp.UserId == lawyer.Id);
        Assert.True(profile2.IsAvailable);
    }

    [Fact]
    public async Task SwitchAvailability_WhenPendingReviewUserToggles_SucceedsWithoutAdminPermissions()
    {
        await using var testContext = await PasswordServiceTestContext.CreateAsync();
        var lawyer = await testContext.CreateUserAsync(UserStatus.PendingReview, emailConfirmed: true);
        await AddLawyerProfileAsync(testContext, lawyer.Id);
        var service = CreateService(testContext, lawyer.Id);

        var request = new UpdateLawyerAvailabilityRequest { IsAvailable = true };
        var response = await service.SwitchAvailabilityAsync(request, CancellationToken.None);

        Assert.True(response.IsAvailable);
        Assert.Equal(lawyer.Id, response.LawyerId);

        testContext.DbContext.ChangeTracker.Clear();
        var profile = await testContext.DbContext.LawyerProfiles.SingleAsync(lp => lp.UserId == lawyer.Id);
        Assert.True(profile.IsAvailable);
    }

    [Fact]
    public async Task SwitchAvailability_NonExistentUser_ThrowsNotFoundException()
    {
        await using var testContext = await PasswordServiceTestContext.CreateAsync();
        var nonExistentId = Guid.NewGuid();
        var service = CreateService(testContext, nonExistentId);

        var request = new UpdateLawyerAvailabilityRequest { IsAvailable = false };
        await Assert.ThrowsAsync<NotFoundException>(() =>
            service.SwitchAvailabilityAsync(request, CancellationToken.None));
    }

    [Fact]
    public async Task GetTopLawyers_ReturnsTopThreeLawyersOrderedByTotalRatingCountThenAverageRating()
    {
        await using var testContext = await PasswordServiceTestContext.CreateAsync();

        // Lawyer 1: 100 count, 4.8 rating (1st: highest count and higher rating than Lawyer 2)
        var lawyer1 = await testContext.CreateUserAsync(UserStatus.Active, emailConfirmed: true);
        var u1 = await testContext.ReloadUserAsync(lawyer1.Id);
        u1.FullName = "Lawyer One";
        testContext.DbContext.LawyerProfiles.Add(new LawyerProfile
        {
            UserId = lawyer1.Id,
            AverageRating = 4.8m,
            TotalRatingCount = 100,
            TotalRatingSum = 480
        });

        // Lawyer 2: 100 count, 4.5 rating (2nd: same 100 count, but lower rating than Lawyer 1)
        var lawyer2 = await testContext.CreateUserAsync(UserStatus.Active, emailConfirmed: true);
        var u2 = await testContext.ReloadUserAsync(lawyer2.Id);
        u2.FullName = "Lawyer Two";
        testContext.DbContext.LawyerProfiles.Add(new LawyerProfile
        {
            UserId = lawyer2.Id,
            AverageRating = 4.5m,
            TotalRatingCount = 100,
            TotalRatingSum = 450
        });

        // Lawyer 3: 50 count, 4.9 rating (3rd: 50 count, higher rating than Lawyer 4)
        var lawyer3 = await testContext.CreateUserAsync(UserStatus.Active, emailConfirmed: true);
        var u3 = await testContext.ReloadUserAsync(lawyer3.Id);
        u3.FullName = "Lawyer Three";
        testContext.DbContext.LawyerProfiles.Add(new LawyerProfile
        {
            UserId = lawyer3.Id,
            AverageRating = 4.9m,
            TotalRatingCount = 50,
            TotalRatingSum = 245
        });

        // Lawyer 4: 50 count, 4.2 rating (4th: excluded by take 3)
        var lawyer4 = await testContext.CreateUserAsync(UserStatus.Active, emailConfirmed: true);
        var u4 = await testContext.ReloadUserAsync(lawyer4.Id);
        u4.FullName = "Lawyer Four";
        testContext.DbContext.LawyerProfiles.Add(new LawyerProfile
        {
            UserId = lawyer4.Id,
            AverageRating = 4.2m,
            TotalRatingCount = 50,
            TotalRatingSum = 210
        });

        // Lawyer 5: 200 count, 5.0 rating but PendingReview (should be excluded)
        var lawyer5 = await testContext.CreateUserAsync(UserStatus.PendingReview, emailConfirmed: true);
        var u5 = await testContext.ReloadUserAsync(lawyer5.Id);
        u5.FullName = "Lawyer Five";
        testContext.DbContext.LawyerProfiles.Add(new LawyerProfile
        {
            UserId = lawyer5.Id,
            AverageRating = 5.0m,
            TotalRatingCount = 200,
            TotalRatingSum = 1000
        });

        // Lawyer 6: 200 count, 5.0 rating but Email not confirmed (should be excluded)
        var lawyer6 = await testContext.CreateUserAsync(UserStatus.Active, emailConfirmed: false);
        var u6 = await testContext.ReloadUserAsync(lawyer6.Id);
        u6.FullName = "Lawyer Six";
        testContext.DbContext.LawyerProfiles.Add(new LawyerProfile
        {
            UserId = lawyer6.Id,
            AverageRating = 5.0m,
            TotalRatingCount = 200,
            TotalRatingSum = 1000
        });

        await testContext.DbContext.SaveChangesAsync();
        testContext.DbContext.ChangeTracker.Clear();

        var service = CreateService(testContext, Guid.NewGuid());
        var topLawyers = await service.GetTopLawyersAsync(CancellationToken.None);

        Assert.NotNull(topLawyers);
        Assert.Equal(3, topLawyers.Count);

        Assert.Equal(lawyer1.Id, topLawyers[0].Id);
        Assert.Equal(100, topLawyers[0].RatingCount);
        Assert.Equal(4.8m, topLawyers[0].AverageRating);

        Assert.Equal(lawyer2.Id, topLawyers[1].Id);
        Assert.Equal(100, topLawyers[1].RatingCount);
        Assert.Equal(4.5m, topLawyers[1].AverageRating);

        Assert.Equal(lawyer3.Id, topLawyers[2].Id);
        Assert.Equal(50, topLawyers[2].RatingCount);
        Assert.Equal(4.9m, topLawyers[2].AverageRating);
    }

    private static LawyerService CreateService(
        PasswordServiceTestContext testContext,
        Guid userId)
    {
        return new LawyerService(
            testContext.UserManager,
            testContext.DbContext,
            new TestCurrentUserService(userId),
            new TestAuthHelperService(),
            new TestFileStorageService());
    }

    private static async Task AddLawyerProfileAsync(
        PasswordServiceTestContext testContext,
        Guid userId)
    {
        var user = await testContext.ReloadUserAsync(userId);
        user.LawyerProfile = new LawyerProfile
        {
            UserId = userId,
            Level = LawyerLevel.GeneralRegistration,
            IsAvailable = true
        };
        await testContext.DbContext.SaveChangesAsync();
        testContext.DbContext.ChangeTracker.Clear();
    }

    private static UpdateLawyerProfileRequest CreateUpdateRequest()
    {
        return new UpdateLawyerProfileRequest
        {
            Level = LawyerLevel.GeneralRegistration,
            Address = "Cairo"
        };
    }

    private sealed class TestCurrentUserService(Guid userId) : ICurrentUserService
    {
        public Guid? UserId { get; } = userId;
        public bool IsAuthenticated => true;
    }

    private sealed class TestAuthHelperService : IAuthHelperService
    {
        public Task EnsureRoleExistsAsync(string roleName)
            => throw new NotSupportedException();

        public Task SendConfirmationEmailAsync(
            ApplicationUser user,
            CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public string GenerateRefreshToken()
            => throw new NotSupportedException();

        public string HashRefreshToken(string refreshToken)
            => throw new NotSupportedException();

        public void RevokeAllActiveRefreshTokens(ApplicationUser applicationUser)
            => throw new NotSupportedException();
    }

    private sealed class TestFileStorageService : IFileStorageService
    {
        public Task<FileUploadResult> UploadAsync(Stream stream, string filePath, string originalFileName, CancellationToken cancellationToken = default) => Task.FromResult(new FileUploadResult { StoragePath = filePath, OriginalFileName = originalFileName, Size = 0 });
        public Task<FileUploadResult> UploadAsync(Stream stream, string filePath, string originalFileName, string? contentType, CancellationToken cancellationToken = default) => Task.FromResult(new FileUploadResult { StoragePath = filePath, OriginalFileName = originalFileName, Size = 0 });
        public Task<byte[]> DownloadAsync(string filePath, CancellationToken cancellationToken = default) => Task.FromResult(Array.Empty<byte>());
        public Task DeleteAsync(string filePath, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task<bool> ExistsAsync(string filePath, CancellationToken cancellationToken = default) => Task.FromResult(true);
        public Task<string> GetDownloadUrlAsync(string filePath, CancellationToken cancellationToken = default) => Task.FromResult("url");
    }
}
