using FluentValidation;
using SmartCourt.Features.Admin.Quotas.DTOs;

namespace SmartCourt.Features.Admin.Quotas.Validators;

public class UpdateDailyLimitRequestValidator : AbstractValidator<UpdateDailyLimitRequest>
{
    public UpdateDailyLimitRequestValidator()
    {
        RuleFor(x => x.DailyCreditLimit)
            .GreaterThanOrEqualTo(0).WithMessage("الحد اليومي لا يمكن أن يكون أقل من صفر.");
    }
}
