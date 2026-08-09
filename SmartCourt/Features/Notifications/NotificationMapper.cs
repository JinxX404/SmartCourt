using SmartCourt.Features.Notifications.DTOs;
using SmartCourt.Features.Notifications.Entities;
using SmartCourt.Features.Notifications.Shared;

namespace SmartCourt.Features.Notifications;

internal static class NotificationMapper
{
    public static NotificationDto ToDto(Notification notification)
    {
        return new NotificationDto(
            notification.Id,
            notification.Type,
            notification.Severity.ToString(),
            notification.Title,
            notification.Body,
            notification.ActionUrl,
            NotificationJson.Deserialize(notification.DataJson),
            notification.CreatedAtUtc,
            notification.ReadAtUtc,
            notification.ExpiresAtUtc);
    }
}
