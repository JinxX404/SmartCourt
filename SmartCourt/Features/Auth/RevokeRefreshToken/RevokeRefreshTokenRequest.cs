namespace SmartCourt.Features.Auth.RevokeRefreshToken;

public record RevokeRefreshTokenRequest(
    string Token,
    string RefreshToken
);
