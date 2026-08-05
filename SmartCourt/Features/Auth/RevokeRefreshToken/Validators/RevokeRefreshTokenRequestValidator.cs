using SmartCourt.Features.Auth.RevokeRefreshToken.DTOs;
using SmartCourt.Common.Entities;
using FluentValidation;

namespace SmartCourt.Features.Auth.RevokeRefreshToken.Validators;

public class RevokeRefreshTokenRequestValidator : AbstractValidator<RevokeRefreshTokenRequest>
{
    public RevokeRefreshTokenRequestValidator()
    {
        RuleFor(x => x.Token)
            .NotEmpty()
            .WithMessage("رمز الوصول مطلوب.");

        RuleFor(x => x.RefreshToken)
            .NotEmpty()
            .WithMessage("رمز التحديث مطلوب.");
    }
}
