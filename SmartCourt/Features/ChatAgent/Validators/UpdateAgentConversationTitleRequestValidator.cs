using FluentValidation;
using SmartCourt.Features.ChatAgent.DTOs;

namespace SmartCourt.Features.ChatAgent.Validators;

public sealed class UpdateAgentConversationTitleRequestValidator : AbstractValidator<UpdateAgentConversationTitleRequest>
{
    public UpdateAgentConversationTitleRequestValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty()
            .WithMessage("عنوان المحادثة مطلوب.")
            .MaximumLength(200)
            .WithMessage("يجب ألا يتجاوز عنوان المحادثة 200 حرف.");
    }
}
