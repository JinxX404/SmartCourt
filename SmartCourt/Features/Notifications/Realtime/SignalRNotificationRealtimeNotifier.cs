using Microsoft.AspNetCore.SignalR;
using SmartCourt.Features.Notifications.DTOs;
using SmartCourt.Features.Notifications.Hubs;

namespace SmartCourt.Features.Notifications.Realtime;

public sealed class SignalRNotificationRealtimeNotifier(
    IHubContext<NotificationsHub, INotificationClient> hubContext)
    : INotificationRealtimeNotifier
{
    public Task NotificationCreatedAsync(
        Guid userId,
        NotificationDto notification,
        CancellationToken cancellationToken)
    {
        return hubContext.Clients.User(userId.ToString())
            .NotificationCreated(notification);
    }

    public Task NotificationReadAsync(
        Guid userId,
        NotificationReadDto update,
        CancellationToken cancellationToken)
    {
        return hubContext.Clients.User(userId.ToString())
            .NotificationRead(update);
    }

    public Task NotificationsReadAllAsync(
        Guid userId,
        NotificationsReadAllDto update,
        CancellationToken cancellationToken)
    {
        return hubContext.Clients.User(userId.ToString())
            .NotificationsReadAll(update);
    }
}
