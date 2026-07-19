using FluentValidation;
using SmartCourt.Features.Users.Shared.DTOs;

namespace SmartCourt.Features.Users.Shared.Validators;

public sealed class DeleteAccountRequestValidator : AbstractValidator<DeleteAccountRequest>
{
    public DeleteAccountRequestValidator()
    {
        RuleFor(request => request.CurrentPassword)
            .NotEmpty()
            .WithMessage("كلمة المرور الحالية مطلوبة.");
    }
}
