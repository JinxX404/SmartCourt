using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartCourt.Interfaces;
using SmartCourt.Features.Notifications.GetMyNotifications;
using SmartCourt.Features.Notifications.MarkNotificationAsRead;

namespace SmartCourt.Features.Notifications;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class NotificationsController(IMediator mediator, ICurrentUserService currentUserService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetMyNotifications()
    {
        var result = await mediator.Send(new GetMyNotificationsQuery(currentUserService.UserId.Value));
        return Ok(result);
    }

    [HttpPatch("{notificationId}/read")]
    public async Task<IActionResult> MarkAsRead(Guid notificationId)
    {
        var result = await mediator.Send(new MarkNotificationAsReadCommand(currentUserService.UserId.Value, notificationId));
        return Ok(result);
    }
}
