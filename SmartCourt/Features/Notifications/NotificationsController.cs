using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartCourt.Common.Models;
using SmartCourt.Features.Notifications.DTOs;

namespace SmartCourt.Features.Notifications;

[ApiController]
[Authorize]
[Route("api/notifications")]
[Produces("application/json")]
public sealed class NotificationsController(INotificationService service)
    : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<ApiResponse<NotificationPageDto>>> GetAsync(
        [FromQuery] GetNotificationsRequest request,
        CancellationToken cancellationToken)
    {
        return Ok(ApiResponse<NotificationPageDto>.Ok(
            await service.GetAsync(request, cancellationToken)));
    }

    [HttpGet("unread-count")]
    public async Task<ActionResult<ApiResponse<UnreadNotificationCountDto>>>
        GetUnreadCountAsync(CancellationToken cancellationToken)
    {
        return Ok(ApiResponse<UnreadNotificationCountDto>.Ok(
            await service.GetUnreadCountAsync(cancellationToken)));
    }

    [HttpPatch("{notificationId:guid}/read")]
    public async Task<ActionResult<ApiResponse<NotificationDto>>> MarkReadAsync(
        Guid notificationId,
        CancellationToken cancellationToken)
    {
        return Ok(ApiResponse<NotificationDto>.Ok(
            await service.MarkReadAsync(notificationId, cancellationToken)));
    }

    [HttpPatch("read-all")]
    public async Task<ActionResult<ApiResponse<NotificationsReadAllDto>>>
        MarkAllReadAsync(CancellationToken cancellationToken)
    {
        return Ok(ApiResponse<NotificationsReadAllDto>.Ok(
            await service.MarkAllReadAsync(cancellationToken)));
    }
}
