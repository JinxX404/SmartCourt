namespace SmartCourt.Features.Auth.RefreshToken.DTOs;

public record RefreshTokenRequest(
    string Token,
    string RefreshToken
);
