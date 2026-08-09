namespace SmartCourt.Features.Notifications.DTOs;

public sealed record NotificationReadDto(
    Guid NotificationId,
    DateTime ReadAtUtc,
    int UnreadCount);
