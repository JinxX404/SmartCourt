using FluentValidation;
using SmartCourt.Features.Articles.DTOs;

namespace SmartCourt.Features.Articles.Validators;

public class CreateCommentRequestValidator : AbstractValidator<CreateCommentRequest>
{
    public CreateCommentRequestValidator()
    {
        RuleFor(x => x.Content)
            .NotEmpty().WithMessage("محتوى التعليق مطلوب.")
            .MaximumLength(1000).WithMessage("التعليق يجب ألا يتجاوز 1000 حرف.");
    }
}
