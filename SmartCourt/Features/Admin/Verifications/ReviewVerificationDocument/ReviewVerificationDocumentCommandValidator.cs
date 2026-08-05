using FluentValidation;

namespace SmartCourt.Features.Admin.Verifications.ReviewVerificationDocument;

public sealed class ReviewVerificationDocumentCommandValidator : AbstractValidator<ReviewVerificationDocumentCommand>
{
    public ReviewVerificationDocumentCommandValidator()
    {
        RuleFor(command => command.DocumentId)
            .NotEmpty();

        RuleFor(command => command.Decision)
            .IsInEnum();

        When(command => command.Decision == VerificationReviewDecision.Reject, () =>
        {
            RuleFor(command => command.RejectionReason)
                .NotEmpty()
                .MaximumLength(500);
        });

        When(command => command.Decision == VerificationReviewDecision.Approve, () =>
        {
            RuleFor(command => command.RejectionReason)
                .Empty()
                .WithMessage("A rejection reason can only be supplied when rejecting a document.");
        });
    }
}
