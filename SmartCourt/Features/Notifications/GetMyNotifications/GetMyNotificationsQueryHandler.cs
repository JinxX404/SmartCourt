using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartCourt.Common.Models;
using SmartCourt.Persistence;

namespace SmartCourt.Features.Notifications.GetMyNotifications;

public sealed class GetMyNotificationsQueryHandler(ApplicationDbContext context) 
    : IRequestHandler<GetMyNotificationsQuery, ApiResponse<GetMyNotificationsResponseDto>>
{
    public async Task<ApiResponse<GetMyNotificationsResponseDto>> Handle(GetMyNotificationsQuery request, CancellationToken cancellationToken)
    {
        var notifications = await context.Notifications
            .AsNoTracking()
            .Where(n => n.UserId == request.UserId)
            .OrderByDescending(n => n.CreatedAt)
            .Take(50)
            .Select(n => new NotificationDto
            {
                Id = n.Id,
                Title = n.Title,
                Message = n.Message,
                IsRead = n.IsRead,
                CreatedAt = n.CreatedAt
            })
            .ToListAsync(cancellationToken);

        return ApiResponse<GetMyNotificationsResponseDto>.Ok(new GetMyNotificationsResponseDto
        {
            Notifications = notifications
        });
    }
}
