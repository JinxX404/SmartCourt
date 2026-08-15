using FluentValidation;
using Microsoft.AspNetCore.Http;
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
            .MaximumLength(5000).WithMessage("يجب ألا يتجاوز محتوى المقال 5000 حرف.")
            .When(x => !x.IsDraft);

        RuleFor(x => x.Tags)
            .MaximumLength(500).WithMessage("يجب ألا تتجاوز الوسوم 500 حرف.");

        RuleFor(x => x.FeaturedImage)
            .Must(f => f == null || f.Length <= 5 * 1024 * 1024)
            .WithMessage("حجم الصورة يجب ألا يتجاوز 5 ميجابايت.")
            .Must(f => f == null || IsAllowedImageExtension(f.FileName))
            .WithMessage("صيغة الصورة غير مدعومة. الصيغ المسموحة: jpg, jpeg, png, webp.");
    }

    private static bool IsAllowedImageExtension(string fileName)
    {
        var ext = System.IO.Path.GetExtension(fileName).ToLowerInvariant();
        return ext is ".jpg" or ".jpeg" or ".png" or ".webp";
    }
}
