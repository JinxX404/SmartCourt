namespace SmartCourt.Features.Auth.RefreshToken.DTOs;

public record RefreshTokenResponse(
    string AccessToken,
    string RefreshToken,
    DateTime ExpiresAt
);
