using SmartCourt.Features.Auth.RefreshToken.DTOs;
using SmartCourt.Common.Entities;
using FluentValidation;

namespace SmartCourt.Features.Auth.RefreshToken.Validators;

public class RefreshTokenRequestValidator : AbstractValidator<RefreshTokenRequest>
{
    public RefreshTokenRequestValidator()
    {
        // Rule removed: The refresh token can be in the body OR in the HttpOnly cookie.
        // Validation for its presence is done in the controller.
    }
}
