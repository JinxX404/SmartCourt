using FluentValidation;
using SmartCourt.Features.DocumentReview.DTOs;

namespace SmartCourt.Features.DocumentReview.Validators;

public class AskLawRequestValidator : AbstractValidator<AskLawRequest>
{
    public AskLawRequestValidator()
    {
        RuleFor(x => x.Query)
            .NotEmpty().WithMessage("Query is required.")
            .MaximumLength(2000).WithMessage("Query is too long (maximum 2000 characters).");
    }
}
