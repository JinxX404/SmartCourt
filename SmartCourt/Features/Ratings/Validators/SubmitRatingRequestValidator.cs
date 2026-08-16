using FluentValidation;
using SmartCourt.Features.Ratings.DTOs;

namespace SmartCourt.Features.Ratings.Validators;

public sealed class SubmitRatingRequestValidator : AbstractValidator<SubmitRatingRequest>
{
    public SubmitRatingRequestValidator()
    {
        RuleFor(x => x.Stars)
            .InclusiveBetween(1, 5)
            .WithMessage("يجب أن يكون التقييم بين 1 و5 نجوم.");

        RuleFor(x => x.Comment)
            .MaximumLength(500)
            .WithMessage("يجب ألا يتجاوز التعليق 500 حرف.");
    }
}
