using FluentValidation;
using SmartCourt.Features.Admin.Quotas.DTOs;

namespace SmartCourt.Features.Admin.Quotas.Validators;

public class AdjustQuotaRequestValidator : AbstractValidator<AdjustQuotaRequest>
{
    public AdjustQuotaRequestValidator()
    {
        RuleFor(x => x.CreditAmount)
            .NotEqual(0).WithMessage("يجب أن تكون القيمة المضافة أو المخصومة مختلفة عن الصفر.");

        RuleFor(x => x.Reason)
            .NotEmpty().WithMessage("يجب توضيح سبب التعديل.");
    }
}
