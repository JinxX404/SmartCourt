using FluentValidation;
using SmartCourt.Features.Notifications.DTOs;
using SmartCourt.Features.Notifications.Shared;

namespace SmartCourt.Features.Notifications.Validators;

public sealed class GetNotificationsRequestValidator
    : AbstractValidator<GetNotificationsRequest>
{
    public GetNotificationsRequestValidator()
    {
        RuleFor(request => request.PageSize)
            .InclusiveBetween(1, 50);
        RuleFor(request => request.Cursor)
            .Must(cursor => cursor is null
                || NotificationCursor.TryDecode(cursor, out _))
            .WithMessage("Cursor is invalid or unsupported.");
    }
}
