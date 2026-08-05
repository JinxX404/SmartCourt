namespace SmartCourt.Features.Auth.Login.DTOs;

public record LoginResponse(
    UserDto User,
    string AccessToken,
    int ExpiresIn,
    string RefreshToken,
    DateTime RefreshTokenExpiration
);

