using FluentValidation;
using SmartCourt.Features.Users.Clients.DTOs;
using System;

namespace SmartCourt.Features.Users.Clients.Validators;

public class CompleteClientProfileRequestValidator : AbstractValidator<CompleteClientProfileRequest>
{
    public CompleteClientProfileRequestValidator()
    {
        RuleFor(x => x.PhoneNumber)
            .NotEmpty().WithMessage("رقم الهاتف مطلوب")
            .Matches(@"^\+20\d{10}$").WithMessage("رقم الهاتف يجب أن يكون بالتنسيق المصري +20XXXXXXXXXX");

        RuleFor(x => x.NationalNumber)
            .NotEmpty()
            .Matches(@"^\d{14}$")
            .WithMessage("الرقم القومي يجب أن يتكون من 14 رقم بالضبط.");

        RuleFor(x => x.DateOfBirth)
            .NotEmpty().WithMessage("تاريخ الميلاد مطلوب")
            .LessThanOrEqualTo(DateOnly.FromDateTime(DateTimeOffset.UtcNow.Date).AddYears(-21)).WithMessage("يجب أن يكون عمر المستخدم 21 عاماً أو أكثر.");

        RuleFor(x => x.Gender)
            .NotNull().WithMessage("الجنس مطلوب")
            .IsInEnum().WithMessage("الجنس يجب أن يكون صالحاً");

        RuleFor(x => x.Address)
            .MaximumLength(500).WithMessage("يجب ألا يتجاوز العنوان 500 حرف");
    }
}
