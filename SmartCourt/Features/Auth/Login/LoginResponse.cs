namespace SmartCourt.Features.Auth.Login;

public record LoginResponse(
    string Id,
    string Email,
    string FullName,
    string Role,
    string Token,
    int ExpiresIn,
    string RefreshToken,
    DateTime RefreshTokenExpiration
);

