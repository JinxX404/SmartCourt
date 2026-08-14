using FluentValidation;
using SmartCourt.Features.Articles.DTOs;

namespace SmartCourt.Features.Articles.Validators;

public class UpdateCategoryRequestValidator : AbstractValidator<UpdateCategoryRequest>
{
    public UpdateCategoryRequestValidator()
    {
        RuleFor(x => x.NameAr)
            .NotEmpty().WithMessage("الاسم العربي للتصنيف مطلوب.")
            .MaximumLength(100).WithMessage("يجب ألا يتجاوز الاسم 100 حرف.");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("يجب ألا يتجاوز الوصف 500 حرف.");
    }
}
