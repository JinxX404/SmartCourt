using SmartCourt.Infrastructure.Persistence.Entities;

namespace SmartCourt.Features.Notifications.Events;

internal interface INotificationEventMapper
{
    IReadOnlyCollection<string> EventTypes { get; }

    Task<IReadOnlyCollection<NotificationDraft>> MapAsync(
        OutboxMessage message,
        CancellationToken cancellationToken);
}
