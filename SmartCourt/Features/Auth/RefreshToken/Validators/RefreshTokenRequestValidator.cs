using SmartCourt.Features.Auth.RefreshToken.DTOs;
using SmartCourt.Common.Entities;
using FluentValidation;

namespace SmartCourt.Features.Auth.RefreshToken.Validators;

public class RefreshTokenRequestValidator : AbstractValidator<RefreshTokenRequest>
{
    public RefreshTokenRequestValidator()
    {
        RuleFor(x => x.RefreshToken)
            .NotEmpty()
            .WithMessage("Refresh token is required.");
    }
}
