using MediatR;
using SmartCourt.Common.Models;

namespace SmartCourt.Features.Notifications.GetMyNotifications;

public sealed record GetMyNotificationsQuery(Guid UserId) : IRequest<ApiResponse<GetMyNotificationsResponseDto>>;

public class GetMyNotificationsResponseDto
{
    public List<NotificationDto> Notifications { get; set; } = [];
}

public class NotificationDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; }
}
