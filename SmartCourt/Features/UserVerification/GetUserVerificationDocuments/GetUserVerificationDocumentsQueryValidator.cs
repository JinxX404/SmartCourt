namespace SmartCourt.Features.UserVerification.GetUserVerificationDocuments
{
    using FluentValidation;

    public sealed class GetUserVerificationDocumentsQueryValidator : AbstractValidator<GetUserVerificationDocumentsQuery>
    {
        public GetUserVerificationDocumentsQueryValidator()
        {
            RuleFor(x => x.UserId)
                .NotEmpty()
                .WithMessage("User Id is required.");
        }
    }
}
