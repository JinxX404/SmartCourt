using SmartCourt.Features.Auth.RevokeRefreshToken.DTOs;
using SmartCourt.Common.Entities;
using FluentValidation;

namespace SmartCourt.Features.Auth.RevokeRefreshToken.Validators;

public class RevokeRefreshTokenRequestValidator : AbstractValidator<RevokeRefreshTokenRequest>
{
    public RevokeRefreshTokenRequestValidator()
    {
        // Rules removed: Token and RefreshToken can be sent via cookies or headers.
        // Validation for presence is handled in the controller.
    }
}
