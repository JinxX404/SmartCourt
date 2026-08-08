using FluentValidation;
using SmartCourt.Features.Users.Clients.DTOs;
using System;

namespace SmartCourt.Features.Users.Clients.Validators;

public class UpdateClientProfileRequestValidator : AbstractValidator<UpdateClientProfileRequest>
{
    public UpdateClientProfileRequestValidator()
    {

        RuleFor(x => x.NationalNumber)
            .NotEmpty()
            .Matches(@"^\d{14}$")
            .WithMessage("الرقم القومي يجب أن يتكون من 14 رقم بالضبط.");

        RuleFor(x => x.Address)
            .MaximumLength(500).WithMessage("يجب ألا يتجاوز العنوان 500 حرف");
    }
}
