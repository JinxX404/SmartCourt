using FluentValidation;
using SmartCourt.Features.Contracts.Files;

namespace SmartCourt.Features.Contracts.Validators;

public sealed class UploadContractFilesRequestValidator
    : AbstractValidator<UploadContractFilesRequest>
{
    public UploadContractFilesRequestValidator()
    {
        RuleFor(request => request.Files)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .Must(files => files.Count is >= 1
                and <= ContractFileUploadPolicy.MaximumFileCount)
            .WithMessage(
                $"Attach between 1 and {ContractFileUploadPolicy.MaximumFileCount} files.")
            .Must(files => files.Sum(file => file?.Length ?? 0)
                <= ContractFileUploadPolicy.MaximumRequestSizeBytes)
            .WithMessage("The combined attachment size cannot exceed 25 MB.");

        RuleForEach(request => request.Files)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .Must(file => file.Length > 0)
            .WithMessage("Empty files are not allowed.")
            .Must(file => file.Length
                <= ContractFileUploadPolicy.MaximumFileSizeBytes)
            .WithMessage("Each attachment cannot exceed 10 MB.")
            .Must(file => Path.GetFileName(file.FileName).Length
                <= ContractFileUploadPolicy.MaximumFileNameLength)
            .WithMessage("Attachment filenames cannot exceed 255 characters.");
    }
}
