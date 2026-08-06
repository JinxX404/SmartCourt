namespace SmartCourt.Features.Notifications.Hubs;

public interface INotificationClient
{
    Task ReceiveNotification(string title, string message, DateTime createdAt);
}
