namespace SmartCourt.Features.Auth.RefreshToken;

public record RefreshTokenRequest(
    string Token,
    string RefreshToken
);
