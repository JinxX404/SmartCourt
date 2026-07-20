using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using SmartCourt.Common.Entities;
using SmartCourt.Common.Exceptions;
using SmartCourt.Features.Auth.Enums;
using SmartCourt.Features.Auth.Shared;
using SmartCourt.Features.Users.Clients;
using SmartCourt.Features.Users.Lawyers;
using SmartCourt.Features.Users.Shared.DTOs;
using SmartCourt.Interfaces;
using SmartCourt.Persistence;
using Xunit;

namespace SmartCourt.Tests.Features.Users;

public sealed class AccountDeletionServiceTests
{
    private const string CurrentPassword = "CurrentPassword123!";

    [Fact]
    public async Task ClientDeletion_WrongPassword_ChangesNothing()
    {
        await using var testContext = CreateTestContext();
        var user = await testContext.CreateUserAsync(isLawyer: false);
        var originalSecurityStamp = user.SecurityStamp;
        var service = testContext.CreateClientService();

        await Assert.ThrowsAsync<BusinessException>(() =>
            service.DeleteProfileAsync(new DeleteAccountRequest("WrongPassword123!"), CancellationToken.None));

        var storedUser = await testContext.ReloadUserAsync(user.Id);
        Assert.Equal(UserStatus.Active, storedUser.Status);
        Assert.Equal(originalSecurityStamp, storedUser.SecurityStamp);
        Assert.All(storedUser.RefreshTokens, token => Assert.True(token.IsActive));
    }

    [Fact]
    public async Task ClientDeletion_RevokesSessionsAndIsSafeWhenRepeated()
    {
        await using var testContext = CreateTestContext();
        var user = await testContext.CreateUserAsync(isLawyer: false);
        var originalSecurityStamp = user.SecurityStamp;
        var service = testContext.CreateClientService();

        await service.DeleteProfileAsync(
            new DeleteAccountRequest(CurrentPassword),
            CancellationToken.None);
        await service.DeleteProfileAsync(
            new DeleteAccountRequest("WrongPassword123!"),
            CancellationToken.None);

        var storedUser = await testContext.ReloadUserAsync(user.Id);
        Assert.Equal(UserStatus.Deleted, storedUser.Status);
        Assert.NotEqual(originalSecurityStamp, storedUser.SecurityStamp);
        Assert.All(storedUser.RefreshTokens, token => Assert.False(token.IsActive));
    }

    [Fact]
    public async Task LawyerDeletion_RevokesSessionsAndHidesProfile()
    {
        await using var testContext = CreateTestContext();
        var user = await testContext.CreateUserAsync(isLawyer: true);
        var originalSecurityStamp = user.SecurityStamp;
        var service = testContext.CreateLawyerService();

        await service.DeleteProfileAsync(
            new DeleteAccountRequest(CurrentPassword),
            CancellationToken.None);

        var storedUser = await testContext.ReloadUserAsync(user.Id);
        Assert.Equal(UserStatus.Deleted, storedUser.Status);
        Assert.NotEqual(originalSecurityStamp, storedUser.SecurityStamp);
        Assert.All(storedUser.RefreshTokens, token => Assert.False(token.IsActive));
        Assert.NotNull(storedUser.LawyerProfile);
        Assert.False(storedUser.LawyerProfile.IsAvailable);
    }

    private static DeletionTestContext CreateTestContext()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseInMemoryDatabase(Guid.NewGuid().ToString())
                .ConfigureWarnings(warnings =>
                    warnings.Ignore(InMemoryEventId.TransactionIgnoredWarning)));
        services.AddIdentityCore<ApplicationUser>()
            .AddRoles<IdentityRole<Guid>>()
            .AddEntityFrameworkStores<ApplicationDbContext>();

        var serviceProvider = services.BuildServiceProvider();
        return new DeletionTestContext(serviceProvider);
    }

    private sealed class DeletionTestContext : IAsyncDisposable
    {
        private readonly ServiceProvider _serviceProvider;
        private readonly TestCurrentUserService _currentUserService = new();
        private readonly TestAuthHelperService _authHelperService = new();

        public DeletionTestContext(ServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            DbContext = serviceProvider.GetRequiredService<ApplicationDbContext>();
            UserManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        }

        private ApplicationDbContext DbContext { get; }
        private UserManager<ApplicationUser> UserManager { get; }

        public ClientService CreateClientService()
        {
            return new ClientService(
                UserManager,
                DbContext,
                _currentUserService,
                _authHelperService);
        }

        public LawyerService CreateLawyerService()
        {
            return new LawyerService(
                UserManager,
                DbContext,
                _currentUserService,
                _authHelperService);
        }

        public async Task<ApplicationUser> CreateUserAsync(bool isLawyer)
        {
            var userId = Guid.NewGuid();
            var user = new ApplicationUser
            {
                Id = userId,
                UserName = $"user-{userId}@example.com",
                Email = $"user-{userId}@example.com",
                FullName = "Test User",
                NationalNumber = userId.ToString("N")[..14],
                Status = UserStatus.Active,
                EmailConfirmed = true,
                ClientProfile = isLawyer ? null : new ClientProfile { UserId = userId },
                LawyerProfile = isLawyer
                    ? new LawyerProfile { UserId = userId, IsAvailable = true }
                    : null
            };
            user.RefreshTokens.Add(new RefreshToken
            {
                HashedToken = Guid.NewGuid().ToString("N"),
                ExpiresOn = DateTime.UtcNow.AddDays(1)
            });

            var result = await UserManager.CreateAsync(user, CurrentPassword);
            Assert.True(result.Succeeded, string.Join(" ", result.Errors.Select(error => error.Description)));

            _currentUserService.UserId = userId;
            DbContext.ChangeTracker.Clear();
            return user;
        }

        public async Task<ApplicationUser> ReloadUserAsync(Guid userId)
        {
            DbContext.ChangeTracker.Clear();
            return await DbContext.Users
                .Include(user => user.RefreshTokens)
                .Include(user => user.ClientProfile)
                .Include(user => user.LawyerProfile)
                .SingleAsync(user => user.Id == userId);
        }

        public async ValueTask DisposeAsync()
        {
            await _serviceProvider.DisposeAsync();
        }
    }

    private sealed class TestCurrentUserService : ICurrentUserService
    {
        public Guid? UserId { get; set; }
        public bool IsAuthenticated => UserId.HasValue;
    }

    private sealed class TestAuthHelperService : IAuthHelperService
    {
        public Task EnsureRoleExistsAsync(string roleName)
        {
            throw new NotSupportedException();
        }

        public Task SendConfirmationEmailAsync(
            ApplicationUser user,
            CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }

        public Task SendChangeEmailConfirmationAsync(
            ApplicationUser user,
            string newEmail,
            CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }

        public string GenerateRefreshToken()
        {
            throw new NotSupportedException();
        }

        public string HashRefreshToken(string refreshToken)
        {
            throw new NotSupportedException();
        }

        public void RevokeAllActiveRefreshTokens(ApplicationUser applicationUser)
        {
            foreach (var token in applicationUser.RefreshTokens.Where(token => token.IsActive))
            {
                token.RevokedOn = DateTime.UtcNow;
            }
        }
    }
}
