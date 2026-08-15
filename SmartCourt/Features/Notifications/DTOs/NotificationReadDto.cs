namespace SmartCourt.Features.Notifications.DTOs;

public sealed record NotificationReadDto(
    Guid NotificationId,
    DateTimeOffset ReadAtUtc,
    int UnreadCount);
