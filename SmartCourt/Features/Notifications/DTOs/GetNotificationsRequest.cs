namespace SmartCourt.Features.Notifications.DTOs;

public sealed class GetNotificationsRequest
{
    public string? Cursor { get; init; }
    public int PageSize { get; init; } = 20;
    public bool? IsRead { get; init; }
}
