using Microsoft.EntityFrameworkCore;
using SmartCourt.Features.Notifications.Entities;
using SmartCourt.Features.Notifications.Realtime;
using SmartCourt.Features.Notifications.Shared;
using SmartCourt.Infrastructure.Persistence.Entities;
using SmartCourt.Infrastructure.Providers.Events;
using SmartCourt.Persistence;

namespace SmartCourt.Features.Notifications.Events;

internal sealed class NotificationOutboxHandler : IOutboxEventHandler
{
    private readonly ApplicationDbContext _dbContext;
    private readonly INotificationRealtimeNotifier _realtimeNotifier;
    private readonly IReadOnlyDictionary<string, INotificationEventMapper> _mappers;

    public NotificationOutboxHandler(
        ApplicationDbContext dbContext,
        INotificationRealtimeNotifier realtimeNotifier,
        IEnumerable<INotificationEventMapper> mappers)
    {
        _dbContext = dbContext;
        _realtimeNotifier = realtimeNotifier;

        var registrations = mappers
            .SelectMany(mapper => mapper.EventTypes.Select(eventType => new
            {
                EventType = eventType,
                Mapper = mapper
            }))
            .ToArray();
        var duplicate = registrations
            .GroupBy(item => item.EventType, StringComparer.Ordinal)
            .FirstOrDefault(group => group.Count() > 1);
        if (duplicate is not null)
        {
            throw new InvalidOperationException(
                $"Multiple notification mappers are registered for '{duplicate.Key}'.");
        }

        _mappers = registrations.ToDictionary(
            item => item.EventType,
            item => item.Mapper,
            StringComparer.Ordinal);
    }

    public IReadOnlyCollection<string> EventTypes => _mappers.Keys.ToArray();

    public async Task HandleAsync(
        OutboxMessage message,
        CancellationToken cancellationToken)
    {
        if (!_mappers.TryGetValue(message.EventType, out var mapper))
        {
            throw new InvalidOperationException(
                $"No notification mapper is registered for '{message.EventType}'.");
        }

        var drafts = await mapper.MapAsync(message, cancellationToken);
        EnsureUniqueDrafts(drafts);

        var notifications = new List<Notification>(drafts.Count);
        var newNotifications = new List<Notification>(drafts.Count);
        var createdAtUtc = message.CreatedAt;
        foreach (var draft in drafts)
        {
            var notification = await _dbContext.Notifications
                .SingleOrDefaultAsync(item =>
                    item.SourceEventId == message.Id
                    && item.RecipientUserId == draft.RecipientUserId
                    && item.Type == draft.Type,
                    cancellationToken);
            if (notification is null)
            {
                notification = Notification.Create(
                    Guid.NewGuid(),
                    draft.RecipientUserId,
                    message.Id,
                    draft.Type,
                    draft.Severity,
                    draft.Title,
                    draft.Body,
                    draft.ActionUrl,
                    draft.Data is null
                        ? null
                        : NotificationJson.Serialize(draft.Data),
                    createdAtUtc,
                    draft.ExpiresAtUtc);
                newNotifications.Add(notification);
            }

            notifications.Add(notification);
        }

        if (newNotifications.Count > 0)
        {
            _dbContext.Notifications.AddRange(newNotifications);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        foreach (var notification in notifications)
        {
            await _realtimeNotifier.NotificationCreatedAsync(
                notification.RecipientUserId,
                NotificationMapper.ToDto(notification),
                cancellationToken);
        }
    }

    private static void EnsureUniqueDrafts(
        IReadOnlyCollection<NotificationDraft> drafts)
    {
        var duplicate = drafts
            .GroupBy(
                draft => (draft.RecipientUserId, draft.Type),
                EqualityComparer<(Guid, string)>.Default)
            .FirstOrDefault(group => group.Count() > 1);
        if (duplicate is not null)
        {
            throw new InvalidOperationException(
                "A notification mapper returned duplicate recipient and type drafts.");
        }
    }
}
