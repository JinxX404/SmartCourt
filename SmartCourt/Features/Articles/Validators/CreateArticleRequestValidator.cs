using FluentValidation;
using SmartCourt.Features.Articles.DTOs;

namespace SmartCourt.Features.Articles.Validators;

public class CreateArticleRequestValidator : AbstractValidator<CreateArticleRequest>
{
    public CreateArticleRequestValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("عنوان المقال مطلوب.")
            .MaximumLength(255).WithMessage("يجب ألا يتجاوز عنوان المقال 255 حرفاً.");

        RuleFor(x => x.CategoryId)
            .NotEmpty().WithMessage("التصنيف مطلوب.");

        // Content is required only if not draft
        RuleFor(x => x.Content)
            .NotEmpty().WithMessage("محتوى المقال مطلوب عند النشر.")
            .When(x => !x.IsDraft);

        RuleFor(x => x.Tags)
            .MaximumLength(500).WithMessage("يجب ألا تتجاوز الوسوم 500 حرف.");

        RuleFor(x => x.FeaturedImageUrl)
            .MaximumLength(1000).WithMessage("رابط الصورة تجاوز الحد المسموح به.");
    }
}
