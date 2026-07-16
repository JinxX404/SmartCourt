using FluentValidation;
using SmartCourt.Features.Users.Clients.DTOs;

namespace SmartCourt.Features.Users.Clients.Validators;

public class UpdateClientProfileRequestValidator : AbstractValidator<UpdateClientProfileRequest>
{
    public UpdateClientProfileRequestValidator()
    {
        /*
         * ALGORITHM:
         * 1. RuleFor Email: NotEmpty, EmailAddress
         * 2. RuleFor PhoneNumber: NotEmpty, Matches specific format
         * 3. RuleFor DateOfBirth: NotEmpty, Must be in the past
         * 4. RuleFor Address: MaximumLength
         */
    }
}
