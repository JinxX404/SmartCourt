using FluentValidation;
using SmartCourt.Features.Users.Lawyers.Dashboard.DTOs;

namespace SmartCourt.Features.Users.Lawyers.Dashboard.Validators;

public sealed class LawyerCalendarQueryValidator : AbstractValidator<LawyerCalendarQuery>
{
    public LawyerCalendarQueryValidator()
    {
        RuleFor(x => x)
            .Must(x => !x.FromUtc.HasValue || !x.ToUtc.HasValue || x.FromUtc.Value < x.ToUtc.Value)
            .WithMessage("يجب أن يكون تاريخ البداية قبل تاريخ النهاية.");

        RuleFor(x => x)
            .Must(x => !x.FromUtc.HasValue || !x.ToUtc.HasValue || (x.ToUtc.Value - x.FromUtc.Value).TotalDays <= 60)
            .WithMessage("يجب ألا تتجاوز الفترة المحددة 60 يوماً.");
    }
}
