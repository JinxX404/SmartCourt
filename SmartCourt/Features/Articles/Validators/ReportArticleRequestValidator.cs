using FluentValidation;
using SmartCourt.Features.Articles.DTOs;

namespace SmartCourt.Features.Articles.Validators;

public class ReportArticleRequestValidator : AbstractValidator<ReportArticleRequest>
{
    public ReportArticleRequestValidator()
    {
        RuleFor(x => x.Reason)
            .NotEmpty().WithMessage("سبب البلاغ مطلوب.")
            .MaximumLength(1000).WithMessage("السبب يجب ألا يتجاوز 1000 حرف.");
    }
}
