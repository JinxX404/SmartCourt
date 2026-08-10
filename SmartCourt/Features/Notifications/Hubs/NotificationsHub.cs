using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace SmartCourt.Features.Notifications.Hubs;

[Authorize]
public sealed class NotificationsHub : Hub<INotificationClient>
{
}
