using SmartCourt.Features.Notifications.DTOs;

namespace SmartCourt.Features.Notifications.Realtime;

public interface INotificationRealtimeNotifier
{
    Task NotificationCreatedAsync(
        Guid userId,
        NotificationDto notification,
        CancellationToken cancellationToken);

    Task NotificationReadAsync(
        Guid userId,
        NotificationReadDto update,
        CancellationToken cancellationToken);

    Task NotificationsReadAllAsync(
        Guid userId,
        NotificationsReadAllDto update,
        CancellationToken cancellationToken);
}
