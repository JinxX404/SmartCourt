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
            .Matches(@"^(\+20|0)1[0125]\d{8}$").WithMessage("رقم الهاتف يجب أن يكون رقم مصري صحيح (مثال: 011xxxxxxxx أو +2011xxxxxxxx)");

        RuleFor(x => x.NationalNumber)
            .NotEmpty()
            .Matches(@"^\d{14}$")
            .WithMessage("الرقم القومي يجب أن يتكون من 14 رقم بالضبط.");

        RuleFor(x => x.Address)
            .MaximumLength(500).WithMessage("يجب ألا يتجاوز العنوان 500 حرف");
    }
}
