using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SmartCourt.Common.Entities;
using SmartCourt.Features.Auth.ChangePassword;
using SmartCourt.Features.Auth.Enums;
using SmartCourt.Features.Auth.ResetPassword;
using SmartCourt.Features.Auth.Shared;
using SmartCourt.Interfaces;
using SmartCourt.Persistence;
using System.Text;
using Xunit;

namespace SmartCourt.Tests.Features.Auth;

internal sealed class PasswordServiceTestContext : IAsyncDisposable
{
    public const string CurrentPassword = "CurrentPassword123!";

    private readonly SqliteConnection _connection;
    private readonly ServiceProvider _serviceProvider;
    private readonly TestCurrentUserService _currentUserService = new();
    private readonly TestAuthHelperService _authHelperService = new();

    private PasswordServiceTestContext(
        SqliteConnection connection,
        ServiceProvider serviceProvider,
        ApplicationDbContext dbContext,
        TestUserManager userManager)
    {
        _connection = connection;
        _serviceProvider = serviceProvider;
        DbContext = dbContext;
        UserManager = userManager;
    }

    public ApplicationDbContext DbContext { get; }
    public TestUserManager UserManager { get; }
    public RoleManager<IdentityRole<Guid>> RoleManager
        => _serviceProvider.GetRequiredService<RoleManager<IdentityRole<Guid>>>();

    public static async Task<PasswordServiceTestContext> CreateAsync(
        TimeSpan? passwordResetTokenLifespan = null)
    {
        var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddDataProtection();
        services.AddDbContext<ApplicationDbContext>(options => options.UseSqlite(connection));
        services.Configure<DataProtectionTokenProviderOptions>(options =>
            options.TokenLifespan = passwordResetTokenLifespan ?? TimeSpan.FromDays(1));
        services.AddIdentityCore<ApplicationUser>()
            .AddRoles<IdentityRole<Guid>>()
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();

        var serviceProvider = services.BuildServiceProvider();
        var dbContext = serviceProvider.GetRequiredService<ApplicationDbContext>();
        await dbContext.Database.EnsureCreatedAsync();

        var userManager = new TestUserManager(
            serviceProvider.GetRequiredService<IUserStore<ApplicationUser>>(),
            serviceProvider.GetRequiredService<IOptions<IdentityOptions>>(),
            serviceProvider.GetRequiredService<IPasswordHasher<ApplicationUser>>(),
            serviceProvider.GetServices<IUserValidator<ApplicationUser>>(),
            serviceProvider.GetServices<IPasswordValidator<ApplicationUser>>(),
            serviceProvider.GetRequiredService<ILookupNormalizer>(),
            serviceProvider.GetRequiredService<IdentityErrorDescriber>(),
            serviceProvider,
            serviceProvider.GetRequiredService<ILogger<UserManager<ApplicationUser>>>());

        return new PasswordServiceTestContext(
            connection,
            serviceProvider,
            dbContext,
            userManager);
    }

    public ChangePasswordService CreateChangePasswordService()
    {
        return new ChangePasswordService(
            UserManager,
            DbContext,
            _authHelperService,
            _currentUserService);
    }

    public ResetPasswordService CreateResetPasswordService()
    {
        return new ResetPasswordService(UserManager, DbContext, _authHelperService);
    }

    public async Task<ApplicationUser> CreateUserAsync(
        UserStatus status = UserStatus.Active,
        bool emailConfirmed = true,
        string? email = null,
        string fullName = "Test User",
        string? role = null)
    {
        var userId = Guid.NewGuid();
        email ??= $"user-{userId}@example.com";
        var user = new ApplicationUser
        {
            Id = userId,
            UserName = email,
            Email = email,
            FullName = fullName,
            NationalNumber = userId.ToString("N")[..14],
            Status = status,
            EmailConfirmed = emailConfirmed
        };

        var result = await UserManager.CreateAsync(user, CurrentPassword);
        Assert.True(
            result.Succeeded,
            string.Join(" ", result.Errors.Select(error => error.Description)));

        if (!string.IsNullOrWhiteSpace(role))
        {
            if (!await RoleManager.RoleExistsAsync(role))
            {
                var roleResult = await RoleManager.CreateAsync(new IdentityRole<Guid>(role));
                Assert.True(roleResult.Succeeded);
            }

            var roleAssignmentResult = await UserManager.AddToRoleAsync(user, role);
            Assert.True(roleAssignmentResult.Succeeded);
        }

        var refreshToken = new RefreshToken
        {
            HashedToken = Guid.NewGuid().ToString("N"),
            ExpiresOn = DateTime.UtcNow.AddDays(1)
        };
        user.RefreshTokens.Add(refreshToken);
        DbContext.Entry(refreshToken).Property<int>("Id").CurrentValue = 1;
        await DbContext.SaveChangesAsync();

        _currentUserService.UserId = userId;
        DbContext.ChangeTracker.Clear();
        return user;
    }

    public async Task<string> GenerateEncodedResetTokenAsync(ApplicationUser user)
    {
        var token = await UserManager.GeneratePasswordResetTokenAsync(user);
        return WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));
    }

    public async Task<string> GenerateEncodedEmailConfirmationTokenAsync(ApplicationUser user)
    {
        var token = await UserManager.GenerateEmailConfirmationTokenAsync(user);
        return WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));
    }

    public async Task<ApplicationUser> ReloadUserAsync(Guid userId)
    {
        DbContext.ChangeTracker.Clear();
        return await DbContext.Users
            .Include(user => user.RefreshTokens)
            .SingleAsync(user => user.Id == userId);
    }

    public async ValueTask DisposeAsync()
    {
        await _serviceProvider.DisposeAsync();
        await _connection.DisposeAsync();
    }

    internal sealed class TestUserManager(
        IUserStore<ApplicationUser> store,
        IOptions<IdentityOptions> optionsAccessor,
        IPasswordHasher<ApplicationUser> passwordHasher,
        IEnumerable<IUserValidator<ApplicationUser>> userValidators,
        IEnumerable<IPasswordValidator<ApplicationUser>> passwordValidators,
        ILookupNormalizer keyNormalizer,
        IdentityErrorDescriber errors,
        IServiceProvider services,
        ILogger<UserManager<ApplicationUser>> logger)
        : UserManager<ApplicationUser>(
            store,
            optionsAccessor,
            passwordHasher,
            userValidators,
            passwordValidators,
            keyNormalizer,
            errors,
            services,
            logger)
    {
        public bool FailExplicitUpdate { get; set; }

        public override Task<IdentityResult> UpdateAsync(ApplicationUser user)
        {
            if (!FailExplicitUpdate)
            {
                return base.UpdateAsync(user);
            }

            FailExplicitUpdate = false;
            return Task.FromResult(IdentityResult.Failed(new IdentityError
            {
                Code = "SimulatedUpdateFailure",
                Description = "Simulated final update failure."
            }));
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
            => throw new NotSupportedException();

        public Task SendConfirmationEmailAsync(
            ApplicationUser user,
            CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task SendChangeEmailConfirmationAsync(
            ApplicationUser user,
            string newEmail,
            CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public string GenerateRefreshToken()
            => throw new NotSupportedException();

        public string HashRefreshToken(string refreshToken)
            => throw new NotSupportedException();

        public void RevokeAllActiveRefreshTokens(ApplicationUser applicationUser)
        {
            foreach (var token in applicationUser.RefreshTokens.Where(token => token.IsActive))
            {
                token.RevokedOn = DateTime.UtcNow;
            }
        }
    }
}

internal sealed class TestWebHostEnvironment : IWebHostEnvironment
{
    public string ApplicationName { get; set; } = "SmartCourt.Tests";
    public string EnvironmentName { get; set; } = "Development";
    public string ContentRootPath { get; set; } = FindContentRoot();
    public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
    public string WebRootPath { get; set; } = string.Empty;
    public IFileProvider WebRootFileProvider { get; set; } = new NullFileProvider();

    private static string FindContentRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            var candidate = Path.Combine(
                directory.FullName,
                "SmartCourt",
                "Features",
                "Auth",
                "Shared",
                "Templates",
                "ResetPasswordEmail.html");

            if (File.Exists(candidate))
            {
                return Path.Combine(directory.FullName, "SmartCourt");
            }

            directory = directory.Parent;
        }

        throw new DirectoryNotFoundException("SmartCourt content root was not found.");
    }
}
