using FluentValidation;

namespace SmartCourt.Features.Admin.Verifications.GetVerificationDetails;

public sealed class GetVerificationDetailsQueryValidator : AbstractValidator<GetVerificationDetailsQuery>
{
    public GetVerificationDetailsQueryValidator()
    {
        RuleFor(query => query.LawyerId)
            .NotEmpty();
    }
}
