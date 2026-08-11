using FluentValidation;
using SmartCourt.Features.Chat.Entities;

namespace SmartCourt.Features.Chat.Attachments;

public sealed class SendChatAttachmentsCommandValidator
    : AbstractValidator<SendChatAttachmentsCommand>
{
    public SendChatAttachmentsCommandValidator()
    {
        RuleFor(command => command.ConversationId).NotEmpty();
        RuleFor(command => command.Caption)
            .MaximumLength(ChatMessage.MaximumContentLength);
        RuleFor(command => command.Files)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .Must(files => files.Count is >= 1 and <= ChatAttachmentPolicy.MaximumFileCount)
            .WithMessage(
                $"Attach between 1 and {ChatAttachmentPolicy.MaximumFileCount} files.")
            .Must(files => files.Sum(file => file?.Length ?? 0)
                <= ChatAttachmentPolicy.MaximumRequestSizeBytes)
            .WithMessage("The combined attachment size cannot exceed 25 MB.");
        RuleForEach(command => command.Files)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .Must(file => file.Length > 0)
            .WithMessage("Empty files are not allowed.")
            .Must(file => file.Length <= ChatAttachmentPolicy.MaximumFileSizeBytes)
            .WithMessage("Each attachment cannot exceed 10 MB.")
            .Must(file => Path.GetFileName(file.FileName).Length
                <= ChatAttachmentPolicy.MaximumFileNameLength)
            .WithMessage("Attachment filenames cannot exceed 255 characters.");
    }
}
