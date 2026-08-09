namespace SmartCourt.Features.Notifications.DTOs;

public sealed record NotificationsReadAllDto(
    DateTime ReadAtUtc,
    int UnreadCount);
