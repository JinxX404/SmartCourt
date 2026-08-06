using Microsoft.AspNetCore.SignalR;
using SmartCourt.Entities;
using SmartCourt.Features.Notifications.Hubs;
using SmartCourt.Persistence;

namespace SmartCourt.Features.Notifications.Services;

public sealed class NotificationsService(
    ApplicationDbContext context,
    IHubContext<NotificationsHub, INotificationClient> hubContext) : INotificationsService
{
    public async Task SendNotificationAsync(Guid userId, string title, string message, CancellationToken cancellationToken = default)
    {
        var notification = new Notification
        {
            UserId = userId,
            Title = title,
            Message = message,
            CreatedAt = DateTime.UtcNow,
            IsRead = false
        };

        context.Notifications.Add(notification);
        await context.SaveChangesAsync(cancellationToken);

        await hubContext.Clients.Group(userId.ToString())
            .ReceiveNotification(title, message, notification.CreatedAt);
    }
}
