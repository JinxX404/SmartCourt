using FluentValidation;

namespace SmartCourt.Features.UserVerification.DeleteVerificationDocument
{
    public sealed class DeleteVerificationDocumentCommandValidator : AbstractValidator<DeleteVerificationDocumentCommand>
    {
        public DeleteVerificationDocumentCommandValidator()
        {
            RuleFor(x => x.DocumentId)
                .NotEmpty()
                .WithMessage("Document Id is required.");

            RuleFor(x => x.UserId)
                .NotEmpty()
                .WithMessage("User Id is required.");
        }
    }
}
