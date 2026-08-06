using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartCourt.Common.Models;
using SmartCourt.Persistence;

namespace SmartCourt.Features.Notifications.MarkNotificationAsRead;

public sealed class MarkNotificationAsReadCommandHandler(ApplicationDbContext context) 
    : IRequestHandler<MarkNotificationAsReadCommand, ApiResponse<bool>>
{
    public async Task<ApiResponse<bool>> Handle(MarkNotificationAsReadCommand request, CancellationToken cancellationToken)
    {
        var notification = await context.Notifications
            .FirstOrDefaultAsync(n => n.Id == request.NotificationId && n.UserId == request.UserId, cancellationToken);

        if (notification == null)
        {
            return ApiResponse<bool>.Fail(["Notification not found."], 404);
        }

        notification.IsRead = true;
        await context.SaveChangesAsync(cancellationToken);

        return ApiResponse<bool>.Ok(true);
    }
}
