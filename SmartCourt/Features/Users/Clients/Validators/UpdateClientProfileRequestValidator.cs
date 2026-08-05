using FluentValidation;
using SmartCourt.Features.Users.Clients.DTOs;
using System;

namespace SmartCourt.Features.Users.Clients.Validators;

public class UpdateClientProfileRequestValidator : AbstractValidator<UpdateClientProfileRequest>
{
    public UpdateClientProfileRequestValidator()
    {
        RuleFor(x => x.PhoneNumber)
            .NotEmpty().WithMessage("رقم الهاتف مطلوب")
            .Matches(@"^\+20\d{10}$").WithMessage("رقم الهاتف يجب أن يكون بالتنسيق المصري +20XXXXXXXXXX");

        RuleFor(x => x.Address)
            .MaximumLength(500).WithMessage("يجب ألا يتجاوز العنوان 500 حرف");
    }
}
