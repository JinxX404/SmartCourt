using FluentValidation;

namespace SmartCourt.Features.UserVerification.SubmitVerificationDocuments
{
    public class SubmitVerificationDocumentsCommandValidator : AbstractValidator<SubmitVerificationDocumentsCommand>
    {
        public SubmitVerificationDocumentsCommandValidator()
        {
            RuleFor(c => c.UserId)
                .NotEmpty().WithMessage("UserId is required");

            RuleFor(c => c.Documents)
                .NotEmpty().WithMessage("Documents are required.");

            RuleFor(x => x.Documents)
            .Must(documents => documents
            .Select(d => d.Type)
            .Distinct()
            .Count() == documents.Count)
            .WithMessage("The same verification document cannot be submitted more than once.");
        }
    }
}
