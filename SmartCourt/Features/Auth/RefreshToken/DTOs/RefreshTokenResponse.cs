using System;

namespace SmartCourt.Features.Auth.RefreshToken.DTOs;

public record RefreshTokenResponse(
    string AccessToken,
    int AccessTokenExpiresInSeconds,
    string RefreshToken,
    DateTime ExpiresAt);
