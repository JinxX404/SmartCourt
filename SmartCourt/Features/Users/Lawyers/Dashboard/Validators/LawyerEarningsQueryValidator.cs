using FluentValidation;
using SmartCourt.Features.Users.Lawyers.Dashboard.DTOs;

namespace SmartCourt.Features.Users.Lawyers.Dashboard.Validators;

public sealed class LawyerEarningsQueryValidator : AbstractValidator<LawyerEarningsQuery>
{
    private static readonly string[] AllowedPeriods = ["3months", "6months", "1year"];
    private static readonly string[] AllowedGroupBy = ["monthly", "weekly"];

    public LawyerEarningsQueryValidator()
    {
        RuleFor(x => x.Period)
            .Must(p => string.IsNullOrWhiteSpace(p) || AllowedPeriods.Contains(p.Trim().ToLowerInvariant()))
            .WithMessage("الفترة غير صالحة. الفترات المسموحة: 3months, 6months, 1year.");

        RuleFor(x => x.GroupBy)
            .Must(g => string.IsNullOrWhiteSpace(g) || AllowedGroupBy.Contains(g.Trim().ToLowerInvariant()))
            .WithMessage("طريقة التجميع غير صالحة. الخيارات المسموحة: monthly, weekly.");
    }
}
