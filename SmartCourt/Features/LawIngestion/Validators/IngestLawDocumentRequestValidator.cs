using FluentValidation;
using SmartCourt.Features.LawIngestion.DTOs;

namespace SmartCourt.Features.LawIngestion.Validators;

public class IngestLawDocumentRequestValidator : AbstractValidator<IngestLawDocumentRequest>
{
    public IngestLawDocumentRequestValidator()
    {
        RuleFor(x => x.FilePath)
            .NotEmpty()
            .WithMessage("FilePath is required.");

        RuleFor(x => x.DocumentTitle)
            .NotEmpty()
            .WithMessage("DocumentTitle is required.");

        RuleFor(x => x.Language)
            .NotEmpty()
            .Must(lang => lang == "ar" || lang == "en")
            .WithMessage("Language must be either 'ar' or 'en'.");
    }
}
