using FluentValidation;
using SmartCourt.Features.Case.AddCaseDocument.DTOs;
using System;

namespace SmartCourt.Features.Case.AddCaseDocument.Validators;

public class AddStoredCaseDocumentRequestValidator : AbstractValidator<AddStoredCaseDocumentRequest>
{
    public AddStoredCaseDocumentRequestValidator()
    {
        RuleFor(x => x.StoredFileId)
            .NotEmpty().WithMessage("StoredFileId is required.")
            .NotEqual(Guid.Empty).WithMessage("StoredFileId cannot be empty.");
    }
}
