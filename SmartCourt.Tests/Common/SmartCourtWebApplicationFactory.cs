using System.IdentityModel.Tokens.Jwt;
using System.Linq.Expressions;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.IdentityModel.Tokens;
using SmartCourt.Common.Entities;
using SmartCourt.Common.Extensions;
using SmartCourt.Features.Auth.Enums;
using SmartCourt.Features.Notifications.Entities;
using SmartCourt.Infrastructure.Providers.Jobs;
using SmartCourt.Interfaces.Providers;
using SmartCourt.Persistence;

namespace SmartCourt.Tests.Common;

public class SqliteRowVersionInterceptor : SaveChangesInterceptor
{
    private static long _notificationSequence;

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        if (eventData.Context != null)
        {
            var entries = eventData.Context.ChangeTracker.Entries()
                .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified);

            foreach (var entry in entries)
            {
                if (entry.Entity is Notification notification
                    && notification.Sequence <= 0)
                {
                    notification.Sequence = Interlocked.Increment(
                        ref _notificationSequence);
                    entry.Property(nameof(Notification.Sequence)).IsTemporary = false;
                }

                var rowVersionProp = entry.Properties.FirstOrDefault(p => p.Metadata.Name == "RowVersion");
                if (rowVersionProp != null)
                {
                    if (rowVersionProp.CurrentValue is not byte[] val || val.Length == 0)
                    {
                        rowVersionProp.CurrentValue = Guid.NewGuid().ToByteArray();
                    }
                    rowVersionProp.IsTemporary = false;
                    rowVersionProp.IsModified = true;
                }
            }
        }
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }
}

public class SmartCourtWebApplicationFactory : WebApplicationFactory<Program>
{
    private SqliteConnection? _sqliteConnection;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureAppConfiguration((context, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Secret"] = "YOUR_JWT_SECRET_KEY_AT_LEAST_32_BYTES_LONG",
                ["Jwt:Issuer"] = "SmartCourtAPI",
                ["Jwt:Audience"] = "SmartCourtClient",
                ["Jwt:ExpiresInMinutes"] = "120",
                ["PaymentProvider:WebhookAllowedIpRanges:0"] = "127.0.0.1/32",
                ["PaymentProvider:WebhookAllowedIpRanges:1"] = "::1/128",
                ["PaymentProvider:WebhookMaximumBodySizeBytes"] = "65536",
                ["Qdrant:Host"] = "localhost",
                ["OutboxDispatch:Enabled"] = "false"
            });
        });

        builder.ConfigureServices(services =>
        {
            // Preserve Async Suffix for CreatedAtAction(nameof(GetAsync), ...)
            services.Configure<MvcOptions>(options =>
            {
                options.SuppressAsyncSuffixInActionNames = false;
            });

            // 1. Replace Database with SQLite In-Memory & Add RowVersion Interceptor
            var descriptor = services.FirstOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));
            if (descriptor != null)
            {
                services.Remove(descriptor);
            }

            _sqliteConnection = new SqliteConnection("Data Source=:memory:");
            _sqliteConnection.Open();

            services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseSqlite(_sqliteConnection);
                options.AddInterceptors(new SqliteRowVersionInterceptor());
            });

            // 2. Disable Rate Limiting for E2E Tests
            services.Configure<RateLimiterOptions>(options =>
            {
                options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(_ =>
                    RateLimitPartition.GetNoLimiter("unlimited"));
            });

            // 3. Replace Background Job services with In-Memory Test Doubles to bypass SQL Server connection
            services.RemoveAll<IContractRecurringJobRegistrar>();
            services.RemoveAll<IContractJobScheduler>();
            services.RemoveAll<IBackgroundJobProvider>();
            services.RemoveAll<IRecurringBackgroundJobProvider>();

            var hangfireHostedServices = services
                .Where(d => d.ServiceType == typeof(Microsoft.Extensions.Hosting.IHostedService)
                            && (d.ImplementationType?.Assembly.FullName?.Contains("Hangfire", StringComparison.OrdinalIgnoreCase) == true
                                || d.ImplementationFactory?.Method.DeclaringType?.Assembly.FullName?.Contains("Hangfire", StringComparison.OrdinalIgnoreCase) == true))
                .ToList();
            foreach (var s in hangfireHostedServices)
            {
                services.Remove(s);
            }

            services.AddSingleton<IContractRecurringJobRegistrar, TestContractRecurringJobRegistrar>();
            services.AddSingleton<IContractJobScheduler, TestContractJobScheduler>();
            services.AddSingleton<IBackgroundJobProvider, TestBackgroundJobProvider>();
            services.AddSingleton<IRecurringBackgroundJobProvider, TestRecurringBackgroundJobProvider>();

            var sp = services.BuildServiceProvider();
            using var scope = sp.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            var createScript = db.Database.GenerateCreateScript()
                .Replace(
                    "\"RowVersion\" BLOB NOT NULL",
                    "\"RowVersion\" BLOB NOT NULL DEFAULT (randomblob(8))",
                    StringComparison.Ordinal);
            db.Database.ExecuteSqlRaw(createScript);
            db.Database.ExecuteSqlRaw("PRAGMA foreign_keys = OFF;");
        });
    }

    public string GenerateJwtToken(Guid userId, string role, string email = "user@smartcourt.test")
    {
        const string secret = "YOUR_JWT_SECRET_KEY_AT_LEAST_32_BYTES_LONG";
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        // SecurityStamp must match DB user SecurityStamp for OnTokenValidated check
        var securityStamp = userId.ToString("N");

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new(ClaimTypes.NameIdentifier, userId.ToString()),
            new(JwtRegisteredClaimNames.Email, email),
            new("name", "Test User"),
            new(ApplicationUserExtensions.SecurityStampClaimType, securityStamp),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(ClaimTypes.Role, role)
        };

        var token = new JwtSecurityToken(
            issuer: "SmartCourtAPI",
            audience: "SmartCourtClient",
            claims: claims,
            notBefore: DateTimeOffset.UtcNow.AddMinutes(-5).UtcDateTime,
            expires: DateTimeOffset.UtcNow.AddHours(2).UtcDateTime,
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public HttpClient CreateAuthenticatedClient(Guid userId, string role, string email = "user@smartcourt.test")
    {
        var client = CreateClient();
        var token = GenerateJwtToken(userId, role, email);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
    }

    public async Task SeedUserAsync(Guid userId, string email, string role, string fullName = "Test User")
    {
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var securityStamp = userId.ToString("N");

        var existingUser = await db.Users.FindAsync(userId);
        if (existingUser == null)
        {
            var user = new ApplicationUser
            {
                Id = userId,
                UserName = email,
                NormalizedUserName = email.ToUpperInvariant(),
                Email = email,
                NormalizedEmail = email.ToUpperInvariant(),
                FullName = fullName,
                Status = UserStatus.Active,
                SecurityStamp = securityStamp,
                EmailConfirmed = true
            };
            db.Users.Add(user);
            await db.SaveChangesAsync();
        }
        else if (existingUser.SecurityStamp != securityStamp)
        {
            existingUser.SecurityStamp = securityStamp;
            await db.SaveChangesAsync();
        }

        var identityRole = await db.Roles.FirstOrDefaultAsync(r => r.Name == role);
        if (identityRole == null)
        {
            identityRole = new IdentityRole<Guid>
            {
                Id = Guid.NewGuid(),
                Name = role,
                NormalizedName = role.ToUpperInvariant()
            };
            db.Roles.Add(identityRole);
            await db.SaveChangesAsync();
        }

        var userRoleExists = await db.UserRoles.AnyAsync(ur => ur.UserId == userId && ur.RoleId == identityRole.Id);
        if (!userRoleExists)
        {
            db.UserRoles.Add(new IdentityUserRole<Guid>
            {
                UserId = userId,
                RoleId = identityRole.Id
            });
            await db.SaveChangesAsync();
        }
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (disposing)
        {
            _sqliteConnection?.Close();
            _sqliteConnection?.Dispose();
        }
    }
}

public class TestContractRecurringJobRegistrar : IContractRecurringJobRegistrar
{
    public Task RegisterAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}

public class TestBackgroundJobProvider : IBackgroundJobProvider
{
    public string Enqueue(Expression<Action> methodCall) => Guid.NewGuid().ToString();
    public string Enqueue<T>(Expression<Action<T>> methodCall) => Guid.NewGuid().ToString();
    public string Enqueue<T>(Expression<Func<T, Task>> methodCall) => Guid.NewGuid().ToString();
    public Task<string> EnqueueAsync<T>(Expression<Func<T, Task>> methodCall, CancellationToken cancellationToken)
        => Task.FromResult(Guid.NewGuid().ToString());
    public Task<string> ScheduleAsync<T>(Expression<Func<T, Task>> methodCall, DateTimeOffset runAt, CancellationToken cancellationToken)
        => Task.FromResult(Guid.NewGuid().ToString());
}

public class TestRecurringBackgroundJobProvider : IRecurringBackgroundJobProvider
{
    public Task RegisterOrUpdateAsync<T>(string recurringJobId, Expression<Func<T, Task>> methodCall, string cronExpression, CancellationToken cancellationToken)
        => Task.CompletedTask;
}

public class TestContractJobScheduler : IContractJobScheduler
{
    public Task<string> ScheduleAutoAcceptAsync(Guid milestoneId, Guid escrowHoldId, int submissionVersion, DateTimeOffset RunAtUtc, CancellationToken cancellationToken)
        => Task.FromResult(Guid.NewGuid().ToString());
    public Task<string> ScheduleReleaseExpiredHoldAsync(Guid escrowHoldId, DateTimeOffset RunAtUtc, CancellationToken cancellationToken)
        => Task.FromResult(Guid.NewGuid().ToString());
    public Task<string> ScheduleProviderReconciliationAsync(Guid paymentTransactionId, DateTimeOffset RunAtUtc, CancellationToken cancellationToken)
        => Task.FromResult(Guid.NewGuid().ToString());
    public Task<string> ScheduleProviderRetryAsync(Guid paymentTransactionId, DateTimeOffset RunAtUtc, CancellationToken cancellationToken)
        => Task.FromResult(Guid.NewGuid().ToString());
    public Task<string> ScheduleSchedulingReconciliationAsync(DateTimeOffset RunAtUtc, CancellationToken cancellationToken)
        => Task.FromResult(Guid.NewGuid().ToString());
    public Task<string> SchedulePendingWalletProjectionReconciliationAsync(DateTimeOffset RunAtUtc, CancellationToken cancellationToken)
        => Task.FromResult(Guid.NewGuid().ToString());
    public Task<string> ScheduleOutboxDispatchAsync(int batchSize, DateTimeOffset RunAtUtc, CancellationToken cancellationToken)
        => Task.FromResult(Guid.NewGuid().ToString());
}
