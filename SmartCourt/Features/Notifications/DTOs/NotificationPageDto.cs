namespace SmartCourt.Features.Notifications.DTOs;

public sealed record NotificationPageDto(
    IReadOnlyList<NotificationDto> Items,
    string? NextCursor,
    int UnreadCount);
