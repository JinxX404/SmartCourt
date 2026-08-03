using FluentValidation;
using Microsoft.AspNetCore.Http;
using System.IO;
using System.Linq;

namespace SmartCourt.Features.Case.CreateCase
{
    public class CreateCaseCommandValidator : AbstractValidator<CreateCaseCommand>
    {
        private static readonly string[] AllowedExtensions = [".pdf", ".docx", ".doc"];

        public CreateCaseCommandValidator()
        {
            RuleFor(c => c.Title)
                .NotEmpty().WithMessage("Case title can't be empty");

            RuleFor(c => c.Description)
                .NotEmpty().WithMessage("Case description can't be empty");

            RuleFor(c => c.Documents)
                .Must(documents => documents == null || documents.All(IsSupportedDocument))
                .WithMessage("Only Word (.doc, .docx) and PDF (.pdf) documents are supported.");
        }

        private static bool IsSupportedDocument(IFormFile file)
        {
            if (file == null || string.IsNullOrWhiteSpace(file.FileName))
                return false;

            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            return AllowedExtensions.Contains(ext);
        }
    }
}
