using System.IO;
using System.Linq;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using SmartCourt.Features.DocumentReview.DTOs;

namespace SmartCourt.Features.DocumentReview.Validators;

public class AnalyzeDocumentRequestValidator : AbstractValidator<AnalyzeDocumentRequest>
{
    private readonly string[] _allowedExtensions = { ".pdf", ".doc", ".docx" };

    public AnalyzeDocumentRequestValidator()
    {
        RuleFor(x => x.File)
            .NotNull().WithMessage("File is required.")
            .Must(f => f.Length <= 10 * 1024 * 1024).WithMessage("File size must not exceed 10 MB.")
            .Must(BeValidExtension).WithMessage("Only .pdf, .doc, and .docx files are supported.");

        RuleFor(x => x.Query)
            .NotEmpty().WithMessage("Query is required.")
            .MaximumLength(2000).WithMessage("Query is too long (maximum 2000 characters).");
    }

    private bool BeValidExtension(IFormFile file)
    {
        if (file == null) return false;
        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        return _allowedExtensions.Contains(ext);
    }
}
