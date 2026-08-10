using SmartCourt.Features.Notifications.DTOs;

namespace SmartCourt.Features.Notifications;

public interface INotificationService
{
    Task<NotificationPageDto> GetAsync(
        GetNotificationsRequest request,
        CancellationToken cancellationToken);

    Task<UnreadNotificationCountDto> GetUnreadCountAsync(
        CancellationToken cancellationToken);

    Task<NotificationDto> MarkReadAsync(
        Guid notificationId,
        CancellationToken cancellationToken);

    Task<NotificationsReadAllDto> MarkAllReadAsync(
        CancellationToken cancellationToken);
}
