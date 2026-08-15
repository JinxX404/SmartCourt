namespace SmartCourt.Features.Notifications.DTOs;

public sealed record NotificationsReadAllDto(
    DateTimeOffset ReadAtUtc,
    int UnreadCount);
