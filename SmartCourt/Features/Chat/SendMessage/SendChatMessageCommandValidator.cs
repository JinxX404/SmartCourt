using FluentValidation;
using SmartCourt.Features.Chat.Entities;

namespace SmartCourt.Features.Chat.SendMessage;

public sealed class SendChatMessageCommandValidator
    : AbstractValidator<SendChatMessageCommand>
{
    public SendChatMessageCommandValidator()
    {
        RuleFor(command => command.ConversationId)
            .NotEmpty();
        RuleFor(command => command.Content)
            .NotEmpty()
            .MaximumLength(ChatMessage.MaximumContentLength);
    }
}
