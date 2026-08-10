using FluentValidation;
using SmartCourt.Features.ChatAgent.DTOs;

namespace SmartCourt.Features.ChatAgent.Validators;

public sealed class CreateAgentConversationRequestValidator : AbstractValidator<CreateAgentConversationRequest>
{
    public CreateAgentConversationRequestValidator()
    {
        RuleFor(x => x.CaseId)
            .Must(id => !id.HasValue || id.Value != Guid.Empty)
            .WithMessage("معرف القضية غير صالح.");
    }
}
