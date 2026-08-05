using Microsoft.EntityFrameworkCore;
using SmartCourt.Common.Exceptions;
using SmartCourt.Infrastructure.Idempotency;
using SmartCourt.Infrastructure.Persistence.Enums;
using SmartCourt.Persistence;
using Xunit;

namespace SmartCourt.Tests.Infrastructure.Idempotency;

public sealed class IdempotencyServiceIntegrationTests : IAsyncLifetime
{
    private readonly string _databaseName =
        $"SmartCourtIdempotencyTests_{Guid.NewGuid():N}";
    private readonly Guid _userId = Guid.NewGuid();
    private readonly DateTime _initialUtc =
        new(2026, 8, 1, 12, 0, 0, DateTimeKind.Utc);

    public async Task InitializeAsync()
    {
        await using var context = CreateContext(_initialUtc);
        await context.Database.EnsureCreatedAsync();
        context.Users.Add(new ApplicationUser
        {
            Id = _userId,
            UserName = "idempotency-test@example.test",
            NormalizedUserName = "IDEMPOTENCY-TEST@EXAMPLE.TEST",
            Email = "idempotency-test@example.test",
            NormalizedEmail = "IDEMPOTENCY-TEST@EXAMPLE.TEST",
            FullName = "Idempotency Test",
            NationalNumber = "12345678901234",
            SecurityStamp = Guid.NewGuid().ToString()
        });
        await context.SaveChangesAsync();
    }

    public async Task DisposeAsync()
    {
        await using var context = CreateContext(_initialUtc);
        await context.Database.EnsureDeletedAsync();
    }

    [Fact]
    public async Task CompletedRequest_IsReplayedWithStoredResponse()
    {
        var scope = NewScope("fund", "Milestone");
        var request = new { amount = 100m, currency = "EGP" };

        await using (var context = CreateContext(_initialUtc))
        {
            var service = CreateService(context, _initialUtc);
            var reservation = await service.ReserveAsync(
                scope,
                "funding-key",
                request,
                CancellationToken.None);

            Assert.False(reservation.IsReplay);
            await service.CompleteAsync(
                reservation.RecordId,
                200,
                new { transactionId = "tx-1" },
                Guid.NewGuid(),
                CancellationToken.None);
        }

        await using var replayContext = CreateContext(_initialUtc);
        var replayService = CreateService(replayContext, _initialUtc);
        var replay = await replayService.ReserveAsync(
            scope,
            "funding-key",
            request,
            CancellationToken.None);

        Assert.True(replay.IsReplay);
        Assert.Equal(IdempotencyStatus.Completed, replay.Status);
        Assert.Equal(200, replay.ResponseStatusCode);
        Assert.Contains("tx-1", replay.ResponseBody);
    }

    [Fact]
    public async Task ReusingKeyWithChangedPayload_ThrowsBusinessConflict()
    {
        var scope = NewScope("fund", "Milestone");
        await using var context = CreateContext(_initialUtc);
        var service = CreateService(context, _initialUtc);

        await service.ReserveAsync(
            scope,
            "same-key",
            new { amount = 100m, currency = "EGP" },
            CancellationToken.None);

        var exception = await Assert.ThrowsAsync<BusinessException>(
            () => service.ReserveAsync(
                scope,
                "same-key",
                new { amount = 101m, currency = "EGP" },
                CancellationToken.None));

        Assert.Contains("بيانات مختلفة", exception.Message);
    }

    [Fact]
    public async Task DifferentHttpKeys_CannotReserveTheSameHoldSettlement()
    {
        var holdId = Guid.NewGuid();
        var firstScope = IdempotencyScope.ForHoldSettlement(
            _userId,
            "release",
            holdId);
        var secondScope = IdempotencyScope.ForHoldSettlement(
            _userId,
            "refund",
            holdId);

        await using var context = CreateContext(_initialUtc);
        var service = CreateService(context, _initialUtc);
        await service.ReserveAsync(
            firstScope,
            "release-http-key",
            new { amount = 100m },
            CancellationToken.None);

        var exception = await Assert.ThrowsAsync<BusinessException>(
            () => service.ReserveAsync(
                secondScope,
                "refund-http-key",
                new { amount = 100m },
                CancellationToken.None));

        Assert.Contains("طلب تسوية محفوظ", exception.Message);
    }

    [Fact]
    public async Task ConcurrentDuplicateReservations_HaveOneWinner()
    {
        var scope = NewScope("fund", "Milestone");
        var request = new { amount = 100m, currency = "EGP" };

        var first = ReserveWithNewContextAsync(
            scope,
            "concurrent-key",
            request);
        var second = ReserveWithNewContextAsync(
            scope,
            "concurrent-key",
            request);
        var results = await Task.WhenAll(first, second);

        Assert.Equal(
            1,
            results.Count(result => result.Reservation is not null));
        Assert.Equal(
            1,
            results.Count(result => result.Exception is not null));
        Assert.IsType<BusinessException>(
            results.Single(result => result.Exception is not null).Exception);
    }

    [Fact]
    public async Task Purge_RemovesOnlyExpiredResponseBodyAndRetainsFinancialRecord()
    {
        var scope = IdempotencyScope.ForHoldSettlement(
            _userId,
            "release",
            Guid.NewGuid());
        var request = new { amount = 100m };

        await using (var context = CreateContext(_initialUtc))
        {
            var service = CreateService(context, _initialUtc);
            var reservation = await service.ReserveAsync(
                scope,
                "retention-key",
                request,
                CancellationToken.None);
            await service.CompleteAsync(
                reservation.RecordId,
                200,
                new { transactionId = "tx-retained" },
                Guid.NewGuid(),
                CancellationToken.None);
        }

        var futureUtc = _initialUtc.AddDays(31);
        await using var purgeContext = CreateContext(futureUtc);
        var purgeService = CreateService(purgeContext, futureUtc);
        var purged = await purgeService.PurgeExpiredResponseBodiesAsync(
            CancellationToken.None);

        Assert.Equal(1, purged);
        var record = await purgeContext.IdempotencyRecords
            .SingleAsync();
        Assert.Equal(IdempotencyStatus.Completed, record.Status);
        Assert.Null(record.ResponseBody);
        Assert.NotNull(record.CompletedAt);
    }

    private IdempotencyScope NewScope(
        string operation,
        string resourceType)
    {
        return new IdempotencyScope(
            _userId,
            operation,
            resourceType,
            Guid.NewGuid());
    }

    private async Task<ReserveResult> ReserveWithNewContextAsync<TRequest>(
        IdempotencyScope scope,
        string key,
        TRequest request)
    {
        try
        {
            await using var context = CreateContext(_initialUtc);
            var service = CreateService(context, _initialUtc);
            return new ReserveResult(
                await service.ReserveAsync(
                    scope,
                    key,
                    request,
                    CancellationToken.None),
                null);
        }
        catch (Exception exception)
        {
            return new ReserveResult(null, exception);
        }
    }

    private ApplicationDbContext CreateContext(DateTime utcNow)
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlServer(ConnectionString)
            .Options;
        return new ApplicationDbContext(
            options,
            new FixedTimeProvider(utcNow));
    }

    private IdempotencyService CreateService(
        ApplicationDbContext context,
        DateTime utcNow)
    {
        return new IdempotencyService(
            context,
            new CanonicalIdempotencyRequestHasher(),
            new FixedTimeProvider(utcNow));
    }

    private string ConnectionString =>
        Environment.GetEnvironmentVariable("TEST_DB_CONNECTION_STRING")
        ?? $"Server=(localdb)\\mssqllocaldb;Database={_databaseName};Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=True;";

    private sealed record ReserveResult(
        IdempotencyReservation? Reservation,
        Exception? Exception);

    private sealed class FixedTimeProvider(DateTime utcNow)
        : TimeProvider
    {
        public override DateTimeOffset GetUtcNow()
        {
            return new DateTimeOffset(utcNow);
        }
    }
}
