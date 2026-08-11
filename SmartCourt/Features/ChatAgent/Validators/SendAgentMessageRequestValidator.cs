using FluentValidation;
using SmartCourt.Features.ChatAgent.DTOs;

namespace SmartCourt.Features.ChatAgent.Validators;

public sealed class SendAgentMessageRequestValidator : AbstractValidator<SendAgentMessageRequest>
{
    public SendAgentMessageRequestValidator()
    {
        RuleFor(x => x.Content)
            .NotEmpty()
            .WithMessage("محتوى الرسالة مطلوب.")
            .MaximumLength(2000)
            .WithMessage("يجب ألا يتجاوز محتوى الرسالة 2000 حرف.");
    }
}
