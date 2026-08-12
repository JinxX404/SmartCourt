using FluentValidation;
using Microsoft.AspNetCore.Http;
using SmartCourt.Features.Case.AddCaseDocument.DTOs;
using System.IO;
using System.Linq;

namespace SmartCourt.Features.Case.AddCaseDocument.Validators;

public class AddCaseDocumentRequestValidator : AbstractValidator<AddCaseDocumentRequest>
{
    private static readonly string[] AllowedExtensions = [".pdf", ".docx", ".doc"];

    public AddCaseDocumentRequestValidator()
    {
        RuleFor(x => x.Documents)
            .NotNull().WithMessage("At least one document is required.")
            .Must(docs => docs != null && docs.Count > 0).WithMessage("At least one document must be uploaded.")
            .Must(docs => docs == null || docs.All(IsSupportedDocument))
            .WithMessage("Only Word (.doc, .docx) and PDF (.pdf) documents are supported.");
    }

    private static bool IsSupportedDocument(IFormFile file)
    {
        if (file == null || string.IsNullOrWhiteSpace(file.FileName) || file.Length == 0)
            return false;

        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        return AllowedExtensions.Contains(ext);
    }
}
