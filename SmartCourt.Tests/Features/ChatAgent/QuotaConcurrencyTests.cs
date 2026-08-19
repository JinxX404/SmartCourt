using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using SmartCourt.Common.Configuration;
using SmartCourt.Common.Exceptions;
using SmartCourt.Features.ChatAgent;
using SmartCourt.Features.ChatAgent.Entities;
using SmartCourt.Persistence;
using Xunit;

namespace SmartCourt.Tests.Features.ChatAgent;

public class QuotaConcurrencyTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly ApplicationDbContext _dbContext;
    private readonly QuotaService _quotaService;
    private readonly TimeProvider _timeProvider;
    private readonly IOptions<QuotaOptions> _quotaOptions;

    public QuotaConcurrencyTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlite(_connection)
            .Options;

        _dbContext = new ApplicationDbContext(options);
        _dbContext.Database.EnsureCreated();

        _timeProvider = TimeProvider.System;
        _quotaOptions = Options.Create(new QuotaOptions { DailyFreeTokens = 1000, Timezone = "Egypt Standard Time" });
        _quotaService = new QuotaService(_dbContext, _timeProvider, NullLogger<QuotaService>.Instance, _quotaOptions);
    }

    [Fact]
    public async Task ReserveQuotaAsync_ConcurrentRequests_CannotExceedDailyLimit()
    {
        // Arrange
        var clientId = Guid.NewGuid();
        int dailyLimit = 1000;
        
        _dbContext.QuotaProfiles.Add(QuotaProfile.Create(clientId, dailyLimit)); 
        await _dbContext.SaveChangesAsync();

        var tz = TimeZoneInfo.FindSystemTimeZoneById("Egypt Standard Time");
        var midnight = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, tz).Date;
        var today = new DateTimeOffset(midnight, tz.GetUtcOffset(midnight));
        var usage = DailyUsage.Create(clientId, today);
        _dbContext.DailyUsages.Add(usage);
        await _dbContext.SaveChangesAsync();

        // Act
        // Attempt 10 concurrent requests of 150 tokens each (Total requested = 1500 tokens)
        // With an initial limit of 1000 tokens, maximum 6 requests should succeed (6 * 150 = 900 tokens).
        // 4 requests should fail with InsufficientQuotaException.
        int requestAmount = 150;
        int concurrentRequestsCount = 10;
        int successCount = 0;
        int failureCount = 0;

        var tasks = Enumerable.Range(0, concurrentRequestsCount).Select(async _ =>
        {
            try
            {
                var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                    .UseSqlite(_connection) 
                    .Options;
                using var scopedContext = new ApplicationDbContext(options);
                var scopedService = new QuotaService(scopedContext, _timeProvider, NullLogger<QuotaService>.Instance, _quotaOptions);
                
                await scopedService.ReserveQuotaAsync(clientId, requestAmount, CancellationToken.None);
                Interlocked.Increment(ref successCount);
            }
            catch (Exception)
            {
                // Expecting InsufficientQuotaException or DbUpdateException due to concurrent ExecuteUpdateAsync on SQLite.
                Interlocked.Increment(ref failureCount);
            }
        });

        await Task.WhenAll(tasks);

        // Assert
        Assert.True(successCount <= 6, $"Expected maximum 6 successes, but got {successCount}");
        
        var finalUsage = await _dbContext.DailyUsages.AsNoTracking().FirstOrDefaultAsync(x => x.ClientId == clientId && x.UsageDate == today);
        Assert.NotNull(finalUsage);
        Assert.True(finalUsage.ConsumedTokens <= dailyLimit, $"Daily limit exceeded! Consumed: {finalUsage.ConsumedTokens}");
        
        int expectedSpent = successCount * requestAmount;
        Assert.Equal(expectedSpent, finalUsage.ConsumedTokens);
    }



    public void Dispose()
    {
        _dbContext.Dispose();
        _connection.Dispose();
    }
}
