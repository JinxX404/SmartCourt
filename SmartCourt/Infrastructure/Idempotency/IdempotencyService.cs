using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using SmartCourt.Common.Exceptions;
using SmartCourt.Infrastructure.Persistence.Entities;
using SmartCourt.Infrastructure.Persistence.Enums;
using SmartCourt.Persistence;

namespace SmartCourt.Infrastructure.Idempotency;

public sealed class IdempotencyService : IIdempotencyService
{
    private const int ResponseRetentionDays = 30;
    private const int MaximumResponseBodyLength = 20_000;

    private static readonly JsonSerializerOptions SerializerOptions =
        new(JsonSerializerDefaults.Web);

    private readonly ApplicationDbContext _dbContext;
    private readonly IIdempotencyRequestHasher _requestHasher;
    private readonly TimeProvider _timeProvider;

    public IdempotencyService(
        ApplicationDbContext dbContext,
        IIdempotencyRequestHasher requestHasher,
        TimeProvider timeProvider)
    {
        _dbContext = dbContext;
        _requestHasher = requestHasher;
        _timeProvider = timeProvider;
    }

    public async Task<IdempotencyReservation> ReserveAsync<TRequest>(
        IdempotencyScope scope,
        string? idempotencyKey,
        TRequest request,
        CancellationToken cancellationToken)
    {
        var key = IdempotencyHeader.Require(idempotencyKey);
        var requestHash = _requestHasher.ComputeHash(scope, request);

        await using var transaction =
            await _dbContext.Database.BeginTransactionAsync(cancellationToken);

        var existingByKey = await FindByKeyAsync(
            scope.UserId,
            key,
            cancellationToken);
        if (existingByKey is not null)
        {
            var reservation = EvaluateExisting(
                existingByKey,
                requestHash);
            await transaction.CommitAsync(cancellationToken);
            return reservation;
        }

        if (scope.ResourceType == IdempotencyScope.HoldSettlementResourceType)
        {
            var existingSettlement = await FindHoldSettlementAsync(
                scope.ResourceId,
                cancellationToken);
            if (existingSettlement is not null)
            {
                throw new BusinessException(
                    "يوجد طلب تسوية محفوظ مسبقًا لحجز الضمان المحدد.");
            }
        }

        var now = _timeProvider.GetUtcNow().UtcDateTime;
        var record = new IdempotencyRecord(
            Guid.NewGuid(),
            scope.UserId,
            key,
            scope.Operation,
            scope.ResourceType,
            scope.ResourceId,
            requestHash,
            now.AddDays(ResponseRetentionDays),
            now);
        _dbContext.IdempotencyRecords.Add(record);

        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return ToReservation(
                record,
                IdempotencyReservationState.Acquired);
        }
        catch (DbUpdateException)
        {
            await transaction.RollbackAsync(cancellationToken);
            _dbContext.ChangeTracker.Clear();

            var concurrentRecord = await FindByKeyAsync(
                scope.UserId,
                key,
                cancellationToken);
            if (concurrentRecord is not null)
            {
                return EvaluateExisting(
                    concurrentRecord,
                    requestHash);
            }

            if (scope.ResourceType
                == IdempotencyScope.HoldSettlementResourceType
                && await FindHoldSettlementAsync(
                    scope.ResourceId,
                    cancellationToken) is not null)
            {
                throw new BusinessException(
                    "يوجد طلب تسوية محفوظ مسبقًا لحجز الضمان المحدد.");
            }

            throw;
        }
    }

    public async Task CompleteAsync<TResponse>(
        Guid recordId,
        int responseStatusCode,
        TResponse response,
        Guid? resultReferenceId,
        CancellationToken cancellationToken)
    {
        await FinishAsync(
            recordId,
            responseStatusCode,
            response,
            resultReferenceId,
            failed: false,
            cancellationToken);
    }

    public async Task FailAsync<TResponse>(
        Guid recordId,
        int responseStatusCode,
        TResponse response,
        Guid? resultReferenceId,
        CancellationToken cancellationToken)
    {
        await FinishAsync(
            recordId,
            responseStatusCode,
            response,
            resultReferenceId,
            failed: true,
            cancellationToken);
    }

    public async Task<int> PurgeExpiredResponseBodiesAsync(
        CancellationToken cancellationToken)
    {
        var now = _timeProvider.GetUtcNow().UtcDateTime;
        var records = await _dbContext.IdempotencyRecords
            .Where(record =>
                record.ResponseBody != null
                && record.ExpiresAt <= now
                && record.Status != IdempotencyStatus.Processing)
            .ToListAsync(cancellationToken);

        foreach (var record in records)
        {
            record.PurgeResponseBody();
        }

        if (records.Count > 0)
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        return records.Count;
    }

    private async Task FinishAsync<TResponse>(
        Guid recordId,
        int responseStatusCode,
        TResponse response,
        Guid? resultReferenceId,
        bool failed,
        CancellationToken cancellationToken)
    {
        if (recordId == Guid.Empty)
        {
            throw new BusinessException(
                "سجل حماية الطلب من التكرار مطلوب.");
        }

        if (responseStatusCode is < 100 or > 599)
        {
            throw new BusinessException(
                "رمز استجابة الطلب المحمي من التكرار غير صالح.");
        }

        var responseBody = JsonSerializer.Serialize(
            response,
            SerializerOptions);
        if (responseBody.Length > MaximumResponseBodyLength)
        {
            throw new BusinessException(
                "حجم استجابة الطلب المحمي من التكرار يتجاوز الحد المسموح بحفظه.");
        }

        var record = await _dbContext.IdempotencyRecords
            .SingleOrDefaultAsync(
                item => item.Id == recordId,
                cancellationToken)
            ?? throw new BusinessException(
                "سجل حماية الطلب من التكرار غير موجود.");

        if (record.Status != IdempotencyStatus.Processing)
        {
            return;
        }

        var now = _timeProvider.GetUtcNow().UtcDateTime;
        if (failed)
        {
            record.Fail(
                responseStatusCode,
                responseBody,
                resultReferenceId,
                now);
        }
        else
        {
            record.Complete(
                responseStatusCode,
                responseBody,
                resultReferenceId,
                now);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task<IdempotencyRecord?> FindByKeyAsync(
        Guid userId,
        string key,
        CancellationToken cancellationToken)
    {
        return await _dbContext.IdempotencyRecords
            .AsNoTracking()
            .SingleOrDefaultAsync(
                record => record.UserId == userId
                    && record.Key == key,
                cancellationToken);
    }

    private async Task<IdempotencyRecord?> FindHoldSettlementAsync(
        Guid resourceId,
        CancellationToken cancellationToken)
    {
        return await _dbContext.IdempotencyRecords
            .AsNoTracking()
            .SingleOrDefaultAsync(
                record =>
                    record.ResourceType
                        == IdempotencyScope.HoldSettlementResourceType
                    && record.ResourceId == resourceId,
                cancellationToken);
    }

    private static IdempotencyReservation EvaluateExisting(
        IdempotencyRecord record,
        string requestHash)
    {
        if (!string.Equals(
                record.RequestHash,
                requestHash,
                StringComparison.Ordinal))
        {
            throw new BusinessException(
                "استُخدم مفتاح الطلب نفسه مسبقًا مع بيانات مختلفة.");
        }

        if (record.Status == IdempotencyStatus.Processing)
        {
            throw new BusinessException(
                "يوجد طلب مطابق قيد المعالجة حاليًا.");
        }

        return ToReservation(
            record,
            IdempotencyReservationState.Replay);
    }

    private static IdempotencyReservation ToReservation(
        IdempotencyRecord record,
        IdempotencyReservationState state)
    {
        return new IdempotencyReservation(
            record.Id,
            state,
            record.RequestHash,
            record.Status,
            record.ResponseStatusCode,
            record.ResponseBody,
            record.ResultReferenceId);
    }
}
