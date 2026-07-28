using FluentValidation;
using SmartCourt.Features.DocumentReview.DTOs;

namespace SmartCourt.Features.DocumentReview.Validators;

public class AnalyzeTextRequestValidator : AbstractValidator<AnalyzeTextRequest>
{
    public AnalyzeTextRequestValidator()
    {
        RuleFor(x => x.Text)
            .NotEmpty().WithMessage("Text is required.")
            .MaximumLength(100000).WithMessage("Text is too long (maximum 100,000 characters).");

        RuleFor(x => x.Query)
            .NotEmpty().WithMessage("Query is required.")
            .MaximumLength(2000).WithMessage("Query is too long (maximum 2000 characters).");
    }
}
