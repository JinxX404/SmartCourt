using FluentValidation;
using SmartCourt.Features.Users.Lawyers.Dashboard.DTOs;

namespace SmartCourt.Features.Users.Lawyers.Dashboard.Validators;

public sealed class LawyerDeadlinesQueryValidator : AbstractValidator<LawyerDeadlinesQuery>
{
    public LawyerDeadlinesQueryValidator()
    {
        RuleFor(x => x.DaysAhead)
            .InclusiveBetween(1, 90)
            .WithMessage("يجب أن تكون فترة الأيام القادمة بين 1 و 90 يوماً.");
    }
}
