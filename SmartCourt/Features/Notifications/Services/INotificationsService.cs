namespace SmartCourt.Features.Notifications.Services;

public interface INotificationsService
{
    Task SendNotificationAsync(Guid userId, string title, string message, CancellationToken cancellationToken = default);
}
