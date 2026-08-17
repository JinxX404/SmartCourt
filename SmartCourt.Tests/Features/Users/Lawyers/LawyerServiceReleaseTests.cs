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
