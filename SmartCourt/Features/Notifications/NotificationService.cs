using FluentValidation;
using Microsoft.EntityFrameworkCore;
using SmartCourt.Common.Domain;
using SmartCourt.Common.Exceptions;
using SmartCourt.Features.Notifications.DTOs;
using SmartCourt.Features.Notifications.Realtime;
using SmartCourt.Features.Notifications.Shared;
using SmartCourt.Interfaces;
using SmartCourt.Persistence;
using ValidationException = SmartCourt.Common.Exceptions.ValidationException;

namespace SmartCourt.Features.Notifications;

public sealed class NotificationService(
    ApplicationDbContext dbContext,
    ICurrentUserService currentUserService,
    IValidator<GetNotificationsRequest> requestValidator,
    INotificationRealtimeNotifier realtimeNotifier,
    TimeProvider timeProvider,
    ILogger<NotificationService> logger) : INotificationService
{
    public async Task<NotificationPageDto> GetAsync(
        GetNotificationsRequest request,
        CancellationToken cancellationToken)
    {
        var validation = await requestValidator.ValidateAsync(
            request,
            cancellationToken);
        if (!validation.IsValid)
        {
            throw new ValidationException(validation.Errors
                .GroupBy(error => error.PropertyName)
                .Select(group => new KeyValuePair<string, string[]>(
                    group.Key,
                    group.Select(error => error.ErrorMessage).ToArray())));
        }

        var userId = RequireUserId();
        var now = timeProvider.GetUtcNow();
        var query = dbContext.Notifications
            .AsNoTracking()
            .Where(notification =>
                notification.RecipientUserId == userId
                && (!notification.ExpiresAtUtc.HasValue
                    || notification.ExpiresAtUtc > now));

        if (request.IsRead.HasValue)
        {
            query = request.IsRead.Value
                ? query.Where(notification => notification.ReadAtUtc.HasValue)
                : query.Where(notification => !notification.ReadAtUtc.HasValue);
        }

        if (request.Cursor is not null
            && NotificationCursor.TryDecode(request.Cursor, out var sequence))
        {
            query = query.Where(notification => notification.Sequence < sequence);
        }

        var notifications = await query
            .OrderByDescending(notification => notification.Sequence)
            .Take(request.PageSize + 1)
            .ToListAsync(cancellationToken);
        var hasMore = notifications.Count > request.PageSize;
        if (hasMore)
        {
            notifications.RemoveAt(notifications.Count - 1);
        }

        var unreadCount = await ActiveUnreadQuery(userId, now)
            .CountAsync(cancellationToken);
        var nextCursor = hasMore && notifications.Count > 0
            ? NotificationCursor.Encode(notifications[^1].Sequence)
            : null;

        return new NotificationPageDto(
            notifications.Select(NotificationMapper.ToDto).ToArray(),
            nextCursor,
            unreadCount);
    }

    public async Task<UnreadNotificationCountDto> GetUnreadCountAsync(
        CancellationToken cancellationToken)
    {
        var userId = RequireUserId();
        var now = timeProvider.GetUtcNow();
        return new UnreadNotificationCountDto(
            await ActiveUnreadQuery(userId, now).CountAsync(cancellationToken));
    }

    public async Task<NotificationDto> MarkReadAsync(
        Guid notificationId,
        CancellationToken cancellationToken)
    {
        if (notificationId == Guid.Empty)
        {
            throw new NotFoundException("Notification", notificationId);
        }

        var userId = RequireUserId();
        var notification = await dbContext.Notifications
            .SingleOrDefaultAsync(item =>
                item.Id == notificationId
                && item.RecipientUserId == userId,
                cancellationToken)
            ?? throw new NotFoundException("Notification", notificationId);

        var changed = notification.MarkRead(
            timeProvider.GetUtcNow());
        if (changed)
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        var now = timeProvider.GetUtcNow();
        var unreadCount = await ActiveUnreadQuery(userId, now)
            .CountAsync(cancellationToken);
        var dto = NotificationMapper.ToDto(notification);
        if (changed)
        {
            await TryBroadcastReadAsync(
                userId,
                new NotificationReadDto(
                    notification.Id,
                    notification.ReadAtUtc!.Value,
                    unreadCount),
                cancellationToken);
        }

        return dto;
    }

    public async Task<NotificationsReadAllDto> MarkAllReadAsync(
        CancellationToken cancellationToken)
    {
        var userId = RequireUserId();
        var readAtUtc = timeProvider.GetUtcNow();
        await dbContext.Notifications
            .Where(notification =>
                notification.RecipientUserId == userId
                && !notification.ReadAtUtc.HasValue)
            .ExecuteUpdateAsync(
                setters => setters.SetProperty(
                    notification => notification.ReadAtUtc,
                    readAtUtc),
                cancellationToken);

        var result = new NotificationsReadAllDto(readAtUtc, 0);
        try
        {
            await realtimeNotifier.NotificationsReadAllAsync(
                userId,
                result,
                cancellationToken);
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            logger.LogWarning(
                exception,
                "Notification read-all update was committed but its real-time broadcast failed for user {UserId}.",
                userId);
        }

        return result;
    }

    private IQueryable<Entities.Notification> ActiveUnreadQuery(
        Guid userId,
        DateTimeOffset nowUtc)
    {
        return dbContext.Notifications
            .AsNoTracking()
            .Where(notification =>
                notification.RecipientUserId == userId
                && !notification.ReadAtUtc.HasValue
                && (!notification.ExpiresAtUtc.HasValue
                    || notification.ExpiresAtUtc > nowUtc));
    }

    private Guid RequireUserId()
    {
        return currentUserService.RequireUserId(
            "Authentication is required to access notifications.");
    }

    private async Task TryBroadcastReadAsync(
        Guid userId,
        NotificationReadDto update,
        CancellationToken cancellationToken)
    {
        try
        {
            await realtimeNotifier.NotificationReadAsync(
                userId,
                update,
                cancellationToken);
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            logger.LogWarning(
                exception,
                "Notification {NotificationId} was marked read but its real-time broadcast failed.",
                update.NotificationId);
        }
    }
}
