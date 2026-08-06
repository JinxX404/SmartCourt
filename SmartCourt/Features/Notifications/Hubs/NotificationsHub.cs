using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using SmartCourt.Interfaces;

namespace SmartCourt.Features.Notifications.Hubs;

[Authorize]
public sealed class NotificationsHub(
    ICurrentUserService currentUserService) : Hub<INotificationClient>
{
    public override async Task OnConnectedAsync()
    {
        var userId = currentUserService.UserId;
        
        if (userId == Guid.Empty)
        {
            throw new HubException("User not authenticated.");
        }

        await Groups.AddToGroupAsync(Context.ConnectionId, userId.ToString());
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = currentUserService.UserId;
        
        if (userId != Guid.Empty)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, userId.ToString());
        }

        await base.OnDisconnectedAsync(exception);
    }
}
