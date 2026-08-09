using SmartCourt.Features.Notifications.DTOs;

namespace SmartCourt.Features.Notifications.Hubs;

public interface INotificationClient
{
    Task NotificationCreated(NotificationDto notification);
    Task NotificationRead(NotificationReadDto update);
    Task NotificationsReadAll(NotificationsReadAllDto update);
}
