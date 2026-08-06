using MediatR;
using SmartCourt.Common.Models;

namespace SmartCourt.Features.Notifications.MarkNotificationAsRead;

public sealed record MarkNotificationAsReadCommand(Guid UserId, Guid NotificationId) : IRequest<ApiResponse<bool>>;
